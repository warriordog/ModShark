using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using ModShark.Reports;
using SharkeyDB.Entities;

namespace ModShark.Rules;

[PublicAPI]
public abstract class QueuedRuleConfig : RuleConfig
{
    public required int BatchLimit { get; set; }
}

public abstract class QueuedRule<TQueueEntity>(ILogger logger, QueuedRuleConfig config, DbContext db, DbSet<TQueueEntity> queue) : IRule
    where TQueueEntity : class, IEntity<int>
{
    /// <inheritdoc />
    public async Task RunRule(Report report, CancellationToken stoppingToken)
    {
        if (!config.Enabled)
        {
            logger.LogDebug("Skipping run, rule is disabled");
            return;
        }

        if (config.BatchLimit < 1)
        {
            logger.LogWarning("Skipping run, batch limit must be greater than zero");
            return;
        }

        if (!await CanRun(stoppingToken))
        {
            logger.LogDebug("Skipping run, precondition check failed");
            return;
        }

        var numChecked = 0;
        
        // Run in batches until the queue is empty
        while (true)
        {
            // Select the next batch
            var maxId = await queue
                // Ensure these are in numerical increasing order
                .OrderBy(q => q.Id)
                // Limit to the batch size
                .Take(config.BatchLimit)
                // Find the highest Id from first BatchLimit rows in queue
                // This is nullable to prevent System.InvalidOperationException - https://stackoverflow.com/a/54117075
                .MaxAsync(q => (int?)q.Id, stoppingToken);

            // Stop when the queue is empty
            if (maxId is null or < 1)
                break;
            
            // Call the actual rule implementation
            logger.LogDebug("Processing batch up to {maxId}", maxId);
            await RunQueuedRule(report, maxId.Value, stoppingToken);

            // Save changes and reset the change tracker.
            // This is important to reduce memory usage.
            await db.SaveChangesAsync(stoppingToken);
            db.ChangeTracker.Clear();
        
            // Delete all processed queue items
            numChecked += await queue
                .Where(q => q.Id <= maxId)
                .ExecuteDeleteAsync(stoppingToken);
        }

        if (numChecked > 0)
            logger.LogDebug("Checked {numChecked} new instances", numChecked);
        else
            logger.LogDebug("Nothing to do, queue is empty");
    }

    /// <summary>
    /// Performs precondition checks and verifies that the rule can safely run.
    /// Return false and record a log entry to skip the run, or return true to allow the run to proceed.
    /// </summary>
    protected virtual Task<bool> CanRun(CancellationToken stoppingToken)
        => Task.FromResult(true);

    /// <summary>
    /// Run a single cycle of the rule.
    /// This method can assume that all prerequisite checks have passed.
    /// The caller is responsible for transactions and cleanup.
    /// </summary>
    /// <param name="report">Report to populate with results</param>
    /// <param name="maxId">Maximum queue ID that can be processed</param>
    /// <param name="stoppingToken">Cancellation token</param>
    protected abstract Task RunQueuedRule(Report report, int maxId, CancellationToken stoppingToken);
}