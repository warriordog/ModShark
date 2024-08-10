using System.Globalization;
using System.Text.RegularExpressions;

namespace ModShark.Utils;

/// <summary>
/// An age, specified in loose human terms.
/// </summary>
public partial record Age(int Years = 0, int Months = 0, int Days = 0) : IComparable<Age>, IComparable
{
    /// <summary>
    /// Given a starting "birthday", returns the date when this age will be reached.
    /// </summary>
    public DateTime ToDate(DateTime birthday)
    {
        
        if (Years > 0) 
            birthday = birthday.AddYears(Years);

        if (Months > 0)
            birthday = birthday.AddMonths(Months);

        if (Days > 0)
            birthday = birthday.AddDays(Days);


        return birthday;
    }
    
    public static Age Parse(string input)
    {
        var match = AgeRegex().Match(input);
        if (!match.Success)
            throw new ArgumentException($"Age is invalid: \"{input}\"", nameof(input));
        
        var yearGroup = match.Groups[1];
        var monthGroup = match.Groups[2];
        var dayGroup = match.Groups[3];
        if (!yearGroup.Success && !monthGroup.Success && !dayGroup.Success)
            throw new ArgumentException($"Age is invalid: \"{input}\"", nameof(input));
        
        var year = ParseAgeGroup(yearGroup);
        var month = ParseAgeGroup(monthGroup);
        var day = ParseAgeGroup(dayGroup);

        return new Age(year, month, day);
    }

    private static int ParseAgeGroup(Group group)
    {
        if (!group.Success)
            return 0;

        // Use spans to avoid extra copy operations.
        var valueString = group.ValueSpan[..^1];
        return int.Parse(valueString, NumberStyles.None);
    }

    public int CompareTo(Age? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var yearsComparison = Years.CompareTo(other.Years);
        if (yearsComparison != 0) return yearsComparison;
        var monthsComparison = Months.CompareTo(other.Months);
        if (monthsComparison != 0) return monthsComparison;
        return Days.CompareTo(other.Days);
    }

    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(null, obj)) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        return obj is Age other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Age)}");
    }

    public static bool operator <(Age? left, Age? right)
    {
        return Comparer<Age>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(Age? left, Age? right)
    {
        return Comparer<Age>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(Age? left, Age? right)
    {
        return Comparer<Age>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(Age? left, Age? right)
    {
        return Comparer<Age>.Default.Compare(left, right) >= 0;
    }

    [GeneratedRegex(@"^\s*(\d+y)?(\d+m)?(\d+d)?\s*$", RegexOptions.Compiled, 1000)]
    private static partial Regex AgeRegex();
}