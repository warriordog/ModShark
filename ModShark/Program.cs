using ModShark;
using ModShark.Rules;
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
builder.Services.AddSingleton(config.Rules.FlaggedUsername);
builder.Services.AddSingleton(config.Worker);
builder.Services.AddScoped<IFlaggedUsernameRule, FlaggedUsernameRule>();
builder.Services.AddHostedService<Worker>();
 
var host = builder.Build();
host.Run();