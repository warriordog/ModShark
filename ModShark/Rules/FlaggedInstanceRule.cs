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
public class FlaggedInstanceConfig : RuleConfig
{
    public bool IncludeSuspended { get; set; }
    public bool IncludeSilenced { get; set; }
    public bool IncludeBlocked { get; set; }
    
    public List<string> HostnamePatterns { get; set; } = [];
    public int Timeout { get; set; }
}

public class FlaggedInstanceRule(ILogger<FlaggedInstanceRule> logger, FlaggedInstanceConfig config, SharkeyContext db, IMetaService metaService) : QueuedRule<MSQueuedInstance>(logger, config, db, db.MSQueuedInstances), IFlaggedInstanceRule
{
    // Merge and pre-compile the pattern for efficiency
    private Regex HostnamePattern { get; } = PatternUtils.CreateMatcher(config.HostnamePatterns, config.Timeout, true);

    public override async Task RunRule(Report report, CancellationToken stoppingToken)
    {
        if (config.HostnamePatterns.Count < 1)
        {
            logger.LogWarning("Skipping run, no patterns defined");
            return;
        }

        await base.RunRule(report, stoppingToken);
    }

    protected override async Task RunQueuedRule(Report report, int maxId, CancellationToken stoppingToken)
    {
        // Get the list of blocked / silenced instances from metadata
        var meta = await metaService.GetInstanceMeta(stoppingToken);
        
        // Query for all new instances that match the given flags
        var query =
            from q in db.MSQueuedInstances.AsNoTracking()
            join i in db.Instances.AsNoTracking()
                on q.InstanceId equals i.Id
            where
                q.Id <= maxId
                && (config.IncludeSuspended || i.SuspensionState == "none")
                && (config.IncludeBlocked || !meta.BlockedHosts.Contains(i.Host))
                && (config.IncludeSilenced || !meta.SilencedHosts.Contains(i.Host))
                && !db.MSFlaggedInstances.Any(f => f.InstanceId == q.InstanceId)
            orderby q.Id
            select i;
        
        // Stream each result due to potentially large size
        var newInstances = query.AsAsyncEnumerable();
        await foreach (var instance in newInstances)
        {
            // The query only excludes exact matches, so check for base domains here
            if (!config.IncludeBlocked && HostUtils.Matches(instance.Host, meta.BlockedHosts))
                continue;
            if (!config.IncludeSilenced && HostUtils.Matches(instance.Host, meta.SilencedHosts))
                continue;
            
            // For better use of database resources, we handle pattern matching in application code.
            // This also gives us .NET's faster and more powerful regex engine.
            if (!HostnamePattern.IsMatch(instance.Host))
                continue;
            
            report.InstanceReports.Add(new InstanceReport
            {
                InstanceId = instance.Id,
                Hostname = instance.Host
            });

            db.MSFlaggedInstances.Add(new MSFlaggedInstance
            {
                InstanceId = instance.Id,
                FlaggedAt = report.ReportDate
            });
        }
    }
}