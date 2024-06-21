using ModShark.Reports;

namespace ModShark.Rules;

public interface IRule
{
    Task RunRule(Report report, CancellationToken stoppingToken);
}