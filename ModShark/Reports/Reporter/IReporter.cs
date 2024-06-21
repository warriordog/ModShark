namespace ModShark.Reports.Reporter;

public interface IReporter
{
    Task MakeReport(Report report, CancellationToken stoppingToken);
}