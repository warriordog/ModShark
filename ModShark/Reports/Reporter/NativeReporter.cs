using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using ModShark.Services;
using SharkeyDB;
using SharkeyDB.Entities;

namespace ModShark.Reports.Reporter;

public interface INativeReporter : IReporter;

[PublicAPI]
public class NativeReporterConfig
{
    public bool Enabled { get; set; } = true;
    public bool UseApi { get; set; }
}

public class NativeReporter(ILogger<NativeReporter> logger, NativeReporterConfig reporterConfig, SharkeyContext db, ISharkeyIdService sharkeyIdService, IServiceAccountService serviceAccountService, ISharkeyHttpService http) : INativeReporter
{
    private const string UserReportComment = "ModShark: username matched one or more flagged patterns";
    private const string InstanceReportComment = "ModShark: instance hostname matched one or more flagged patterns";
    
    public async Task MakeReport(Report report, CancellationToken stoppingToken)
    {
        if (!reporterConfig.Enabled)
        {
            logger.LogDebug("Skipping native - disabled in config");
            return;
        }
        
        if (!report.HasReports)
        {
            logger.LogDebug("Skipping native - report is empty");
            return;
        }
        
        var reporterId = await serviceAccountService.GetServiceAccountId(stoppingToken);
        if (reporterId == null)
        {
            logger.LogWarning("Skipping native - configured service account could not be found");
            return;
        }

        await SaveUserReports(report, reporterId, stoppingToken);
        await SaveInstanceReports(report, reporterId, stoppingToken);

        await db.SaveChangesAsync(stoppingToken);
    }

    private async Task SaveUserReports(Report report, string reporterId, CancellationToken stoppingToken)
    {
        if (!report.HasUserReports)
            return;

        foreach (var userReport in report.UserReports)
        {
            await MakeReport(userReport.UserId, userReport.Hostname, reporterId, UserReportComment, report.ReportDate, stoppingToken);
        }
    }

    private async Task SaveInstanceReports(Report report, string reporterId, CancellationToken stoppingToken)
    {
        if (!report.HasInstanceReports)
            return;
        
        // We can't report an instance directly, so try to find a system user to report instead.
        var reportedIds = await GetReportableInstanceUsers(report, stoppingToken);
        
        foreach (var instanceReport in report.InstanceReports)
        {
            // Lookup the target account ID from the table we just produced
            if (!reportedIds.TryGetValue(instanceReport.Hostname, out var reportedId))
            {
                logger.LogWarning("Could not issue native report against instance {instanceId} ({instanceHost}) because no reportable users were found.", instanceReport.InstanceId, instanceReport.Hostname);
                continue;
            }

            await MakeReport(reportedId, instanceReport.Hostname, reporterId, InstanceReportComment, report.ReportDate, stoppingToken);
        }
    }

    private async Task<Dictionary<string, string>> GetReportableInstanceUsers(Report report, CancellationToken stoppingToken)
    {
        var reportedHostnames = report.InstanceReports.Select(r => r.Hostname).ToHashSet();
        var reportedIds = await db.Users
            // All users from any of the reported hosts
            .Where(u => u.Host != null && reportedHostnames.Contains(u.Host))
            // Grouped by host
            .GroupBy(u => u.Host!)
            // Collapsed into a host->userId mapping
            .ToDictionaryAsync(
                g => g.Key,
                g => g
                    // In order of 'instance.actor' > '%.%' > %
                    .OrderBy(u =>
                        u.UsernameLower == "instance.actor"
                            ? 0
                            : u.UsernameLower.Contains(".")
                                ? 1
                                : 2)
                    // Returning only the best-matching user ID
                    .First().Id,
                stoppingToken
            );
        return reportedIds;
    }

    private async Task MakeReport(string reportedId, string? reportedHost, string reporterId, string comment, DateTime reportDate, CancellationToken stoppingToken)
    {
        // API report is simple - just call it
        if (reporterConfig.UseApi)
        {
            try
            {
                await http.ReportAbuse(reportedId, comment, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to make report against {instance} user {reportedId} - API call failed with exception", reportedHost ?? "LOCAL", reportedId);
            }
            
            // Don't allow API errors to "fall back" on DB reports, because they're often due to a missing / invalid user.
            // When that's the case, inserting a DB report will also fail with a foreign key violation.
            return;
        }

        // DB report requires us to construct the entire entity including ID
        var reportId = sharkeyIdService.GenerateId(reportDate);
        var abuseUserReport = new AbuseUserReport
        {
            Id = reportId,
            TargetUserHost = reportedHost,
            TargetUserId = reportedId,
            ReporterHost = null,
            ReporterId = reporterId,
            AssigneeId = null,
            Comment = comment
        };
        db.AbuseUserReports.Add(abuseUserReport);
    }
}