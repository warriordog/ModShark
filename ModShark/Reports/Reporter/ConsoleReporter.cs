using JetBrains.Annotations;

namespace ModShark.Reports.Reporter;

public interface IConsoleReporter : IReporter;

[PublicAPI]
public class ConsoleReporterConfig
{
    public bool Enabled { get; set; } = true;
}

public class ConsoleReporter(ILogger<ConsoleReporter> logger, ConsoleReporterConfig reporterConfig) : IConsoleReporter
{
    public Task MakeReport(Report report, CancellationToken _)
    {
        if (!reporterConfig.Enabled)
        {
            logger.LogDebug("Skipping console - disabled in config");
            return Task.CompletedTask;
        }
        
        if (!report.HasReports)
        {
            logger.LogDebug("Skipping console - report is empty");
            return Task.CompletedTask;
        }
        
        LogUserReports(report);
        LogInstanceReports(report);
        
        return Task.CompletedTask;
    }

    private void LogUserReports(Report report)
    {
        if (!report.HasUserReports)
            return;

        logger.LogInformation("Flagged {count} new user(s)", report.UserReports.Count);

        foreach (var user in report.UserReports)
        {
            if (user.IsLocal)
                logger.LogInformation("Flagged new user {id} - local @{username}", user.UserId, user.Username);
            else
                logger.LogInformation("Flagged new user {id} - remote @{username}@{host}", user.UserId, user.Username, user.Hostname);
        }
    }

    private void LogInstanceReports(Report report)
    {
        if (!report.HasInstanceReports)
            return;
        
        logger.LogInformation("Flagged {count} new instance(s)", report.InstanceReports.Count);

        foreach (var instance in report.InstanceReports)
        {
            logger.LogInformation("Flagged new instance {id} - {host}", instance.InstanceId, instance.Hostname);
        }
    }
}