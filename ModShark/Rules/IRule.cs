namespace ModShark.Rules;

public interface IRule
{
    Task RunRule(CancellationToken stoppingToken);
}