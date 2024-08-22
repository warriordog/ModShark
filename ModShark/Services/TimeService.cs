namespace ModShark.Services;

public interface ITimeService
{
    public DateTime Now { get; }
    public DateTime UtcNow { get; }

    public Task Delay(int milliseconds, CancellationToken stoppingToken);
}

public class TimeService : ITimeService
{
    public DateTime Now
        => DateTime.Now;
    
    public DateTime UtcNow
        => DateTime.UtcNow;

    public Task Delay(int milliseconds, CancellationToken stoppingToken)
        => Task.Delay(milliseconds, stoppingToken);
}