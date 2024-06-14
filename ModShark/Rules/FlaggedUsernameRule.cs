using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ModShark.Services;
using SharkeyDB;
using SharkeyDB.Entities;

namespace ModShark.Rules;

public interface IFlaggedUsernameRule
{
    Task RunRule(CancellationToken stoppingToken);
}

public class FlaggedUsernameRule(ILogger<FlaggedUsernameRule> logger, FlaggedUsernameConfig config, SharkeyContext db, ISendGridService sendGrid) : IFlaggedUsernameRule
{
    // TODO use LastFlagged to re-check after config changes.
    public async Task RunRule(CancellationToken stoppingToken)
    {
        if (!config.Enabled)
            return;

        if (config.FlaggedPatterns.Count < 1)
            return;

        if (!config.IncludeLocal && !config.IncludeRemote)
            return;
        
        // Make sure that all timestamps are the same
        var now = DateTime.UtcNow;
        
        // 0. Transaction - we MUST do these together for data consistency
        await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
        
        // 1. Flag - scan "user" and insert to "ms_user" only for matches
        var report = await FlagNewUsers(now, stoppingToken);
        
        // 2. Sync - bulk insert with LastChecked=now, on conflict do nothing
        await SyncDatabase(now, stoppingToken);
        
        // 3. Write report
        if (report.Count > 0)
        {
            logger.LogInformation("Flagged {count} new users", report.Count);
        
            foreach (var user in report)
            {
                if (user.Host == null)
                    logger.LogInformation("Flagged new user {id} - local @{username}", user.Id, user.Username);
                else
                    logger.LogInformation("Flagged new user {id} - remote @{username}@{host}", user.Id, user.Username, user.Host);
            }
            
            await WriteReport(report, stoppingToken);
        }
        else
        {
            logger.LogDebug("Found no flagged usernames");
        }
        
        // 4. Commit transaction
        await transaction.CommitAsync(stoppingToken);
    }

    private async Task<List<User>> FlagNewUsers(DateTime now, CancellationToken stoppingToken)
    {
        var report = new List<User>();
        
        // Merge patterns - postgres doesn't support ANY with regex
        var pattern = string.Join("|", config.FlaggedPatterns.Select(p => $"({p})"));
        
        var query =
            from u in db.Users
            where
                !db.MSUsers.Any(msu => msu.UserId == u.Id)
                && (config.IncludeLocal || u.Host != null)
                && (config.IncludeRemote || u.Host == null)
                && (config.IncludeDeleted || !u.IsDeleted)
                && (config.IncludeSuspended || !u.IsSuspended)
                && Regex.IsMatch(u.UsernameLower, pattern)
            select u;
        
        // Stream each result due to potentially large size
        var flaggedUsers = query.AsAsyncEnumerable();
        await foreach (var user in flaggedUsers)
        {
            report.Add(user);

            db.MSUsers.Add(new MSUser
            {
                UserId = user.Id,
                CheckedAt = now,
                IsFlagged = true
            });
        }
        
        // Save changes
        await db.SaveChangesAsync(stoppingToken);
        
        return report;
    }

    private async Task SyncDatabase(DateTime now, CancellationToken stoppingToken)
    {
        const string query =
        """
            INSERT INTO ms_user (user_id, checked_at)
            SELECT
                id as user_id,
                {0} as checked_at
            FROM "user"
            ON CONFLICT(user_id)
                DO NOTHING
        """;
        var parameters = new object[] { now };
        
        await db.Database.ExecuteSqlRawAsync(query, parameters, stoppingToken);
    }
    
    private async Task WriteReport(List<User> report, CancellationToken stoppingToken)
    {
        var items = string.Join("", report.Select(u =>
        {
            var type = u.Host == null
                ? "<strong>Local user</strong>"
                : "Remote user";

            var name = u.Host == null
                ? $"@{u.Username}"
                : $"@{u.Username}@{u.Host}";
            
            return $"<li>{type} {u.Id} - <code>{name}</code></li>";
        }));

        var header = report.Count > 0
            ? "new flagged usernames"
            : "new flagged username";
        var body = $"<h1>ModShark AutoMod</h1><h2>{header}:</h2><ul>{items}</ul>";
        var subject = $"ModShark: {header}";
        
        await sendGrid.SendReport(subject, body, stoppingToken);
    }
}

public class FlaggedUsernameConfig
{
    public bool Enabled { get; set; }
    public bool IncludeLocal { get; set; }
    public bool IncludeRemote { get; set; }
    public bool IncludeDeleted { get; set; }
    public bool IncludeSuspended { get; set; }
    public List<string> FlaggedPatterns { get; set; } = [];
}