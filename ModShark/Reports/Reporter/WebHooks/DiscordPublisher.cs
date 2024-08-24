using System.Net;
using System.Net.Http.Json;
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
    public const int RateLimitMarginOfError = 100;
    public const int MaxRateLimitTries = 2;
    
    public async Task SendReport(WebHook webHook, Report report, CancellationToken stoppingToken)
    {
        if (webHook.MaxLength > 2000)
        {
            logger.LogWarning("Skipping Discord - MaxLength exceeds Discord's official limits");
            return;
        }
        
        var message = renderService
            .RenderReport(report, DocumentFormat.Markdown, includeFlags: webHook.IncludeFlags)
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
            var response = await SendRequest(webHook.Url, execute, stoppingToken);
            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                var details = await response.Content.ReadAsStringAsync(stoppingToken);
                logger.LogError("Failed to send WebHook: got HTTP/{code} {details}", response.StatusCode, details);
                break;
            }
        }
    }

    private async Task<HttpResponseMessage> SendRequest<T>(string url, T body, CancellationToken stoppingToken)
    {
        var response = await httpService.PostAsync(url, body, stoppingToken);
            
        // Check rate limit body
        // https://discord.com/developers/docs/topics/rate-limits
        for (var retry = 1; response.StatusCode == HttpStatusCode.TooManyRequests; retry++)
        {
            if (retry > MaxRateLimitTries)
                throw new HttpRequestException("Request failed: we got a rate limit response despite following the appropriate limits");
            
            // Read the rate limit from response
            var rateLimit = await response.Content.ReadFromJsonAsync<RateLimitedResponse>(stoppingToken)
                ?? throw new HttpRequestException("Unable to parse Rate Limit response");
            logger.LogDebug("Rate limited (hard) for {limit} second(s)", rateLimit.RetryAfter);
            
            // Wait for the instructed limit
            var limitMilliseconds = (int)Math.Ceiling(rateLimit.RetryAfter * 1000) + RateLimitMarginOfError;
            await timeService.Delay(limitMilliseconds, stoppingToken);
            
            // Retry the request
            response = await httpService.PostAsync(url, body, stoppingToken);
        }
        
        // Check rate limit headers
        // https://discord.com/developers/docs/topics/rate-limits
        var limitRemaining = response.Headers.GetNumeric<int>("X-RateLimit-Remaining") ?? int.MaxValue;
        if (limitRemaining < 1)
        {
            var limitSeconds = response.Headers.GetNumeric<float>("X-RateLimit-Reset-After") ?? 1f;
            logger.LogDebug("Rate limited (soft) for {limit} second(s)", limitSeconds);
            
            var limitMilliseconds = (int)Math.Ceiling(limitSeconds * 1000) + RateLimitMarginOfError;
            await timeService.Delay(limitMilliseconds, stoppingToken);
        }

        return response;
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

[PublicAPI]
internal class RateLimitedResponse
{
    [JsonPropertyName("retry_after")]
    public float RetryAfter { get; set; }
}