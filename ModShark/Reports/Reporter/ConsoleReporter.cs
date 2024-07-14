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
        LogNoteReports(report);
        
        return Task.CompletedTask;
    }

    private void LogUserReports(Report report)
    {
        if (!report.HasUserReports)
            return;

        logger.LogInformation("Flagged {count} new user(s)", report.UserReports.Count);

        foreach (var userReport in report.UserReports)
        {
            if (userReport.IsLocal)
                logger.LogInformation("Flagged new user {id} - local @{username}", userReport.User.Id, userReport.User.Username);
            else
                logger.LogInformation("Flagged new user {id} - remote @{username}@{host}", userReport.User.Id, userReport.User.Username, userReport.User.Host);
        }
    }

    private void LogInstanceReports(Report report)
    {
        if (!report.HasInstanceReports)
            return;
        
        logger.LogInformation("Flagged {count} new instance(s)", report.InstanceReports.Count);

        foreach (var instanceReport in report.InstanceReports)
        {
            logger.LogInformation("Flagged new instance {id} - {host}", instanceReport.Instance.Id, instanceReport.Instance.Host);
        }
    }

    private void LogNoteReports(Report report)
    {
        if (!report.HasNoteReports)
            return;
        
        logger.LogInformation("Flagged {count} new note(s)", report.NoteReports.Count);
        
        foreach (var noteReport in report.NoteReports)
        {
            if (noteReport.IsLocal)
                logger.LogInformation("Flagged new note {id} by user {userId} - local @{username}", noteReport.Note.Id, noteReport.User.Id, noteReport.User.Username);
            else
                logger.LogInformation("Flagged new note {id} by user {userId} - remote @{username}@{host}", noteReport.Note.Id, noteReport.User.Id, noteReport.User.Username, noteReport.User.Host);
        }
    }
}