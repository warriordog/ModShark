using Microsoft.EntityFrameworkCore;
using SharkeyDB;
using SharkeyDB.Entities;

namespace ModShark.Services;

public interface IMetaService
{
    Task<Meta> GetInstanceMeta(CancellationToken stoppingToken, bool force = false);
}

public class MetaService(ILogger<MetaService> logger, SharkeyContext db) : IMetaService
{
    private Meta? CachedMeta { get; set; }
    
    public async Task<Meta> GetInstanceMeta(CancellationToken stoppingToken, bool force = false)
    {
        if (CachedMeta == null || force)
        {
            CachedMeta = await FetchInstanceMeta(stoppingToken);
        }

        return CachedMeta;
    }

    private async Task<Meta> FetchInstanceMeta(CancellationToken stoppingToken)
    {
        var meta = await db.Metas
            .OrderBy(m => m.Id)
            .FirstOrDefaultAsync(stoppingToken);
        
        if (meta == null)
        {
            logger.LogWarning("Could not loading instance meta table - using placeholder");
            meta = new Meta
            {
                // This is the default value used by Sharkey
                Id = "x"
            };
        }

        return meta;
    }
}