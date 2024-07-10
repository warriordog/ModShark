using ModShark.Reports;
using ModShark.Rules;

namespace ModShark.Services;

public interface IRuleService
{
    Task RunRules(Report report, CancellationToken stoppingToken);
}

public class RuleService(ILogger<RuleService> logger, IFlaggedUserRule flaggedUserRule, IFlaggedInstanceRule flaggedInstanceRule) : IRuleService
{
    public async Task RunRules(Report report, CancellationToken stoppingToken)
    {
        await RunRule(report, flaggedUserRule, stoppingToken);
        await RunRule(report, flaggedInstanceRule, stoppingToken);
    }

    private async Task RunRule(Report report, IRule rule, CancellationToken stoppingToken)
    {
        try
        {
            await rule.RunRule(report, stoppingToken);
        }
        catch (Exception ex)
        {
            var name = rule.GetType().Name;
            logger.LogError(ex, "Failed to run rule {name}", name);
        }
    }
}