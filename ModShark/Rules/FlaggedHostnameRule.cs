using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using ModShark.Reports;
using ModShark.Utils;
using SharkeyDB;
using SharkeyDB.Entities;

namespace ModShark.Rules;

public interface IFlaggedHostnameRule : IRule;

[PublicAPI]
public class FlaggedHostnameConfig
{
    public bool Enabled { get; set; }
    public List<string> FlaggedPatterns { get; set; } = [];
    public int Timeout { get; set; }
}

public class FlaggedHostnameRule(ILogger<FlaggedHostnameRule> logger, FlaggedHostnameConfig config, SharkeyContext db) : IFlaggedHostnameRule
{
    // Merge and pre-compile the pattern for efficiency
    private Regex Pattern { get; } = PatternUtils.CreateMatcher(config.FlaggedPatterns, config.Timeout, true);
    
    public async Task RunRule(Report report, CancellationToken stoppingToken)
    {
        if (!config.Enabled)
        {
            logger.LogDebug("Skipping run, rule is disabled");
            return;
        }

        if (config.FlaggedPatterns.Count < 1)
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
        
        // Query for all new instances that match the given flags
        var query =
            from q in db.MSQueuedInstances.AsNoTracking()
            join i in db.Instances.AsNoTracking()
                on q.InstanceId equals i.Id
            where
                q.Id <= maxId
                && !db.MSFlaggedInstances.Any(f => f.InstanceId == q.InstanceId)
            orderby q.Id
            select i;
        
        // Stream each result due to potentially large size
        var newInstances = query.AsAsyncEnumerable();
        await foreach (var instance in newInstances)
        {
            // For better use of database resources, we handle pattern matching in application code.
            // This also gives us .NET's faster and more powerful regex engine.
            if (!Pattern.IsMatch(instance.Host))
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