using System.Text.Json.Serialization;
using JetBrains.Annotations;
using ModShark.Reports.Reporter;
using ModShark.Rules;
using ModShark.Services;
using SharkeyDB;

namespace ModShark;

[PublicAPI]
public class ModSharkConfig
{
    public required SharkeyConfig Sharkey { get; set; }
    public required SharkeyDBConfig Postgres { get; set; }
    public required WorkerConfig Worker { get; set; }
    public required ReportersConfig Reporters { get; set; }
    public required RulesConfig Rules { get; set; }
}

[PublicAPI]
public class SharkeyConfig
{
    [JsonConverter(typeof(JsonStringEnumConverter<IdFormat>))]
    public IdFormat IdFormat { get; set; } = IdFormat.AidX;
    public required string ServiceAccount { get; set; }
    public required string ApiEndpoint { get; set; } 
}

[PublicAPI]
public class ReportersConfig
{
    public required SendGridReporterConfig SendGrid { get; set; }
    public required ConsoleReporterConfig Console { get; set; }
    public required NativeReporterConfig Native { get; set; }
    public required PostReporterConfig Post { get; set; }
}

[PublicAPI]
public class RulesConfig
{
    public required FlaggedUsernameConfig FlaggedUsername { get; set; }
    public required FlaggedHostnameConfig FlaggedHostname { get; set; }
}