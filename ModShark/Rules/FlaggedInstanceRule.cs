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
public class FlaggedInstanceConfig
{
    public bool Enabled { get; set; }
    
    public bool IncludeSuspended { get; set; }
    public bool IncludeSilenced { get; set; }
    public bool IncludeBlocked { get; set; }
    
    public List<string> HostnamePatterns { get; set; } = [];
    public int Timeout { get; set; }
}

public class FlaggedInstanceRule(ILogger<FlaggedInstanceRule> logger, FlaggedInstanceConfig config, SharkeyContext db, IMetaService metaService) : IFlaggedInstanceRule
{
    // Merge and pre-compile the pattern for efficiency
    private Regex HostnamePattern { get; } = PatternUtils.CreateMatcher(config.HostnamePatterns, config.Timeout, true);
    
    public async Task RunRule(Report report, CancellationToken stoppingToken)
    {
        if (!config.Enabled)
        {
            logger.LogDebug("Skipping run, rule is disabled");
            return;
        }

        if (config.HostnamePatterns.Count < 1)
        {
            logger.LogWarning("Skipping run, no patterns defined");
            return;
        }
        
        // Find all new flagged instances
        await FlagNewInstances(report, stoppingToken);
    }
    
    private async Task FlagNewInstances(Report report, CancellationToken stoppingToken)
    {
        // Cap at this number to ensure that we don't mistakenly clobber new inserts.
        // This is nullable to prevent System.InvalidOperationException - https://stackoverflow.com/a/54117075
        var maxId = await db.MSQueuedInstances
            .MaxAsync(q => (int?)q.Id, stoppingToken);

        // Stop early if the queue is empty
        if (maxId is null or < 1)
        {
            logger.LogDebug("Nothing to do, instance queue is empty");
            return;
        }
        
        // Get the list of blocked / silenced instances from metadata
        var meta = await metaService.GetInstanceMeta(stoppingToken);
        var extendedBlockedHosts = meta.BlockedHosts.Select(h => $".{h.ToLower()}").ToList();
        var extendedSilencedHosts = meta.SilencedHosts.Select(h => $".{h.ToLower()}").ToList();
        
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
            // Check for base domain and alternate-case matches.
            // This cannot be done efficiently in-database.
            var lowerHost = instance.Host.ToLower();
            if (!config.IncludeBlocked && extendedBlockedHosts.Any(h => lowerHost.EndsWith(h)))
                continue;
            if (!config.IncludeSilenced && extendedSilencedHosts.Any(h => lowerHost.EndsWith(h)))
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
        
        // Delete all processed queue items
        var numChecked = await db.MSQueuedInstances
            .Where(q => q.Id <= maxId)
            .ExecuteDeleteAsync(stoppingToken);
        logger.LogDebug("Checked {numChecked} new instances", numChecked);
        
        // Save changes
        await db.SaveChangesAsync(stoppingToken);
    }
}