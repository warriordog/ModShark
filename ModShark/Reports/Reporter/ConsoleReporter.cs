namespace ModShark.Reports.Reporter;

public interface IConsoleReporter : IReporter;

public class ConsoleReporter(ILogger<ConsoleReporter> logger) : IConsoleReporter
{
    public Task MakeReport(Report report, CancellationToken _)
    {
        LogUserReports(report);
        LogInstanceReports(report);
        
        return Task.CompletedTask;
    }

    private void LogUserReports(Report report)
    {
        if (!report.HasUserReports)
        {
            logger.LogDebug("Skipping console - report is empty");
            return;
        }

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