using ModShark;
using ModShark.Reports;
using ModShark.Reports.Reporter;
using ModShark.Rules;
using ModShark.Services;
using SharkeyDB;

var builder = Host.CreateApplicationBuilder(args);

// Simple logging for dev, Systemd logging for production
builder.Logging.ClearProviders();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
    });
}
else
{
    builder.Logging.AddSystemdConsole(options =>
    {
        options.IncludeScopes = true;
        options.UseUtcTimestamp = true;
        options.TimestampFormat = " [yyyy-MM-dd HH:mm:ss] ";
    });
}


// Add local environment
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true);

// Read config
var config = builder.Configuration
    .GetSection("ModShark")
    .Get<ModSharkConfig>()
    ?? throw new ApplicationException("Configuration file is invalid: could not map to the config object.");

builder.Services.AddSharkeyDB(config.Postgres);
builder.Services.AddSingleton(config.Sharkey);
builder.Services.AddSingleton(config.Worker);
builder.Services.AddSingleton(config.Reporters.SendGrid);
builder.Services.AddSingleton(config.Reporters.Console);
builder.Services.AddSingleton(config.Reporters.Native);
builder.Services.AddSingleton(config.Reporters.Post);
builder.Services.AddSingleton(config.Rules.FlaggedUsername);
builder.Services.AddSingleton(config.Rules.FlaggedHostname);

builder.Services.AddHttpClient<IHttpService, HttpService>();
builder.Services.AddScoped<ISharkeyHttpService, SharkeyHttpService>();

builder.Services.AddSingleton<IRandomService, RandomService>();
builder.Services.AddSingleton<ITimeService, TimeService>();
builder.Services.AddSingleton<ISharkeyIdService, SharkeyIdService>();

builder.Services.AddSingleton<IConsoleReporter, ConsoleReporter>();
builder.Services.AddScoped<ISendGridReporter, SendGridReporter>();
builder.Services.AddScoped<INativeReporter, NativeReporter>();
builder.Services.AddScoped<IPostReporter, PostReporter>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<IFlaggedUsernameRule, FlaggedUsernameRule>();
builder.Services.AddScoped<IFlaggedHostnameRule, FlaggedHostnameRule>();
builder.Services.AddScoped<IRuleService, RuleService>();

builder.Services.AddScoped<IMetaService, MetaService>();
builder.Services.AddScoped<IServiceAccountService, ServiceAccountService>();
builder.Services.AddHostedService<Worker>();
 
var host = builder.Build();
host.Run();