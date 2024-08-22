using System.Net;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ModShark.Reports.Document;
using ModShark.Reports.Render;
using ModShark.Services;
using ModShark.Utils;

namespace ModShark.Reports.Reporter.WebHooks;

public interface IDiscordPublisher : IWebHookPublisher;

public class DiscordPublisher(ILogger<DiscordPublisher> logger, IHttpService httpService, IRenderService renderService, ITimeService timeService) : IDiscordPublisher
{
    public async Task SendReport(WebHook webHook, Report report, CancellationToken stoppingToken)
    {
        if (webHook.MaxLength > 2000)
        {
            logger.LogWarning("Skipping Discord - MaxLength exceeds Discord's official limits");
            return;
        }
        
        var message = renderService
            .RenderReport(report, DocumentFormat.Markdown)
            .ToStrings(webHook.MaxLength)
            .ToList();

        foreach (var page in message)
        {
            var execute = new DiscordExecute
            {
                Content = page,
                
                // Prevent rendering usernames as mentions
                // https://discord.com/developers/docs/resources/message#allowed-mentions-object
                AllowedMentions = {
                    Parse = []
                },
                
                // Prevent rendering links as previews
                // https://discord.com/developers/docs/resources/message#message-object-message-flags
                Flags = MessageFlags.SuppressEmbeds
            };

            // Send the next page
            // https://discord.com/developers/docs/resources/webhook#execute-webhook
            var response = await httpService.PostAsync(webHook.Url, execute, stoppingToken);
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                var details = await response.Content.ReadAsStringAsync(stoppingToken);
                logger.LogError("Failed to send WebHook: got HTTP/{code} {details}", response.StatusCode, details);
                break;
            }
            
            // Check rate limit headers
            var limitRemaining = response.Headers.GetNumeric<int>("X-RateLimit-Remaining") ?? int.MaxValue;
            if (limitRemaining < 1)
            {
                var limitSeconds = response.Headers.GetNumeric<double>("X-RateLimit-Reset-After") ?? 0d;
                var limitMilliseconds = (int)Math.Ceiling(limitSeconds * 1000) + 100; // 100ms margin of error
                await timeService.Delay(limitMilliseconds, stoppingToken);
            }
        }
    }
}


[PublicAPI]
internal class DiscordExecute
{
    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("allowed_mentions")]
    public AllowedMentions AllowedMentions { get; set; } = new();
    
    [JsonPropertyName("flags")]
    public MessageFlags? Flags { get; set; }
}

[PublicAPI]
internal class AllowedMentions
{
    [JsonPropertyName("parse")]
    public string[]? Parse { get; set; }
}

[PublicAPI]
[Flags]
internal enum MessageFlags
{
    SuppressEmbeds = 1 << 2,
    SuppressNotifications = 1 << 12
}