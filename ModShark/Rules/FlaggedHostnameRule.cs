using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using ModShark.Services;
using SharkeyDB;
using SharkeyDB.Entities;

namespace ModShark.Rules;

public interface IFlaggedHostnameRule
{
    Task RunRule(CancellationToken stoppingToken);
}

[PublicAPI]
public class FlaggedHostnameConfig
{
    public bool Enabled { get; set; }
    public List<string> FlaggedPatterns { get; set; } = [];
    public int Timeout { get; set; }
}

public class FlaggedHostnameRule(ILogger<FlaggedHostnameRule> logger, FlaggedHostnameConfig config, SharkeyContext db, ISendGridService sendGrid) : IFlaggedHostnameRule
{
    // Merge and pre-compile the pattern for efficiency
    private Regex Pattern { get; } = new(
        string.Join("|", config.FlaggedPatterns.Select(p => $"({p})")),
        RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
    );
    
    public async Task RunRule(CancellationToken stoppingToken)
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
        
        // Make sure that all timestamps are the same
        var now = DateTime.UtcNow;
        
        // Find all new flagged instances
        var report = await FlagNewInstances(now, stoppingToken);
        
        // Emit all configured reports
        if (report != null)
            await WriteReport(now, report, stoppingToken);
    }
    
    private async Task<List<Instance>?> FlagNewInstances(DateTime now, CancellationToken stoppingToken)
    {
        // Cap at this number to ensure that we don't mistakenly clobber new inserts.
        // This is nullable to prevent System.InvalidOperationException - https://stackoverflow.com/a/54117075
        var maxId = await db.MSQueuedInstances
            .MaxAsync(q => (int?)q.Id, stoppingToken);

        // Stop early if the queue is empty
        if (maxId is null or < 1)
        {
            logger.LogDebug("Nothing to do, instance queue is empty");
            return null;
        }
        
        var report = new List<Instance>();
        
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
            
            report.Add(instance);

            db.MSFlaggedInstances.Add(new MSFlaggedInstance
            {
                InstanceId = instance.Id,
                FlaggedAt = now
            });
        }
        
        // Delete all processed queue items
        await db.MSQueuedInstances
            .Where(q => q.Id <= maxId)
            .ExecuteDeleteAsync(stoppingToken);
        
        // Save changes
        await db.SaveChangesAsync(stoppingToken);
        
        return report;
    }
    
    
    private async Task WriteReport(DateTime now, List<Instance> report, CancellationToken stoppingToken)
    {
        if (report.Count <= 0)
        {
            logger.LogDebug("Skipping report - found no flagged hostnames");
            return;
        }
        
        WriteConsoleReport(report);
        await WriteEmailReport(now, report, stoppingToken);
    }

    private void WriteConsoleReport(List<Instance> report)
    {
        logger.LogInformation("Flagged {count} new instances", report.Count);

        foreach (var instance in report)
        {
            logger.LogInformation("Flagged new instance {id} - {host}", instance.Id, instance.Host);
        }
    }

    private async Task WriteEmailReport(DateTime now, List<Instance> report, CancellationToken stoppingToken)
    {
        
        var items = string.Join("", report.Select(u => $"<li>{u.Id} - <code>{u.Host}</code></li>"));

        var count = report.Count;
        var header = count == 1
            ? "1 new flagged hostname"
            : $"{count} new flagged hostnames";
        var body = $"<h1>ModShark auto-moderator</h1><h2>Found {header} at {now}</h2><ul>{items}</ul>";
        var subject = $"ModShark: {header}";
        
        await sendGrid.SendReport(subject, body, stoppingToken);
    }
}