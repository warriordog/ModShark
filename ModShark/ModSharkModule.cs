using ModShark.Reports;
using ModShark.Reports.Reporter;
using ModShark.Rules;
using ModShark.Services;
using SharkeyDB;

namespace ModShark;

public static class ModSharkModule
{
    public static T AddModShark<T>(this T services, IConfiguration configuration)
        where T : IServiceCollection
    {
        // Register dependencies first.
        // Order doesn't really matter, but it makes more sense conceptually.
        services.AddSharkeyDB();
        
        // Read and register config.
        // This will be reworked later.
        var config = configuration
            .GetSection("ModShark")
            .Get<ModSharkConfig>()
            ?? throw new ApplicationException("Configuration file is invalid: could not map to the config object.");
        services.AddSingleton(config.Postgres);
        services.AddSingleton(config.Sharkey);
        services.AddSingleton(config.Worker);
        services.AddSingleton(config.Reporters.SendGrid);
        services.AddSingleton(config.Reporters.Console);
        services.AddSingleton(config.Reporters.Native);
        services.AddSingleton(config.Reporters.Post);
        services.AddSingleton(config.Rules.FlaggedUser);
        services.AddSingleton(config.Rules.FlaggedInstance);
        services.AddSingleton(config.Rules.FlaggedNote);

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
        services.AddHostedService<Worker>();
        
        return services;
    }
}