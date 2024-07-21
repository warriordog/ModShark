using SharkeyDB.Entities;

namespace ModShark.Utils;

public static class InstanceExtensions
{
    /// <summary>
    /// Gets a formatted string containing the instance's software name and version.
    /// If either field is missing, then it is replaced with a placeholder "?".
    /// </summary>
    public static string GetSoftwareString(this Instance instance)
    {
        var name = instance.HasSoftwareName
            ? instance.SoftwareName
            : "?";

        var version = instance.HasSoftwareVersion
            ? instance.SoftwareVersion
            : "?";

        return $"{name} v{version}";
    }
}