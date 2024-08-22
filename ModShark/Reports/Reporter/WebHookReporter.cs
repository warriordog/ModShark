using JetBrains.Annotations;
using ModShark.Reports.Reporter.WebHooks;

namespace ModShark.Reports.Reporter;

[PublicAPI]
public class WebHookReporterConfig
{
    public bool Enabled { get; set; }

    public List<WebHook> Hooks { get; set; } = [];
}

public interface IWebHookReporter : IReporter;

public class WebHookReporter(ILogger<WebHookReporter> logger, WebHookReporterConfig reporterConfig, IDiscordPublisher discordPublisher) : IWebHookReporter
{
    public async Task MakeReport(Report report, CancellationToken stoppingToken)
    {
        if (!reporterConfig.Enabled)
        {
            logger.LogDebug("Skipping WebHooks - disabled in config");
            return;
        }
        
        if (reporterConfig.Hooks.Count < 1) 
        {
            logger.LogWarning("Skipping WebHooks - No hooks defined");
            return;
        }

        for (var i = 0; i < reporterConfig.Hooks.Count; i++)
        {
            try
            {
                logger.LogInformation("Sending report via WebHook ID {number}", i);

                var webHook = reporterConfig.Hooks[i];
                await SendWebHook(webHook, report, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in WebHook ID {number}", i);
            }
        }
    }

    private async Task SendWebHook(WebHook webHook, Report report, CancellationToken stoppingToken)
    {
        if (string.IsNullOrEmpty(webHook.Url))
        {
            logger.LogWarning("Skipping WebHook - Url is missing");
            return;
        }
        
        if (!Enum.IsDefined(webHook.Type))
        {
            logger.LogWarning("Skipping WebHook - Type is invalid");
            return;
        }
        
        if (webHook.MaxLength < 100)
        {
            logger.LogWarning("Skipping WebHook - MaxLength is too short");
            return;
        }

        // Currently, discord is the only supported type.
        // Add additional types using if...else.
        await discordPublisher.SendReport(webHook, report, stoppingToken);
    }
}
