using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using ModShark.Reports.Document;
using ModShark.Reports.Render;
using ModShark.Services;
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

    public bool IncludeFlags { get; set; }
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

public partial class PostReporter(ILogger<PostReporter> logger, SharkeyConfig sharkeyConfig, PostReporterConfig reporterConfig, IUserService userService, ISharkeyHttpService http, IRenderService renderService) : IPostReporter
{
    private const int MinimumNoteLength = 100;

    
    // Parse the audience list from handle[] into (handle, username, host?)[].
    // For performance, we do this only once.
    private List<Audience> ParsedAudience { get; } = reporterConfig
        .Audience
        .Select(a => AudienceRegex().Match(a))
        .Where(m => m.Success)
        .Select(m =>
        {
            var username = m.Groups[1].Value;
            var host =
                m.Groups[2].Success
                    ? m.Groups[2].Value.ToLower()
                    : null;
            var handle =
                host == null
                    ? $"@{username}"
                    : $"@{username}@{host}";
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

        if (sharkeyConfig.MaxNoteLength < MinimumNoteLength)
        {
            logger.LogWarning("Skipping Post - max note length is too low: {limit}", sharkeyConfig.MaxNoteLength);
            return;
        }
        
        logger.LogInformation("Sending report via post");

        var visibleUserIds = await GetAudienceIds(stoppingToken);
        var posts = RenderPost(report);

        string? lastNoteId = null;
        foreach (var post in posts)
        {
            // Delay to avoid rate limits
            if (lastNoteId != null)
                await Task.Delay(1200, stoppingToken);
            
            // Send the post
            var response = await http.CreateNote(
                post,
                SharkeyVisibility,
                stoppingToken,
                localOnly: reporterConfig.LocalOnly,
                cw: Subject,
                visibleUserIds: visibleUserIds,
                inReplyTo: lastNoteId
            );

            // Save the post ID so that we can reply to it
            lastNoteId = response.CreatedNote.Id;
        }
    }

    private List<string> RenderPost(Report report)
    {
        var audienceHandles = ParsedAudience
            .Select(a => a.Handle);
        var audienceText = string.Join(' ', audienceHandles);

        // To make sure that text is chunked correctly, we render everything *except* the report and calculate the length.
        var finalTemplate = reporterConfig.Template
            .Replace("$audience", audienceText);
        var reportChunkSize = sharkeyConfig.MaxNoteLength - finalTemplate.Length;
        if (reportChunkSize < MinimumNoteLength)
        {
            logger.LogWarning("Post template is too long and would overflow the limit of {limit}", sharkeyConfig.MaxNoteLength);
            return [];
        }
        
        // Chunk the report and generate posts.
        // We have to re-render the template each time, to ensure that the audience is carried over.
        var postBuilder = renderService.RenderReport(report, DocumentFormat.MFM, includeFlags: reporterConfig.IncludeFlags);
        return postBuilder
            .ToStrings(reportChunkSize)
            .Select(chunk => finalTemplate.Replace("$report_body", chunk))
            .ToList();
    }

    

    // Can be simplified once this is implemented: https://github.com/dotnet/efcore/issues/11799
    private async Task<List<string>?> GetAudienceIds(CancellationToken stoppingToken)
    {
        if (reporterConfig.Audience.Count == 0)
            return null;
        
        var audienceIds = new HashSet<string>();
        foreach (var (handle, username, host) in ParsedAudience)
        {
            var id = await
                userService.QueryByUserHost(username, host)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(stoppingToken);
        
            if (id == null)
            {
                logger.LogWarning("Cannot send post to {handle} - user was not found in the database", handle);
                continue;
            }
        
            audienceIds.Add(id);
        }
        
        return audienceIds.ToList();
    }

    [GeneratedRegex(@"^@([^@]+)(?:@(.+))?$", RegexOptions.Compiled, 1000)]
    private static partial Regex AudienceRegex();

    private record Audience(string Handle, string Username, string? Host);
}