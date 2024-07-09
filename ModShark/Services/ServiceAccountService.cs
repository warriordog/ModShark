using Microsoft.EntityFrameworkCore;
using SharkeyDB;

namespace ModShark.Services;

public interface IServiceAccountService
{
    Task<string?> GetServiceAccountId(CancellationToken stoppingToken);

    Task<string?> GetServiceAccountToken(CancellationToken stoppingToken);
}

public class ServiceAccountService(SharkeyConfig config, SharkeyContext db) : IServiceAccountService
{
    private string? CachedId { get; set; }
    private string? CachedToken { get; set; }
    
    public async Task<string?> GetServiceAccountId(CancellationToken stoppingToken)
        => CachedId ??= await LookupReporterId(stoppingToken);

    private async Task<string?> LookupReporterId(CancellationToken stoppingToken)
        => await db.Users
            .AsNoTracking()
            .Where(u => u.Host == null && u.Username == config.ServiceAccount)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(stoppingToken);

    public async Task<string?> GetServiceAccountToken(CancellationToken stoppingToken)
        => CachedToken ??= await LookupReporterToken(stoppingToken);

    private async Task<string?> LookupReporterToken(CancellationToken stoppingToken)
        => await db.Users
            .AsNoTracking()
            .Where(u => u.Host == null && u.Username == config.ServiceAccount)
            .Select(u => u.Token)
            .FirstOrDefaultAsync(stoppingToken);
}