using ModShark.Reports;

namespace ModShark.Rules;

public abstract class RuleConfig
{
    public bool Enabled { get; set; }
}

public interface IRule
{
    Task RunRule(Report report, CancellationToken stoppingToken);
}