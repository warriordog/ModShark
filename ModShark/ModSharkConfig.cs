using JetBrains.Annotations;
using ModShark.Reports.Reporter;
using ModShark.Rules;
using SharkeyDB;

namespace ModShark;

[PublicAPI]
public class ModSharkConfig
{
    public required SendGridConfig SendGrid { get; set; }
    public required ConsoleConfig Console { get; set; }
    public required WorkerConfig Worker { get; set; }
    public required RulesConfig Rules { get; set; }
    public required SharkeyDBConfig Postgres { get; set; }
}

[PublicAPI]
public class RulesConfig
{
    public required FlaggedUsernameConfig FlaggedUsername { get; set; }
    public required FlaggedHostnameConfig FlaggedHostname { get; set; }
}