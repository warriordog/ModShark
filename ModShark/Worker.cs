using ModShark.Services;

namespace ModShark;

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

        await scope.ServiceProvider
            .GetRequiredService<IRuleService>()
            .RunRules(stoppingToken);
    }
}

public class WorkerConfig
{
    public int PollInterval { get; set; }
}