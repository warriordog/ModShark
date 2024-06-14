using SharkeyDB;

namespace ModShark;

public class ModSharkConfig
{
    public required SharkeyDBModuleConfig Postgres { get; set; }
}