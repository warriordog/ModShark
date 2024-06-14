using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SharkeyDB;
using SharkeyDB.Entities;

namespace ModShark.Rules;

public interface IFlaggedUsernameRule
{
    Task RunRule(CancellationToken stoppingToken);
}

public class FlaggedUsernameRule(ILogger<FlaggedUsernameConfig> logger, FlaggedUsernameConfig config, SharkeyContext db) : IFlaggedUsernameRule
{
    // TODO evaluate impact of database-side regular expressions.
    // TODO use LastFlagged to re-check after config changes.
    public async Task RunRule(CancellationToken stoppingToken)
    {
        if (!config.Enabled)
            return;

        // Run the actual rule logic
        var reportLines = await FlagNewUsers(stoppingToken);
        
        if (reportLines.Count > 0)
        {
            WriteLog(reportLines);   
            WriteReport(reportLines);
        }
        else
        {
            logger.LogDebug("Found no flagged usernames");
        }
    }

    private void WriteLog(List<string> reportLines)
    {
        foreach (var line in reportLines)
        {
            logger.LogInformation("Flagged new {line}", line);   
        }
    }

    private void WriteReport(List<string> reportLines)
    {
        // TODO send email
    }

    private async Task<List<string>> FlagNewUsers(CancellationToken stoppingToken)
    {
        var now = DateTime.UtcNow;
        var reportLines = new List<string>();

        // Process each user, with streaming due to potentially large size
        var flaggedUsers = FindNewUsers();
        await foreach (var user in flaggedUsers)
        {
            // Add to report
            reportLines.Add(GetReportLine(user));
            
            // Mark as flagged
            db.MSUsers.Add(new MSUser
            {
                UserId = user.Id,
                LastChecked = now,
                LastFlagged = now,
            });
        }
        
        // Save changes
        await db.SaveChangesAsync(stoppingToken);
        
        return reportLines;
    }

    private static string GetReportLine(User user)
        => user.Host != null
            ? $"remote user {user.Id} - @{user.Username}@{user.Host}"
            : $"local user {user.Id} - @{user.Username}";

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