using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using ModShark.Services;
using ModShark.Utils;
using SharkeyDB;
using SharkeyDB.Entities;

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

public partial class PostReporter(ILogger<PostReporter> logger, PostReporterConfig reporterConfig, SharkeyContext db, ISharkeyHttpService http, ILinkService linkService) : IPostReporter
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
        PostVisibility.Public => Note.VisibilityPublic,
        PostVisibility.Unlisted => Note.VisibilityHome,
        PostVisibility.Followers => Note.VisibilityFollowers,
        PostVisibility.Private => Note.VisibilitySpecified,
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
    private string RenderReport(Report report)
    {
        var messageBuilder = new StringBuilder();

        messageBuilder.Append("**ModShark Report**\n\n");
        RenderInstanceReports(report, messageBuilder);
        RenderUserReports(report, messageBuilder);
        RenderNoteReports(report, messageBuilder);
        
        return messageBuilder.ToString();
    }

    private void RenderInstanceReports(Report report, StringBuilder message)
    {
        if (!report.HasInstanceReports)
            return;

        var count = report.InstanceReports.Count;
        if (count == 1)
            message.Append("Found 1 new flagged instance:\n");
        else
            message.Append($"**Found {count} new flagged instances:**\n");
        
        foreach (var instanceReport in report.InstanceReports)
        {
            var instanceLink = linkService.GetLinkToInstance(instanceReport.Instance);
            var localInstanceLink = linkService.GetLocalLinkToInstance(instanceReport.Instance);
            
            // Instance remote link
            message
                .Append("- Remote instance ")
                .AppendMarkdownLink(instanceLink, () => message
                    .AppendMarkdownCode(instanceReport.Instance.Id)
                    .Append($" ({instanceReport.Instance.Host})"));

            // instance local link
            message
                .Append(' ')
                .AppendMarkdownLink(localInstanceLink, () => message
                    .AppendMarkdownItalics(true, "[local mirror]"));
            
            message.Append('\n');
        }

        message.Append('\n');
    }

    private void RenderUserReports(Report report, StringBuilder message)
    {
        if (!report.HasUserReports)
            return;

        var count = report.UserReports.Count;
        if (count == 1)
            message.Append("Found 1 new flagged users:\n");
        else
            message.Append($"**Found {count} new flagged users:**\n");
        
        foreach (var userReport in report.UserReports)
        {
            var userLink = linkService.GetLinkToUser(userReport.User);
            
            if (userReport.IsLocal)
            {
                // User local link
                message
                    .Append("- ")
                    .AppendMarkdownBold(() => message
                        .Append("Local user ")
                        .AppendMarkdownLink(userLink, () => message
                            .AppendMarkdownCode(userReport.User.Id)
                            .Append($" ({userReport.User.Username})")));
            }
            else
            {
                var instanceLink = linkService.GetLinkToInstance(userReport.Instance);
                var localInstanceLink = linkService.GetLocalLinkToInstance(userReport.Instance);
                var localUserLink = linkService.GetLocalLinkToUser(userReport.User);
                
                // User remote link
                message
                    .Append("- Remote user ")
                    .AppendMarkdownLink(userLink, () => message
                        .AppendMarkdownCode(userReport.User.Id)
                        .Append($" ({userReport.User.Username}@{userReport.User.Host})"));
                
                // User local link
                message
                    .Append(' ')
                    .AppendMarkdownLink(localUserLink, () => message
                        .AppendMarkdownItalics(true, "[local mirror]"));
                
                // Instance remote link
                message
                    .Append("\n   from instance ")
                    .AppendMarkdownLink(instanceLink, () => message
                        .AppendMarkdownCode(userReport.Instance.Id)
                        .Append($" ({userReport.Instance.Host})"));

                // instance local link
                message
                    .Append(' ')
                    .AppendMarkdownLink(localInstanceLink, () => message
                        .AppendMarkdownItalics(true, "[local mirror]"));
            }
            
            message.Append('\n');
        }

        message.Append('\n');
    }

    private void RenderNoteReports(Report report, StringBuilder message)
    {
        if (!report.HasNoteReports)
            return;

        var count = report.NoteReports.Count;
        if (count == 1)
            message.Append("Found 1 new flagged note:\n");
        else
            message.Append($"**Found {count} new flagged notes:**\n");
        
        foreach (var noteReport in report.NoteReports)
        {
            var noteLink = linkService.GetLinkToNote(noteReport.Note);
            var userLink = linkService.GetLinkToUser(noteReport.User);
            
            if (noteReport.IsLocal) 
            {
                // note local link
                message
                    .Append("- ")
                    .AppendMarkdownBold(() => message
                        .Append("Local note ")
                        .AppendMarkdownLink(noteLink, () => message
                            .AppendMarkdownCode(noteReport.Note.Id)));
                
                // user local link
                message
                    .Append("\n   by user ")
                    .AppendMarkdownLink(userLink, () => message
                        .AppendMarkdownCode(noteReport.User.Id)
                        .Append($" ({noteReport.User.Username})"));
            }
            else 
            {
                var instanceLink = linkService.GetLinkToInstance(noteReport.Instance);
                var localInstanceLink = linkService.GetLocalLinkToInstance(noteReport.Instance);
                var localNoteLink = linkService.GetLocalLinkToNote(noteReport.Note);
                var localUserLink = linkService.GetLocalLinkToUser(noteReport.User);
                
                // Note remote link
                message
                    .Append("- Remote note ")
                    .AppendMarkdownLink(noteLink, () => message
                        .AppendMarkdownCode(noteReport.Note.Id));
                
                // Note local link
                message
                    .Append(' ')
                    .AppendMarkdownLink(localNoteLink, () => message
                        .AppendMarkdownItalics(true, "[local mirror]"));
                
                // User remote link
                message
                    .Append("\n   by user ")
                    .AppendMarkdownLink(userLink, () => message
                        .AppendMarkdownCode(noteReport.User.Id)
                        .Append($" ({noteReport.User.Username}@{noteReport.User.Host})"));
                
                // User local link
                message
                    .Append(' ')
                    .AppendMarkdownLink(localUserLink, () => message
                        .AppendMarkdownItalics(true, "[local mirror]"));
                
                // Instance remote link
                message
                    .Append("\n   from instance ")
                    .AppendMarkdownLink(instanceLink, () => message
                        .AppendMarkdownCode(noteReport.Instance.Id)
                        .Append($" ({noteReport.Instance.Host})"));

                // instance local link
                message
                    .Append(' ')
                    .AppendMarkdownLink(localInstanceLink, () => message
                        .AppendMarkdownItalics(true, "[local mirror]"));
            }
            
            message.Append('\n');
        }

        message.Append('\n');
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