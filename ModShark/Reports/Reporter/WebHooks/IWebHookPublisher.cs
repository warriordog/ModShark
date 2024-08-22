namespace ModShark.Reports.Reporter.WebHooks;

public interface IWebHookPublisher
{
    Task SendReport(WebHook webHook, Report report, CancellationToken stoppingToken);
}