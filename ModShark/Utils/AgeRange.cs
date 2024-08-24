using System.Text.RegularExpressions;

namespace ModShark.Utils;

/// <summary>
/// A bounded or unbounded range of ages.
/// </summary>
/// <param name="Start">First age to include in the range. If null, then there is no lower bound.</param>
/// <param name="End">First age to exclude from the range. If null, then there is no upper bound.</param>
public partial record AgeRange(Age? Start, Age? End)
{
    /// <summary>
    /// Given a starting "birthday", returns the earliest date included in this range.
    /// </summary>
    public DateTime GetStartDate(DateTime birthday)
        => Start?.ToDate(birthday) ?? DateTime.MinValue;

    /// <summary>
    /// Given a starting "birthday", returns the date when this range ends.
    /// The returned date is NOT included in the range.
    /// </summary>
    public DateTime GetEndDate(DateTime birthday)
        => End?.ToDate(birthday) ?? DateTime.MaxValue;

    /// <summary>
    /// Returns true if the given age is within this range.
    /// </summary>
    public bool IsInRange(Age age) =>
        (Start == null || Start <= age)
        && (End == null || End > age);

    /// <summary>
    /// Given the current date as reference, returns true if a given "birthday" represents an age that is included in this range.
    /// </summary>
    public bool IsInRange(DateTime birthday, DateTime now)
    {
        if (birthday > now)
            throw new ArgumentOutOfRangeException(nameof(birthday), birthday, "birthday cannot be in the future");
        
        var start = GetStartDate(birthday);
        var end = GetEndDate(birthday);

        return start <= now && end > now;
    }

    public override string ToString()
    {
        var start = Start?.ToString() ?? "0y";

        return End != null
            ? $"{start} - {End}"
            : start;
    }

    public static AgeRange Parse(string input)
    {
        var match = AgeRangeRegex().Match(input);
        if (!match.Success)
            throw new ArgumentException($"Age range is invalid: \"{input}\"", nameof(input));
        
        var start = ParseAgeGroup(match.Groups[1]);
        var end = ParseAgeGroup(match.Groups[2]);

        return new AgeRange(start, end);
    }

    private static Age? ParseAgeGroup(Group group)
    {
        if (!group.Success)
            return null;

        return Age.Parse(group.Value);
    }

    [GeneratedRegex(@"^\s*([\dymd]+)(?:\s*-\s*([\dymd]+))?\s*$", RegexOptions.Compiled, 1000)]
    private static partial Regex AgeRangeRegex();
}