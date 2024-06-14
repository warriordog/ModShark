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

public class FlaggedUsernameRule(ILogger<FlaggedUsernameConfig> logger, FlaggedUsernameConfig config, SharkeyContext db, ISendGridService sendGrid) : IFlaggedUsernameRule
{
    // TODO evaluate impact of database-side regular expressions.
    // TODO use LastFlagged to re-check after config changes.
    public async Task RunRule(CancellationToken stoppingToken)
    {
        if (!config.Enabled)
            return;

        if (config.FlaggedPatterns.Count < 1)
            return;

        if (!config.IncludeLocal && !config.IncludeRemote)
            return;
        
        // Run the actual rule logic
        var report = await FlagNewUsers(stoppingToken);
        
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
    }

    private async Task WriteReport(List<User> report, CancellationToken stoppingToken)
    {
        var items = string.Join("", report.Select(u =>
        {
            var type = u.Host == null
                ? "<strong>Local</strong>"
                : "Remote";

            var name = u.Host == null
                ? $"@{u.Username}"
                : $"@{u.Username}@{u.Host}";
            
            return $"<li>{type} user {u.Id} - <code>{name}</code></li>";
        }));

        var header = report.Count > 0
            ? "new flagged usernames"
            : "new flagged username";
        var body = $"<h1>ModShark AutoMod</h1><h2>{header}:</h2><ul>{items}</ul>";
        var subject = $"ModShark: {header}";
        
        await sendGrid.SendReport(subject, body, stoppingToken);
    }

    private async Task<List<User>> FlagNewUsers(CancellationToken stoppingToken)
    {
        var now = DateTime.UtcNow;
        var report = new List<User>();

        // Process each user, with streaming due to potentially large size
        var flaggedUsers = FindNewUsers();
        await foreach (var user in flaggedUsers)
        {
            // Add to report
            report.Add(user);
            
            // Mark as flagged
            db.MSUsers.Add(new MSUser
            {
                UserId = user.Id,
                LastChecked = now,
                LastFlagged = now,
            });
        }
        
        // Save changes
        // TODO move later in pipeline
        await db.SaveChangesAsync(stoppingToken);
        
        return report;
    }

    private IAsyncEnumerable<User> FindNewUsers()
    {
        // Merge patterns - postgres doesn't support ANY with regex
        var pattern = string.Join("|", config.FlaggedPatterns.Select(p => $"({p})"));
        
        // Find next batch of users
        var query =
            from su in db.Users
            join mu in db.MSUsers
                on su.Id equals mu.UserId
                into j
            from mu in j.DefaultIfEmpty()
            where
                mu.LastChecked == null
                && (config.IncludeLocal || su.Host != null)
                && (config.IncludeRemote || su.Host == null)
                && (config.IncludeDeleted || !su.IsDeleted)
                && (config.IncludeSuspended || !su.IsSuspended)
                && Regex.IsMatch(su.UsernameLower, pattern)
            select su;
        
        return query.AsAsyncEnumerable();
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