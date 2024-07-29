using ModShark;

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

builder.Services.AddModShark(builder.Configuration);
 
var host = builder.Build();
host.Run();