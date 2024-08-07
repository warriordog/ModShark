using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModShark;
using ModShark.Main;

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
        
// Read and register config.
// This will be reworked later.
var config = builder.Configuration
    .GetSection("ModShark")
    .Get<ModSharkConfig>()
    ?? throw new ApplicationException("Configuration file is invalid: could not map to the config object.");
builder.Services.AddSingleton(config.Postgres);
builder.Services.AddSingleton(config.Sharkey);
builder.Services.AddSingleton(config.Worker);
builder.Services.AddSingleton(config.Reporters.SendGrid);
builder.Services.AddSingleton(config.Reporters.Console);
builder.Services.AddSingleton(config.Reporters.Native);
builder.Services.AddSingleton(config.Reporters.Post);
builder.Services.AddSingleton(config.Rules.FlaggedUser);
builder.Services.AddSingleton(config.Rules.FlaggedInstance);
builder.Services.AddSingleton(config.Rules.FlaggedNote);

builder.Services.AddModShark(builder.Configuration);
builder.Services.AddHostedService<Worker>();
 
var host = builder.Build();
host.Run();