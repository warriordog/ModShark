using Microsoft.EntityFrameworkCore;
using ModShark.Reports;
using SharkeyDB.Entities;

namespace ModShark.Rules;

public abstract class QueuedRule<TQueueEntity>(ILogger logger, RuleConfig config, DbContext db, DbSet<TQueueEntity> queue) : IRule
    where TQueueEntity : class, IEntity<int>
{
    /// <inheritdoc />
    public virtual async Task RunRule(Report report, CancellationToken stoppingToken)
    {
        if (!config.Enabled)
        {
            logger.LogDebug("Skipping run, rule is disabled");
            return;
        }
        
        // Cap at this number to ensure that we don't mistakenly clobber new inserts.
        // This is nullable to prevent System.InvalidOperationException - https://stackoverflow.com/a/54117075
        var maxId = await queue.MaxAsync(q => (int?)q.Id, stoppingToken);

        // Stop early if the queue is empty
        if (maxId is null or < 1)
        {
            logger.LogDebug("Nothing to do, queue is empty");
            return;
        }

        // Call the actual rule implementation
        await RunQueuedRule(report, maxId.Value, stoppingToken);
        
        // Delete all processed queue items
        var numChecked = await queue
            .Where(q => q.Id <= maxId)
            .ExecuteDeleteAsync(stoppingToken);
        logger.LogDebug("Checked {numChecked} new instances", numChecked);
        
        // Save changes
        await db.SaveChangesAsync(stoppingToken);
    }

    /// <summary>
    /// Run a single cycle of the rule.
    /// This method can assume that all prerequisite checks have passed.
    /// The caller is responsible for transactions and cleanup.
    /// </summary>
    /// <param name="report">Report to populate with results</param>
    /// <param name="maxId">Maximum queue ID to process in this cycle</param>
    /// <param name="stoppingToken">Cancellation token</param>
    protected abstract Task RunQueuedRule(Report report, int maxId, CancellationToken stoppingToken);
}