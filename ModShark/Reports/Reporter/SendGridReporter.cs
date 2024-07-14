using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ModShark.Services;
using ModShark.Utils;

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

public class SendGridReporter(ILogger<SendGridReporter> logger, SendGridReporterConfig reporterConfig, IHttpService httpService, ILinkService linkService) : ISendGridReporter
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

    private (string subject, string message) RenderReport(Report report)
    {
        var subject = "ModShark auto-moderator";
        var message = RenderReportMessage(report, subject);

        return (subject, message);
    }

    private string RenderReportMessage(Report report, string subject)
    {
        var messageBuilder = new StringBuilder();
        
        messageBuilder.Append($"<h1>{subject}</h1>");
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
            message.Append("<h2>Found 1 new flagged instance</h2>");
        else
            message.Append($"<h2>Found {count} new flagged instances</h2>");
        
        message.Append("<ul>");
        foreach (var instanceReport in report.InstanceReports)
        {
            var instanceLink = linkService.GetLinkToInstance(instanceReport.Instance);
            var localInstanceLink = linkService.GetLocalLinkToInstance(instanceReport.Instance);

            message
                .AppendHtml("li", () => message
                    // Instance remote link
                    .Append("Remote instance ")
                    .AppendHtmlAnchor(instanceLink, () => message
                        .Append($"<code>{instanceReport.Instance.Id}</code> ({instanceReport.Instance.Host})"))

                    // instance local link
                    .Append(' ')
                    .AppendHtmlAnchor(localInstanceLink, () => message
                        .AppendHtmlStyle("font-style: italic", () => message
                            .Append("[local mirror]"))));
        }
        message.Append("</ul>");
    }

    private void RenderUserReports(Report report, StringBuilder message)
    {
        if (!report.HasUserReports)
            return;

        var count = report.UserReports.Count;
        if (count == 1)
            message.Append("<h2>Found 1 new flagged username</h2>");
        else
            message.Append($"<h2>Found {count} new flagged usernames</h2>");
        
        message.Append("<ul>");
        foreach (var userReport in report.UserReports)
        {
            message.Append("<li>");
            
            var userLink = linkService.GetLinkToUser(userReport.User);
            
            if (userReport.IsLocal)
            {
                // User local link
                message
                    .AppendHtml("strong", () => message
                        .Append("Local user ")
                        .AppendHtmlAnchor(userLink, () => message
                            .Append($"<code>{userReport.User.Id}</code> ({userReport.User.Username})")));
            }
            else
            {
                var instanceLink = linkService.GetLinkToInstance(userReport.Instance);
                var localInstanceLink = linkService.GetLocalLinkToInstance(userReport.Instance);
                var localUserLink = linkService.GetLocalLinkToUser(userReport.User);
                
                // User remote link
                message
                    .Append("Remote user ")
                    .AppendHtmlAnchor(userLink, () => message
                        .Append($"<code>{userReport.User.Id}</code> ({userReport.User.Username}@{userReport.User.Host})"));
                
                // User local link
                message
                    .Append(' ')
                    .AppendHtmlAnchor(localUserLink, () => message
                        .AppendHtmlStyle("font-style: italic", () => message
                            .Append("[local mirror]")));
                
                // Instance remote link
                message
                    .Append(" from instance ")
                    .AppendHtmlAnchor(instanceLink, () => message
                        .Append($"<code>{userReport.Instance.Id}</code> ({userReport.Instance.Host})"));
                
                // Instance local link
                message
                    .Append(' ')
                    .AppendHtmlAnchor(localInstanceLink, () => message
                        .AppendHtmlStyle("font-style: italic", () => message
                            .Append("[local mirror]")));
            }
            
            message.Append("</li>");
        }
        message.Append("</ul>");
    }
    
    private void RenderNoteReports(Report report, StringBuilder message)
    {
        if (!report.HasNoteReports)
            return;

        var count = report.NoteReports.Count;
        if (count == 1)
            message.Append("<h2>Found 1 new flagged note</h2>");
        else
            message.Append($"<h2>Found {count} new flagged notes</h2>");
        
        message.Append("<ul>");
        foreach (var noteReport in report.NoteReports)
        {
            var noteLink = linkService.GetLinkToNote(noteReport.Note);
            var userLink = linkService.GetLinkToUser(noteReport.User);
            
            message.Append("<li>");
            
            if (noteReport.IsLocal) 
            {
                // note local link
                message
                    .AppendHtml("strong", () => message
                        .Append("Local note ")
                        .AppendHtmlAnchor(noteLink, () => message
                            .Append($"<code>{noteReport.Note.Id}</code>")));
                
                // user local link
                message
                    .Append(" by user ")
                    .AppendHtmlAnchor(userLink, () => message
                        .Append($"<code>{noteReport.User.Id}</code> ({noteReport.User.Username})"));
            }
            else 
            {
                var instanceLink = linkService.GetLinkToInstance(noteReport.Instance);
                var localInstanceLink = linkService.GetLocalLinkToInstance(noteReport.Instance);
                var localNoteLink = linkService.GetLocalLinkToNote(noteReport.Note);
                var localUserLink = linkService.GetLocalLinkToUser(noteReport.User);
                
                // Note remote link
                message
                    .Append("Remote note ")
                    .AppendHtmlAnchor(noteLink, () => message
                        .Append($"<code>{noteReport.Note.Id}</code>"));
                
                // Note local link
                message
                    .Append(' ')
                    .AppendHtmlAnchor(localNoteLink, () => message
                        .AppendHtmlStyle("font-style: italic", () => message
                            .Append("[local mirror]")));
                
                // User remote link
                message
                    .Append(" by user ")
                    .AppendHtmlAnchor(userLink, () => message
                        .Append($"<code>{noteReport.User.Id}</code> ({noteReport.User.Username}@{noteReport.User.Host})"));
                
                // User local link
                message
                    .Append(' ')
                    .AppendHtmlAnchor(localUserLink, () => message
                        .AppendHtmlStyle("font-style: italic", () => message
                            .Append("[local mirror]")));
                
                // Instance remote link
                message
                    .Append(" from instance ")
                    .AppendHtmlAnchor(instanceLink, () => message
                        .Append($"<code>{noteReport.Instance.Id}</code> ({noteReport.Instance.Host})"));
                
                // Instance local link
                message
                    .Append(' ')
                    .AppendHtmlAnchor(localInstanceLink, () => message
                        .AppendHtmlStyle("font-style: italic", () => message
                            .Append("[local mirror]")));
            }
            
            message.Append("</li>");
        }
        message.Append("</ul>");
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