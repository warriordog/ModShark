using System.Text.RegularExpressions;
using JetBrains.Annotations;
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
    public async Task RunRule(CancellationToken stoppingToken)
    {
        if (!config.Enabled)
            return;

        if (config.FlaggedPatterns.Count < 1)
        {
            logger.LogWarning("Skipping run, no patterns defined");
            return;
        }

        if (!config.IncludeLocal && !config.IncludeRemote)
        {
            logger.LogWarning("Skipping run, all users are excluded (local & remote)");
            return;
        }
        
        // Make sure that all timestamps are the same
        var now = DateTime.UtcNow;
        
        // Find all new flagged users
        var report = await FlagNewUsers(now, stoppingToken);
        
        // Emit all configured reports
        if (report != null)
            await WriteReport(now, report, stoppingToken);
    }

    private async Task<List<User>?> FlagNewUsers(DateTime now, CancellationToken stoppingToken)
    {
        // Cap at this number to ensure that we don't mistakenly clobber new inserts.
        // This is nullable to prevent System.InvalidOperationException - https://stackoverflow.com/a/54117075
        var maxId = await db.MSQueuedUsers
            .MaxAsync(q => (int?)q.Id, stoppingToken);

        // Stop early if the queue is empty
        if (maxId is null or < 1)
        {
            logger.LogDebug("Nothing to do, user queue is empty");
            return null;
        }
        
        var report = new List<User>();
        
        // Query for all new users that match the given flags
        var query =
            from q in db.MSQueuedUsers
            join u in db.Users
                on q.UserId equals u.Id
            where
                q.Id <= maxId
                && (config.IncludeLocal || u.Host != null)
                && (config.IncludeRemote || u.Host == null)
                && (config.IncludeSuspended || !u.IsSuspended)
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
            if (!IsFlaggedUsername(user.UsernameLower))
                continue;
            
            report.Add(user);

            db.MSFlaggedUsers.Add(new MSFlaggedUser
            {
                UserId = user.Id,
                FlaggedAt = now
            });
        }
        
        // Delete all processed queue items
        await db.MSQueuedUsers
            .Where(q => q.Id <= maxId)
            .ExecuteDeleteAsync(stoppingToken);
        
        // Save changes
        await db.SaveChangesAsync(stoppingToken);
        
        return report;
    }
    
    private bool IsFlaggedUsername(string username)
        => config.FlaggedPatterns.Any(pattern
            // TODO pre-compile the regular expressions
            => Regex.IsMatch(username, pattern)
        );

    private async Task WriteReport(DateTime now, List<User> report, CancellationToken stoppingToken)
    {
        if (report.Count <= 0)
        {
            logger.LogDebug("Skipping report - found no flagged usernames");
            return;
        }
        
        WriteConsoleReport(report);
        await WriteEmailReport(now, report, stoppingToken);
    }

    private void WriteConsoleReport(List<User> report)
    {
        logger.LogInformation("Flagged {count} new users", report.Count);

        foreach (var user in report)
        {
            if (user.Host == null)
                logger.LogInformation("Flagged new user {id} - local @{username}", user.Id, user.Username);
            else
                logger.LogInformation("Flagged new user {id} - remote @{username}@{host}", user.Id, user.Username, user.Host);
        }
    }

    private async Task WriteEmailReport(DateTime now, List<User> report, CancellationToken stoppingToken)
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

        var count = report.Count;
        var header = count == 1
            ? "1 new flagged username"
            : $"{count} new flagged usernames";
        var body = $"<h1>ModShark auto-moderator</h1><h2>Found {header} at {now}</h2><ul>{items}</ul>";
        var subject = $"ModShark: {header}";
        
        await sendGrid.SendReport(subject, body, stoppingToken);
    }
}

[PublicAPI]
public class FlaggedUsernameConfig
{
    public bool Enabled { get; set; }
    public bool IncludeLocal { get; set; }
    public bool IncludeRemote { get; set; }
    public bool IncludeDeleted { get; set; }
    public bool IncludeSuspended { get; set; }
    public List<string> FlaggedPatterns { get; set; } = [];
}