using Microsoft.Extensions.DependencyInjection;

namespace SharkeyDB;

public static class SharkeyDBModule
{
    public static T UseSharkeyDB<T>(this T services)
        where T : IServiceCollection
    {
        services.AddDbContext<SharkeyContext>();

        return services;
    }
}