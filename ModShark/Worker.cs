using JetBrains.Annotations;
using ModShark.Reports;
using ModShark.Services;

namespace ModShark;

[PublicAPI]
public class WorkerConfig
{
    public int PollInterval { get; set; }
}

public class Worker(ILogger<Worker> logger, WorkerConfig config, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker started at {time} with polling interval {interval}", DateTimeOffset.Now, config.PollInterval);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogDebug("Poll starting at {time}", DateTimeOffset.Now);
            await RunTick(stoppingToken);
            logger.LogDebug("Poll complete at {time}", DateTimeOffset.Now);
            
            await Task.Delay(config.PollInterval, stoppingToken);
        }
        
        logger.LogInformation("Worker stopped at {time}", DateTimeOffset.Now);
    }

    private async Task RunTick(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var report = new Report();
        
        // Run rules to build report
        await scope.ServiceProvider
            .GetRequiredService<IRuleService>()
            .RunRules(report, stoppingToken);
        
        // Save the report
        await scope.ServiceProvider
            .GetRequiredService<IReportService>()
            .MakeReports(report, stoppingToken);
    }
}