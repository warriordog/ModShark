using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using ModShark.Reports;
using ModShark.Utils;
using SharkeyDB;
using SharkeyDB.Entities;

namespace ModShark.Rules;

public interface IFlaggedUserRule : IRule;

[PublicAPI]
public class FlaggedUserConfig
{
    public bool Enabled { get; set; }
    public bool IncludeLocal { get; set; }
    public bool IncludeRemote { get; set; }
    public bool IncludeDeleted { get; set; }
    public bool IncludeSuspended { get; set; }
    public bool IncludeSilenced { get; set; }
    public List<string> UsernamePatterns { get; set; } = [];
    public int Timeout { get; set; }
}

public class FlaggedUserRule(ILogger<FlaggedUserRule> logger, FlaggedUserConfig config, SharkeyContext db) : IFlaggedUserRule
{
    // Merge and pre-compile the pattern for efficiency
    private Regex UsernamePattern { get; } = PatternUtils.CreateMatcher(config.UsernamePatterns, config.Timeout);
    
    public async Task RunRule(Report report, CancellationToken stoppingToken)
    {
        if (!config.Enabled)
        {
            logger.LogDebug("Skipping run, rule is disabled");
            return;
        }

        if (config.UsernamePatterns.Count < 1)
        {
            logger.LogWarning("Skipping run, no patterns defined");
            return;
        }

        if (!config.IncludeLocal && !config.IncludeRemote)
        {
            logger.LogWarning("Skipping run, all users are excluded (local & remote)");
            return;
        }
        
        // Find all new flagged users
        await FlagNewUsers(report, stoppingToken);
    }

    private async Task FlagNewUsers(Report report, CancellationToken stoppingToken)
    {
        // Cap at this number to ensure that we don't mistakenly clobber new inserts.
        // This is nullable to prevent System.InvalidOperationException - https://stackoverflow.com/a/54117075
        var maxId = await db.MSQueuedUsers
            .MaxAsync(q => (int?)q.Id, stoppingToken);

        // Stop early if the queue is empty
        if (maxId is null or < 1)
        {
            logger.LogDebug("Nothing to do, user queue is empty");
            return;
        }
        
        // Query for all new users that match the given flags
        var query =
            from q in db.MSQueuedUsers.AsNoTracking()
            join u in db.Users.AsNoTracking()
                on q.UserId equals u.Id
            where
                q.Id <= maxId
                && (config.IncludeLocal || u.Host != null)
                && (config.IncludeRemote || u.Host == null)
                && (config.IncludeSuspended || !u.IsSuspended)
                && (config.IncludeSilenced || !u.IsSilenced)
                && (config.IncludeDeleted || !u.IsDeleted)
                && !db.MSFlaggedUsers.Any(f => f.UserId == q.UserId)
            orderby q.Id
            select u;
        
        // Stream each result due to potentially large size
        var newUsers = query.AsAsyncEnumerable();
        await foreach (var user in newUsers)
        {
            // For better use of database resources, we handle pattern matching in application code.
            // This also gives us .NET's faster and more powerful regex engine.
            if (!UsernamePattern.IsMatch(user.UsernameLower))
                continue;
            
            report.UserReports.Add(new UserReport
            {
                UserId = user.Id,
                Username = user.Username,
                Hostname = user.Host
            });

            db.MSFlaggedUsers.Add(new MSFlaggedUser
            {
                UserId = user.Id,
                FlaggedAt = report.ReportDate
            });
        }
        
        // Delete all processed queue items
        var numChecked = await db.MSQueuedUsers
            .Where(q => q.Id <= maxId)
            .ExecuteDeleteAsync(stoppingToken);
        logger.LogDebug("Checked {numChecked} new users", numChecked);
        
        
        // Save changes
        await db.SaveChangesAsync(stoppingToken);
    }
}