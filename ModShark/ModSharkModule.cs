using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModShark.Reports;
using ModShark.Reports.Reporter;
using ModShark.Rules;
using ModShark.Services;
using SharkeyDB;

namespace ModShark;

public static class ModSharkModule
{
    /// <summary>
    /// Registers ModShark into the provided service collection.
    /// </summary>
    public static T AddModShark<T>(this T services, IConfiguration configuration)
        where T : IServiceCollection
    {
        // Register dependencies first.
        // Order doesn't really matter, but it makes more sense conceptually.
        services.AddSharkeyDB();

        // Register all services.
        services.AddHttpClient<IHttpService, HttpService>();
        services.AddScoped<ISharkeyHttpService, SharkeyHttpService>();

        services.AddSingleton<IRandomService, RandomService>();
        services.AddSingleton<ITimeService, TimeService>();
        services.AddSingleton<ISharkeyIdService, SharkeyIdService>();

        services.AddSingleton<IConsoleReporter, ConsoleReporter>();
        services.AddScoped<ISendGridReporter, SendGridReporter>();
        services.AddScoped<INativeReporter, NativeReporter>();
        services.AddScoped<IPostReporter, PostReporter>();
        services.AddScoped<IReportService, ReportService>();

        services.AddScoped<IFlaggedUserRule, FlaggedUserRule>();
        services.AddScoped<IFlaggedInstanceRule, FlaggedInstanceRule>();
        services.AddScoped<IFlaggedNoteRule, FlaggedNoteRule>();
        services.AddScoped<IRuleService, RuleService>();

        services.AddSingleton<ILinkService, LinkService>();
        services.AddScoped<IMetaService, MetaService>();
        services.AddScoped<IServiceAccountService, ServiceAccountService>();
        
        return services;
    }
}