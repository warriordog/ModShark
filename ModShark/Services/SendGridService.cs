using System.Net;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace ModShark.Services;

public interface ISendGridService
{
    Task SendReport(string subject, string message, CancellationToken stoppingToken);
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

public class SendGridService(ILogger<SendGridService> logger, SendGridConfig config, IHttpService http) : ISendGridService
{
    public async Task SendReport(string subject, string message, CancellationToken stoppingToken)
    {
        if (!config.Enabled)
        {
            logger.LogDebug("Skipping email - SendGrid is disabled in config");
            return;
        }

        if (string.IsNullOrEmpty(config.ApiKey))
        {
            logger.LogWarning("Skipping email - API key is missing");
            return;
        }
        
        if (string.IsNullOrEmpty(config.FromAddress))
        {
            logger.LogWarning("Skipping email - sender address is missing");
            return;
        }

        if (config.ToAddresses.Count < 1)
        {
            logger.LogWarning("Skipping email - no recipients specified");
            return;
        }
        
        logger.LogInformation("Sending message {subject}: {body}", subject, message);

        // https://www.twilio.com/docs/sendgrid/api-reference/mail-send/mail-send
        var body = new SendGridSend
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
                new SendGridContent
                {
                    Type = "text/html",
                    Value = message
                }
            ]
        };

        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {config.ApiKey}"
        };

        var response = await http.PostAsync("https://api.sendgrid.com/v3/mail/send", body, headers, stoppingToken);

        if (response.StatusCode != HttpStatusCode.Accepted)
        {
            var details = await response.Content.ReadAsStringAsync(stoppingToken);
            logger.LogError("Failed to send notification email: got HTTP/{code} {details}", response.StatusCode, details);
            
            // We intentionally don't throw any errors, since email send can fail for various transient reasons.
            // This may change if we ever want to add retry logic, or handle the error in upstream code.
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