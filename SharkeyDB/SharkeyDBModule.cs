using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharkeyDB;

public static class SharkeyDBModule
{
    public static T AddSharkeyDB<T>(this T services, SharkeyDBConfig config)
        where T : IServiceCollection
    {
        // https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-strings#aspnet-core
        services.AddDbContext<SharkeyContext>(options =>
            options.UseNpgsql(config.Connection));

        return services;
    }
}

[PublicAPI]
public class SharkeyDBConfig
{
    public required string Connection { get; set; }
}