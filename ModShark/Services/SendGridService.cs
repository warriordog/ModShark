using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace ModShark.Services;

public interface ISendGridService
{
    Task SendReport(string subject, string body, CancellationToken stoppingToken);
}

[PublicAPI]
public class SendGridConfig
{
    public bool Enabled { get; set; }
    public string ApiKey { get; set; } = "";
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "";
    public List<string> ToAddresses { get; set; } = [];
}

public class SendGridService(ILogger<SendGridService> logger, SendGridConfig config, HttpClient httpClient) : ISendGridService
{
    public async Task SendReport(string subject, string body, CancellationToken stoppingToken)
    {
        if (!config.Enabled)
        {
            logger.LogDebug("Skipping email - SendGrid is disabled in config");
            return;
        }
        
        logger.LogInformation("Sending message {subject}: {body}", subject, body);

        // https://www.twilio.com/docs/sendgrid/api-reference/mail-send/mail-send
        var request = new SendGridSend
        {
            Personalizations = config.ToAddresses
                .Select(to => new SendGridPersonalization
                {
                    To =
                    [
                        new SendGridAddress
                        {
                            Email = to
                        }
                    ]
                })
                .ToList(),
            From = new SendGridAddress
            {
                Email = config.FromAddress,
                Name = config.FromName
            },
            Subject = subject,
            Content =
            [
                new SendGridContent()
                {
                    Type = "text/html",
                    Value = body
                }
            ]
        };
        var message = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
        message.Content = JsonContent.Create(request);
        var response = await httpClient.SendAsync(message, stoppingToken);

        if (response.StatusCode != HttpStatusCode.Accepted)
        {
            var details = await response.Content.ReadAsStringAsync(stoppingToken);
            logger.LogWarning("Failed to send notification email: got HTTP/{code} {details}", response.StatusCode, details);
        }
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal class SendGridSend
{
    [JsonPropertyName("personalizations")]
    public required List<SendGridPersonalization> Personalizations { get; set; } 
    
    [JsonPropertyName("from")]
    public required SendGridAddress From { get; set; }
    
    [JsonPropertyName("subject")]
    public required string? Subject { get; set; }
    
    [JsonPropertyName("content")]
    public List<SendGridContent> Content { get; set; } = [];
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal class SendGridPersonalization
{
    [JsonPropertyName("to")]
    public required List<SendGridAddress> To { get; set; } = [];
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal class SendGridAddress
{
    [JsonPropertyName("email")]
    public required string Email { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal class SendGridContent
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    
    [JsonPropertyName("value")]
    public required string Value { get; set; }
}