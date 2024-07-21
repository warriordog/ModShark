using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using ModShark.Reports;
using ModShark.Services;
using ModShark.Utils;
using SharkeyDB;
using SharkeyDB.Entities;

namespace ModShark.Rules;

public interface IFlaggedUserRule : IRule;

[PublicAPI]
public class FlaggedUserConfig : QueuedRuleConfig
{
    public bool IncludeLocal { get; set; }
    public bool IncludeRemote { get; set; }
    
    public bool IncludeDeleted { get; set; }
    public bool IncludeSuspended { get; set; }
    public bool IncludeSilenced { get; set; }
    
    public bool IncludeBlockedInstance { get; set; }
    public bool IncludeSilencedInstance { get; set; }
    
    public List<string> UsernamePatterns { get; set; } = [];
    public List<string> DisplayNamePatterns { get; set; } = [];
    public List<string> BioPatterns { get; set; } = []; 
    public int Timeout { get; set; }
}

public class FlaggedUserRule(ILogger<FlaggedUserRule> logger, FlaggedUserConfig config, SharkeyContext db, IMetaService metaService) : QueuedRule<MSQueuedUser>(logger, config, db, db.MSQueuedUsers), IFlaggedUserRule
{
    // Merge and pre-compile the patterns for efficiency
    private Regex UsernamePattern { get; } = PatternUtils.CreateMatcher(config.UsernamePatterns, config.Timeout, ignoreCase: true);
    private Regex DisplayNamePattern { get; } = PatternUtils.CreateMatcher(config.DisplayNamePatterns, config.Timeout, ignoreCase: true);
    private Regex BioPattern { get; } = PatternUtils.CreateMatcher(config.BioPatterns, config.Timeout, ignoreCase: true);

    private bool HasUsernamePatterns => config.UsernamePatterns.Count > 0;
    private bool HasDisplayNamePatterns => config.DisplayNamePatterns.Count > 0;
    private bool HasBioPatterns => config.BioPatterns.Count > 0;
    
    protected override Task<bool> CanRun(CancellationToken stoppingToken)
    {
        if (!HasUsernamePatterns && !HasDisplayNamePatterns && !HasBioPatterns)
        {
            logger.LogWarning("Skipping run, no patterns defined");
            return Task.FromResult(false);
        }

        if (!config.IncludeLocal && !config.IncludeRemote)
        {
            logger.LogWarning("Skipping run, all users are excluded (local & remote)");
            return Task.FromResult(false);
        }

        if (!config.IncludeRemote && config.IncludeBlockedInstance)
        {
            logger.LogWarning($"Configuration error: {nameof(FlaggedUserConfig.IncludeBlockedInstance)} has no effect when {nameof(FlaggedUserConfig.IncludeRemote)} is false");
        }

        if (!config.IncludeRemote && config.IncludeSilencedInstance)
        {
            logger.LogWarning($"Configuration error: {nameof(FlaggedUserConfig.IncludeSilencedInstance)} has no effect when {nameof(FlaggedUserConfig.IncludeRemote)} is false");
        }

        return Task.FromResult(true);
    }

    protected override async Task RunQueuedRule(Report report, int maxId, CancellationToken stoppingToken)
    {
        // Get the list of blocked / silenced instances from metadata
        var meta = await metaService.GetInstanceMeta(stoppingToken);
        
        // Query for all new users that match the given flags
        var newUsers = db.MSQueuedUsers
            .AsNoTracking()
            .Include(q => q.FlaggedUser)
            .Include(q => q.User!)  // database constraints ensure that "User" cannot be null
                .ThenInclude(u => u.Instance)
            .Include(q => q.User!)  // database constraints ensure that "User" cannot be null
                .ThenInclude(u => u.Profile)
            .Where(q => 
                q.Id <= maxId
                && (config.IncludeLocal || q.User!.Host != null)
                && (config.IncludeRemote || q.User!.Host == null)
                && (config.IncludeSuspended || !q.User!.IsSuspended)
                && (config.IncludeSilenced || !q.User!.IsSilenced)
                && (config.IncludeDeleted || !q.User!.IsDeleted)
                && (config.IncludeBlockedInstance || q.User!.Host == null || !meta.BlockedHosts.Contains(q.User!.Host))
                && (config.IncludeSilencedInstance || q.User!.Host == null || !meta.SilencedHosts.Contains(q.User!.Host))
                && !db.MSFlaggedUsers.Any(f => f.UserId == q.UserId))
            .OrderBy(q => q.Id)
            .Select(q => q.User!)
            .AsAsyncEnumerable();
        
        // Stream each result due to potentially large size
        await foreach (var user in newUsers)
        {
            // Check for base domain and alternate-case matches.
            // This cannot be done efficiently in-database.
            if (user.Host != null)
            {
                // The query only excludes exact matches, so check for base domains here
                if (!config.IncludeBlockedInstance && HostUtils.Matches(user.Host, meta.BlockedHosts))
                    continue;
                if (!config.IncludeSilencedInstance && HostUtils.Matches(user.Host, meta.SilencedHosts))
                    continue;
            }

            // For better use of database resources, we handle pattern matching in application code.
            // This also gives us .NET's faster and more powerful regex engine.
            if (!HasFlaggedUsername(user) && !HasFlaggedDisplayName(user) && !HasFlaggedBio(user))
                continue;
            
            report.UserReports.Add(new UserReport
            {
                Instance = user.Instance,
                User = user
            });

            db.MSFlaggedUsers.Add(new MSFlaggedUser
            {
                UserId = user.Id,
                FlaggedAt = report.ReportDate
            });
        }
    }

    private bool HasFlaggedUsername(User user)
    {
        if (!HasUsernamePatterns)
            return false;
        
        return UsernamePattern.IsMatch(user.UsernameLower);
    }

    private bool HasFlaggedDisplayName(User user)
    {
        if (!HasDisplayNamePatterns)
            return false;

        if (!user.HasName)
            return false;

        return DisplayNamePattern.IsMatch(user.Name);
    }

    private bool HasFlaggedBio(User user)
    {
        if (!HasBioPatterns)
            return false;

        if (!user.HasProfile)
            return false;
        
        if (!user.Profile.HasDescription)
            return false;

        return BioPattern.IsMatch(user.Profile.Description);
    }
}