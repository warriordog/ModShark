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
}

public class NativeReporter(ILogger<NativeReporter> logger, NativeReporterConfig reporterConfig, SharkeyContext db, ISharkeyIdService sharkeyIdService) : INativeReporter
{
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

        await SaveUserReports(report, stoppingToken);
        await SaveInstanceReports(report, stoppingToken);

        await db.SaveChangesAsync(stoppingToken);
    }

    private async Task SaveUserReports(Report report, CancellationToken stoppingToken)
    {
        if (!report.HasUserReports)
            return;
        
        // We have to provide a report user ID, so we use the instance actor        
        var reporterId = await GetInstanceActorId(stoppingToken);
        
        foreach (var userReport in report.UserReports)
        {
            var abuseReport = new AbuseUserReport
            {
                Id = sharkeyIdService.GenerateId(report.ReportDate),
                TargetUserHost = userReport.Hostname,
                TargetUserId = userReport.UserId,
                ReporterHost = null,
                ReporterId = reporterId,
                AssigneeId = null,
                Comment = "ModShark: username matched one or more flagged patterns"
            };
            db.AbuseUserReports.Add(abuseReport);
        }
    }

    private async Task SaveInstanceReports(Report report, CancellationToken stoppingToken)
    {
        if (!report.HasInstanceReports)
            return;
            
        // We have to provide a report user ID, so we use the instance actor        
        var reporterId = await GetInstanceActorId(stoppingToken);
        
        // We can't report an instance directly, so try to find a system user to report instead.
        var reportedHostnames = report.InstanceReports.Select(r => r.Hostname).ToHashSet();
        var reportedIds = await db.Users
                // All users from any of the reported hosts
            .Where(u =>
                u.Host != null
                && reportedHostnames.Contains(u.Host))
                // Grouped by host
            .GroupBy(u => 
                u.Host!
            )
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
        
        foreach (var instanceReport in report.InstanceReports)
        {
            if (!reportedIds.TryGetValue(instanceReport.Hostname, out var reportedId))
            {
                logger.LogWarning("Could not issue native report against instance {instanceId} ({instanceHost}) because no reportable users were found.", instanceReport.InstanceId, instanceReport.Hostname);
                continue;
            }

            var abuseReport = new AbuseUserReport
            {
                Id = sharkeyIdService.GenerateId(report.ReportDate),
                TargetUserHost = instanceReport.Hostname,
                TargetUserId = reportedId,
                ReporterHost = null,
                ReporterId = reporterId,
                AssigneeId = null,
                Comment = "ModShark: instance hostname matched one or more flagged patterns"
            };
            db.AbuseUserReports.Add(abuseReport);
        }
    }

    private async Task<string> GetInstanceActorId(CancellationToken stoppingToken)
        => await db.Users
            .Where(u => u.Host == null && u.Username == "instance.actor")
            .Select(u => u.Id)
            .SingleAsync(stoppingToken);
}