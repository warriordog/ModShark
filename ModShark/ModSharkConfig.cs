using ModShark.Rules;
using ModShark.Services;
using SharkeyDB;

namespace ModShark;

public class ModSharkConfig
{
    public required SendGridConfig SendGrid { get; set; }
    public required WorkerConfig Worker { get; set; }
    public required RulesConfig Rules { get; set; }
    public required SharkeyDBConfig Postgres { get; set; }
}

public class RulesConfig
{
    public required FlaggedUsernameConfig FlaggedUsername { get; set; }
}