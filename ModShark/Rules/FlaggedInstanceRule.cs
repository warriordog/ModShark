using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using ModShark.Reports;
using ModShark.Services;
using ModShark.Utils;
using SharkeyDB;
using SharkeyDB.Entities;

namespace ModShark.Rules;

public interface IFlaggedInstanceRule : IRule;

[PublicAPI]
public class FlaggedInstanceConfig : QueuedRuleConfig
{
    public bool IncludeSuspended { get; set; }
    public bool IncludeSilenced { get; set; }
    public bool IncludeBlocked { get; set; }
    
    public List<string> NamePatterns { get; set; } = [];
    public List<string> HostnamePatterns { get; set; } = [];
    public List<string> DescriptionPatterns { get; set; } = [];
    public List<string> ContactPatterns { get; set; } = [];
    public List<string> SoftwarePatterns { get; set; } = [];
    public int Timeout { get; set; }
}

public class FlaggedInstanceRule(ILogger<FlaggedInstanceRule> logger, FlaggedInstanceConfig config, SharkeyContext db, IMetaService metaService) : QueuedRule<MSQueuedInstance>(logger, config, db, db.MSQueuedInstances), IFlaggedInstanceRule
{
    // Merge and pre-compile the patterns for efficiency
    private Regex NamePattern { get; } = PatternUtils.CreateMatcher(config.NamePatterns, config.Timeout, ignoreCase: true);
    private Regex HostnamePattern { get; } = PatternUtils.CreateMatcher(config.HostnamePatterns, config.Timeout, ignoreCase: true);
    private Regex DescriptionPattern { get; } = PatternUtils.CreateMatcher(config.DescriptionPatterns, config.Timeout, ignoreCase: true);
    private Regex ContactPattern { get; } = PatternUtils.CreateMatcher(config.ContactPatterns, config.Timeout, ignoreCase: true);
    private Regex SoftwarePattern { get; } = PatternUtils.CreateMatcher(config.SoftwarePatterns, config.Timeout, ignoreCase: true);
    

    private bool HasNamePatterns => config.NamePatterns.Count > 0;
    private bool HasHostnamePatterns => config.HostnamePatterns.Count > 0;
    private bool HasDescriptionPatterns => config.DescriptionPatterns.Count > 0;
    private bool HasContactPatterns => config.ContactPatterns.Count > 0;
    private bool HasSoftwarePatterns => config.SoftwarePatterns.Count > 0;

    protected override Task<bool> CanRun(CancellationToken stoppingToken)
    {
        if (!HasNamePatterns && !HasHostnamePatterns && !HasDescriptionPatterns && !HasContactPatterns && !HasSoftwarePatterns)
        {
            logger.LogWarning("Skipping run, no patterns defined");
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    protected override async Task RunQueuedRule(Report report, int maxId, CancellationToken stoppingToken)
    {
        // Get the list of blocked / silenced instances from metadata
        var meta = await metaService.GetInstanceMeta(stoppingToken);
        
        // Query for all new instances that match the given flags
        var newInstances = db.MSQueuedInstances
            .AsNoTracking()
            .Include(q => q.FlaggedInstance)
            .Include(q => q.Instance!) // database constraints ensure that "Instance" cannot be null
            .Where(q =>
                q.Id <= maxId
                && (config.IncludeSuspended || q.Instance!.SuspensionState == "none")
                && (config.IncludeBlocked || !meta.BlockedHosts.Contains(q.Instance!.Host))
                && (config.IncludeSilenced || !meta.SilencedHosts.Contains(q.Instance!.Host))
                && !db.MSFlaggedInstances.Any(f => f.InstanceId == q.InstanceId))
            .OrderBy(q => q.Id)
            .Select(q => q.Instance!)
            .AsAsyncEnumerable();
        
        // Stream each result due to potentially large size
        await foreach (var instance in newInstances)
        {
            // The query only excludes exact matches, so check for base domains here
            if (!config.IncludeBlocked && HostUtils.Matches(instance.Host, meta.BlockedHosts))
                continue;
            if (!config.IncludeSilenced && HostUtils.Matches(instance.Host, meta.SilencedHosts))
                continue;
            
            // For better use of database resources, we handle pattern matching in application code.
            // This also gives us .NET's faster and more powerful regex engine.
            if (!IsFlagged(instance))
                continue;
            
            report.InstanceReports.Add(new InstanceReport
            {
                Instance = instance
            });

            db.MSFlaggedInstances.Add(new MSFlaggedInstance
            {
                InstanceId = instance.Id,
                FlaggedAt = report.ReportDate
            });
        }
    }

    private bool IsFlagged(Instance instance)
        => HasFlaggedName(instance)
           || HasFlaggedHostname(instance)
           || HasFlaggedDescription(instance)
           || HasFlaggedContact(instance)
           || HasFlaggedSoftware(instance);

    private bool HasFlaggedName(Instance instance)
    {
        if (!HasNamePatterns)
            return false;

        if (!instance.HasName)
            return false;

        return NamePattern.IsMatch(instance.Name);
    }

    private bool HasFlaggedHostname(Instance instance)
    {
        if (!HasHostnamePatterns)
            return false;

        return HostnamePattern.IsMatch(instance.Host);
    }

    private bool HasFlaggedDescription(Instance instance)
    {
        if (!HasDescriptionPatterns)
            return false;

        if (!instance.HasDescription)
            return false;

        return DescriptionPattern.IsMatch(instance.Description);
    }

    private bool HasFlaggedContact(Instance instance)
    {
        if (!HasContactPatterns)
            return false;

        if (instance.HasMaintainerName && ContactPattern.IsMatch(instance.MaintainerName))
            return true;

        if (instance.HasMaintainerEmail && ContactPattern.IsMatch(instance.MaintainerEmail))
            return true;

        return false;
    }

    private bool HasFlaggedSoftware(Instance instance)
    {
        if (!HasSoftwarePatterns)
            return false;

        if (!instance.HasSoftwareName && !instance.HasSoftwareVersion)
            return false;

        var versionString = instance.GetSoftwareString();
        return SoftwarePattern.IsMatch(versionString);
    }
}