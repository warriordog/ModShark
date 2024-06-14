using ModShark;
using SharkeyDB;

var builder = Host.CreateApplicationBuilder(args);

// Add local environment
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true);

// Read config
var config = builder.Configuration
    .GetSection("ModShark")
    .Get<ModSharkConfig>()
    ?? throw new ApplicationException("Configuration file is invalid: could not map to the config object.");

builder.Services.AddSharkeyDB(config.Postgres);
builder.Services.AddHostedService<Worker>();
 
var host = builder.Build();
host.Run();