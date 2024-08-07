using Microsoft.EntityFrameworkCore;
using SharkeyDB;
using SharkeyDB.Entities;

namespace ModShark.Services;

public interface IUserService
{
    /// <summary>
    /// Finds a User by username and hostname.
    /// If host is null, then searches local users. 
    /// </summary>
    /// <remarks>
    /// The returned IQueryable is non-tracking.
    /// </remarks>
    IQueryable<User> QueryByUserHost(string username, string? host);
    Task<string?> GetServiceAccountId(CancellationToken stoppingToken);

    Task<string?> GetServiceAccountToken(CancellationToken stoppingToken);
}

public class UserService(SharkeyConfig config, SharkeyContext db) : IUserService
{
    private string? CachedId { get; set; }
    private string? CachedToken { get; set; }

    public IQueryable<User> QueryByUserHost(string username, string? host)
    {
        var usernameLower = username.ToLower();
        var hostLower = host?.ToLower();
        
        return
            db.Users
            .AsNoTracking()
            .Where(u => u.UsernameLower == usernameLower && u.Host == hostLower);
    }

    public async Task<string?> GetServiceAccountId(CancellationToken stoppingToken)
        => CachedId ??= await LookupReporterId(stoppingToken);

    private async Task<string?> LookupReporterId(CancellationToken stoppingToken)
        => await 
            QueryByUserHost(config.ServiceAccount, null)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(stoppingToken);

    public async Task<string?> GetServiceAccountToken(CancellationToken stoppingToken)
        => CachedToken ??= await LookupReporterToken(stoppingToken);

    private async Task<string?> LookupReporterToken(CancellationToken stoppingToken)
        => await
            QueryByUserHost(config.ServiceAccount, null)
            .Select(u => u.Token)
            .FirstOrDefaultAsync(stoppingToken);
}