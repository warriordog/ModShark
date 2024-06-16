using ModShark.Rules;

namespace ModShark.Services;

public interface IRuleService
{
    Task RunRules(CancellationToken stoppingToken);
}

public class RuleService(ILogger<RuleService> logger, IFlaggedUsernameRule flaggedUsernameRule, IFlaggedHostnameRule flaggedHostnameRule) : IRuleService
{
    public async Task RunRules(CancellationToken stoppingToken)
    {
        await RunRule(flaggedUsernameRule, stoppingToken);
        await RunRule(flaggedHostnameRule, stoppingToken);
    }

    private async Task RunRule(IRule rule, CancellationToken stoppingToken)
    {
        try
        {
            await rule.RunRule(stoppingToken);
        }
        catch (Exception ex)
        {
            var name = rule.GetType().Name;
            logger.LogError(ex, "Failed to run {name}", name);
        }
    }
}