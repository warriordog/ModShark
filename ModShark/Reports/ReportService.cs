using ModShark.Reports.Reporter;

namespace ModShark.Reports;

public interface IReportService
{
    Task MakeReports(Report report, CancellationToken stoppingToken);
}

public class ReportService(ILogger<ReportService> logger, ISendGridReporter sendGridReporter, IConsoleReporter consoleReporter, INativeReporter nativeReporter) : IReportService
{
    public async Task MakeReports(Report report, CancellationToken stoppingToken)
    {
        if (!report.HasReports)
        {
            logger.LogDebug("Skipping report - no new flags");
            return;
        }
        
        await MakeReport(report, consoleReporter, stoppingToken);
        await MakeReport(report, nativeReporter, stoppingToken);
        await MakeReport(report, sendGridReporter, stoppingToken);
    }

    private async Task MakeReport(Report report, IReporter reporter, CancellationToken stoppingToken)
    {
        try
        {
            await reporter.MakeReport(report, stoppingToken);
        }
        catch (Exception ex)
        {
            var name = reporter.GetType().Name;
            logger.LogError(ex, "Failed to run reporter {name}", name);
        }
    }
}