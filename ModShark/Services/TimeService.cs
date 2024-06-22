namespace ModShark.Services;

public interface ITimeService
{
    public DateTime Now { get; }
    public DateTime UtcNow { get; }
}

public class TimeService : ITimeService
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}