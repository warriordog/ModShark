using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using ModShark.Services;
using SharkeyDB;

namespace ModShark.Reports.Reporter;

public interface IPostReporter : IReporter;

[PublicAPI]
public class PostReporterConfig
{
    public bool Enabled { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter<PostVisibility>))]
    public PostVisibility Visibility { get; set; } = PostVisibility.Followers;
    
    public bool LocalOnly { get; set; }

    public HashSet<string> Audience { get; set; } = [];

    public string? Subject { get; set; } = "ModShark Report";
    
    public string Template { get; set; } = "$report_body";
}

public enum PostVisibility
{
    /// <summary>
    /// Completely public
    /// </summary>
    Public,
    
    /// <summary>
    /// Unlisted / home-only
    /// </summary>
    Unlisted,
    
    /// <summary>
    /// Visible to followers only
    /// </summary>
    Followers,
    
    /// <summary>
    /// Send as a DM
    /// </summary>
    Private
}

public partial class PostReporter(ILogger<PostReporter> logger, PostReporterConfig reporterConfig, SharkeyContext db, ISharkeyHttpService http) : IPostReporter
{
    // Parse the audience list from handle[] into (handle, username, host?)[].
    // For performance, we do this only once.
    private List<Audience> ParsedAudience { get; } = reporterConfig
        .Audience
        .Select(a => AudienceRegex().Match(a))
        .Where(m => m.Success)
        .Select(m =>
        {
            var handle = m.Value;
            var username = m.Groups[1].Value;
            var host =
                m.Groups[2].Success
                    ? m.Groups[2].Value
                    : null;
            return new Audience(handle, username, host);
        })
        .ToList();

    private string? SharkeyVisibility => reporterConfig.Visibility switch
    {
        PostVisibility.Public => "public",
        PostVisibility.Unlisted => "home",
        PostVisibility.Followers => "followers",
        PostVisibility.Private => "specified",
        _ => null
    };

    // Convert empty string to null.
    // Workaround for https://github.com/dotnet/runtime/issues/36510
    private string? Subject => reporterConfig.Subject == ""
        ? null
        : reporterConfig.Subject;
    
    public async Task MakeReport(Report report, CancellationToken stoppingToken)
    {
        if (!reporterConfig.Enabled)
        {
            logger.LogDebug("Skipping Post - disabled in config");
            return;
        }

        if (string.IsNullOrEmpty(reporterConfig.Template))
        {
            logger.LogWarning("Skipping Post - no template specified");
            return;
        }

        if (SharkeyVisibility == null)
        {
            logger.LogWarning("Skipping Post - visibility is invalid: {visibility}", reporterConfig.Visibility);
            return;
        }

        var content = RenderPost(report);
        var visibleUserIds = await GetAudienceIds(stoppingToken);
        
        await http.CreateNote(
            content,
            SharkeyVisibility,
            stoppingToken,
            localOnly: reporterConfig.LocalOnly,
            cw: Subject,
            visibleUserIds: visibleUserIds
        );
    }

    private string RenderPost(Report report)
    {
        var audienceText = string.Join(' ', reporterConfig.Audience);
        var reportText = RenderReport(report);

        return reporterConfig.Template
            .Replace("$audience", audienceText)
            .Replace("$report_body", reportText);
    }

    // Adapted from SendGridReporter.RenderReport()
    private static string RenderReport(Report report)
    {
        var messageBuilder = new StringBuilder();

        messageBuilder.Append("**ModShark Report**\n\n");
        RenderUserReports(report, messageBuilder);
        RenderInstanceReports(report, messageBuilder);
        RenderNoteReports(report, messageBuilder);
        
        return messageBuilder.ToString();
    }

    private static void RenderUserReports(Report report, StringBuilder messageBuilder)
    {
        if (!report.HasUserReports)
            return;

        var count = report.UserReports.Count;
        messageBuilder.Append($"**Found {count} new flagged username(s):**\n");
        
        foreach (var userReport in report.UserReports)
        {
            if (userReport.IsLocal)
                messageBuilder.Append($"- **Local user {userReport.User.Id}** - `@{userReport.User.Username}`\n");
            else
                messageBuilder.Append($"- Remote user {userReport.User.Id} - `@{userReport.User.Username}@{userReport.User.Host}`\n");
        }

        messageBuilder.Append('\n');
    }

    private static void RenderInstanceReports(Report report, StringBuilder messageBuilder)
    {
        if (!report.HasInstanceReports)
            return;

        var count = report.InstanceReports.Count;
        messageBuilder.Append($"**Found {count} new flagged instance(s):**\n");
        
        foreach (var instanceReport in report.InstanceReports)
        {
            messageBuilder.Append($"- {instanceReport.Instance.Id} - `{instanceReport.Instance.Host}`\n");
        }
    }

    private static void RenderNoteReports(Report report, StringBuilder messageBuilder)
    {
        if (!report.HasNoteReports)
            return;

        var count = report.UserReports.Count;
        messageBuilder.Append($"**Found {count} new flagged notes(s):**\n");
        
        foreach (var noteReport in report.NoteReports)
        {
            if (noteReport.IsLocal)
                messageBuilder.Append($"- **Local note {noteReport.Note.Id}** by user {noteReport.User.Id} - `@{noteReport.User.Username}`\n");
            else
                messageBuilder.Append($"- Remote note {noteReport.Note.Id} by user {noteReport.User.Id} - `@{noteReport.User.Username}@{noteReport.User.Host}`\n");
        }

        messageBuilder.Append('\n');
    }

    // Can be simplified once this is implemented: https://github.com/dotnet/efcore/issues/11799
    private async Task<IEnumerable<string>?> GetAudienceIds(CancellationToken stoppingToken)
    {
        if (reporterConfig.Audience.Count == 0)
            return null;
        
        var audienceIds = new HashSet<string>();
        foreach (var (handle, username, host) in ParsedAudience)
        {
            var id = await db.Users
                .AsNoTracking()
                .Where(u => u.Host == host && u.Username == username)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(stoppingToken);
        
            if (id == null)
            {
                logger.LogWarning("Cannot send post to {handle} - user was not found in the database", handle);
                continue;
            }
        
            audienceIds.Add(id);
        }
        
        return audienceIds;
    }

    [GeneratedRegex(@"^@([^@]+)(?:@(.+))?$", RegexOptions.Compiled, 1000)]
    private static partial Regex AudienceRegex();

    private record Audience(string Handle, string Username, string? Host);
}