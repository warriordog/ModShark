﻿using System.Text.RegularExpressions;
using ModShark.Reports;

namespace ModShark.Utils;

public static class FlagExtensions
{
    /// <summary>
    /// Executes a pattern and adds all matches to the ReportFlags collection.
    /// Returns true if any patterns were matched.
    /// </summary>
    public static bool TryAddPattern(this ReportFlags flags, Regex pattern, string input, string category = "text")
    {
        var matches = pattern
            .Matches(input)
            .Select(m => m.Value)
            .ToList();
        
        foreach (var text in matches)
        {
            flags.Text.Add(category, text);
        }
        
        return matches.Count != 0;
    }
    
    /// <summary>
    /// Compares a birthday against an AgeRange and adds any match to the ReportFlags collection.
    /// Returns true if the range was matched.
    /// </summary>
    public static bool TryAddAgeRange(this ReportFlags flags, AgeRange range, DateTime birthday, DateTime today)
    {
        if (!range.IsInRange(birthday, today))
            return false;

        flags.AgeRanges.Add(range);
        return true;
    }
}