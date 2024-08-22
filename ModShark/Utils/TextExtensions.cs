using System.Text;

namespace ModShark.Utils;

public static class TextExtensions
{
    /// <summary>
    /// Left-pads a string with repeated copies of a specified spacer string.
    /// The spacer will be repeated exactly <see cref="level"/> times.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If level is negative</exception>
    public static string Indent(this string text, string spacer, int level)
    {
        // Bounds check
        if (level < 0)
            throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be at least zero");

        // Happy path optimization
        if (level == 0)
            return text;
        
        // Standard case
        var builder = new StringBuilder();
        for (var i = 0; i < level; i++)
        {
            builder.Append(spacer);
        }
        builder.Append(text);
        return builder.ToString();
    }
}