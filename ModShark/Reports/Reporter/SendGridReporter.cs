using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ModShark.Services;

namespace ModShark.Reports.Reporter;

public interface ISendGridReporter : IReporter;

[PublicAPI]
public class SendGridReporterConfig
{
    public bool Enabled { get; set; }
    public string ApiKey { get; set; } = "";
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "";
    public List<string> ToAddresses { get; set; } = [];
}

public class SendGridReporter(ILogger<SendGridReporter> logger, SendGridReporterConfig reporterConfig, IHttpService httpService) : ISendGridReporter
{
    public async Task MakeReport(Report report, CancellationToken stoppingToken)
    {
        if (!reporterConfig.Enabled)
        {
            logger.LogDebug("Skipping SendGrid - disabled in config");
            return;
        }

        if (string.IsNullOrEmpty(reporterConfig.ApiKey))
        {
            logger.LogWarning("Skipping SendGrid - API key is missing");
            return;
        }
        
        if (string.IsNullOrEmpty(reporterConfig.FromAddress))
        {
            logger.LogWarning("Skipping SendGrid - sender address is missing");
            return;
        }

        if (reporterConfig.ToAddresses.Count < 1)
        {
            logger.LogWarning("Skipping SendGrid - no recipients specified");
            return;
        }
        
        if (!report.HasReports)
        {
            logger.LogDebug("Skipping SendGrid - report is empty");
            return;
        }
        
        var (subject, message) = RenderReport(report);
        logger.LogInformation("Sending email via SendGrid {subject}: {body}", subject, message);

        var body = CreateSend(subject, message);
        await SendEmail(body, stoppingToken);
    }

    private static (string subject, string message) RenderReport(Report report)
    {
        var subject = "ModShark auto-moderator";
        var message = RenderReportMessage(report, subject);

        return (subject, message);
    }

    private static string RenderReportMessage(Report report, string subject)
    {
        var messageBuilder = new StringBuilder();
        
        messageBuilder.Append($"<h1>{subject}</h1>");
        RenderUserReports(report, messageBuilder);
        RenderInstanceReports(report, messageBuilder);
        
        return messageBuilder.ToString();
    }

    private static void RenderUserReports(Report report, StringBuilder messageBuilder)
    {
        if (!report.HasUserReports)
            return;

        var count = report.UserReports.Count;
        messageBuilder.Append($"<h2>Found {count} new flagged username(s)</h2>");
        
        messageBuilder.Append("<ul>");
        foreach (var entry in report.UserReports)
        {
            messageBuilder.Append("<li>");
            
            if (entry.IsLocal)
                messageBuilder.Append($"<strong>Local user {entry.UserId}</strong> - <code>@{entry.Username}</code>");
            else
                messageBuilder.Append($"Remote user {entry.UserId} - <code>@{entry.Username}@{entry.Hostname}</code>");
            
            messageBuilder.Append("</li>");
        }
        messageBuilder.Append("</ul>");
    }

    private static void RenderInstanceReports(Report report, StringBuilder messageBuilder)
    {
        if (!report.HasInstanceReports)
            return;

        var count = report.InstanceReports.Count;
        messageBuilder.Append($"<h2>Found {count} new flagged instance(s)</h2>");
        
        messageBuilder.Append("<ul>");
        foreach (var entry in report.InstanceReports)
        {
            messageBuilder.Append($"<li>{entry.InstanceId} - <code>{entry.Hostname}</code></li>");
        }
        messageBuilder.Append("</ul>");
    }

    private SendGridSend CreateSend(string subject, string message)
        // https://www.twilio.com/docs/sendgrid/api-reference/mail-send/mail-send
        => new()
        {
            Personalizations = reporterConfig.ToAddresses
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
                Email = reporterConfig.FromAddress,
                Name = reporterConfig.FromName
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

    private async Task SendEmail(SendGridSend send, CancellationToken stoppingToken)
    {
        var headers = new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {reporterConfig.ApiKey}"
        };

        var response = await httpService.PostAsync("https://api.sendgrid.com/v3/mail/send", send, headers, stoppingToken);
        
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