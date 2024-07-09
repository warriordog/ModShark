using Microsoft.EntityFrameworkCore;
using SharkeyDB;

namespace ModShark.Services;

public interface IServiceAccountService
{
    Task<string?> GetReporterId(CancellationToken stoppingToken);
}

public class ServiceAccountService(SharkeyConfig config, SharkeyContext db) : IServiceAccountService
{
    private string? CachedId { get; set; }
    
    public async Task<string?> GetReporterId(CancellationToken stoppingToken)
        => CachedId ??= await LookupReporterId(stoppingToken);

    private async Task<string?> LookupReporterId(CancellationToken stoppingToken)
        => await db.Users
            .Where(u => u.Host == null && u.Username == config.ServiceAccount)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(stoppingToken);
}