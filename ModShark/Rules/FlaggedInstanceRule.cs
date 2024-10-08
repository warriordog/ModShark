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
            var flags = new ReportFlags();
            if (!IsFlagged(instance, flags))
                continue;
            
            report.InstanceReports.Add(new InstanceReport
            {
                Instance = instance,
                Flags = flags
            });

            db.MSFlaggedInstances.Add(new MSFlaggedInstance
            {
                InstanceId = instance.Id,
                FlaggedAt = report.ReportDate
            });
        }
    }

    private bool IsFlagged(Instance instance, ReportFlags flags)
    {
        var isFlagged = false;
        isFlagged |= FlagName(instance, flags);
        isFlagged |= FlagHostname(instance, flags);
        isFlagged |= FlagDescription(instance, flags);
        isFlagged |= FlagContact(instance, flags);
        isFlagged |= FlagSoftware(instance, flags);
        return isFlagged;
    }

    private bool FlagName(Instance instance, ReportFlags flags)
    {
        if (!HasNamePatterns)
            return false;

        if (!instance.HasName)
            return false;

        return flags.TryAddPattern(NamePattern, instance.Name, "name");
    }

    private bool FlagHostname(Instance instance, ReportFlags flags)
    {
        if (!HasHostnamePatterns)
            return false;


        return flags.TryAddPattern(HostnamePattern, instance.Host, "host");
    }

    private bool FlagDescription(Instance instance, ReportFlags flags)
    {
        if (!HasDescriptionPatterns)
            return false;

        if (!instance.HasDescription)
            return false;
        
        return flags.TryAddPattern(DescriptionPattern, instance.Description, "description");
    }

    private bool FlagContact(Instance instance, ReportFlags flags)
    {
        if (!HasContactPatterns)
            return false;

        var nameFlagged = instance.HasMaintainerName && flags.TryAddPattern(ContactPattern, instance.MaintainerName, "maintainer");
        var emailFlagged = instance.HasMaintainerEmail && flags.TryAddPattern(ContactPattern, instance.MaintainerEmail, "maintainer");
        
        return nameFlagged || emailFlagged;
    }

    private bool FlagSoftware(Instance instance, ReportFlags flags)
    {
        if (!HasSoftwarePatterns)
            return false;

        if (!instance.HasSoftwareName && !instance.HasSoftwareVersion)
            return false;

        return flags.TryAddPattern(SoftwarePattern, instance.GetSoftwareString(), "software");
    }
}