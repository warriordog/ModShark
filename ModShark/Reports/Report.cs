using System.Diagnostics.CodeAnalysis;
using ModShark.Utils;
using SharkeyDB.Entities;

namespace ModShark.Reports;

public class Report
{
    public DateTime ReportDate { get; init; } = DateTime.UtcNow;
    public List<UserReport> UserReports { get; init; } = [];
    public List<InstanceReport> InstanceReports { get; init; } = [];
    public List<NoteReport> NoteReports { get; init; } = [];
    
    public bool HasReports => HasUserReports || HasInstanceReports || HasNoteReports;
    public bool HasUserReports => UserReports.Count > 0;
    public bool HasInstanceReports => InstanceReports.Count > 0;
    public bool HasNoteReports => NoteReports.Count > 0;

    public int TotalFlags =>
        InstanceReports.Aggregate(0, (sum, report) => sum + report.Flags.Count) +
        UserReports.Aggregate(0, (sum, report) => sum + report.Flags.Count) +
        NoteReports.Aggregate(0, (sum, report) => sum + report.Flags.Count);
}

public class InstanceReport
{
    public required Instance Instance { get; set; }

    public ReportFlags Flags { get; set; } = new ();
}

public class UserReport
{
    public Instance? Instance { get; set; }
    public required User User { get; set; }
    
    [MemberNotNullWhen(false, nameof(Instance))]
    public bool IsLocal => Instance == null;

    public ReportFlags Flags { get; set; } = new ();
}

public class NoteReport
{
    public Instance? Instance { get; set; }
    public required User User { get; set; }
    public required Note Note { get; set; }
    
    [MemberNotNullWhen(false, nameof(Instance))]
    public bool IsLocal => Instance == null;

    public ReportFlags Flags { get; set; } = new ();
}

/// <summary>
/// Snippets of content that have been flagged by a rule
/// </summary>
public class ReportFlags
{
    public int Count => Text.Count + AgeRanges.Count;
    public bool HasAny => Count > 0;

    /// <summary>
    /// Flagged text, keyed by category.
    /// </summary>
    public MultiMap<string, string> Text { get; set; } = [];
    public bool HasText => Text.Count > 0;

    /// <summary>
    /// Flagged age ranges
    /// </summary>
    public HashSet<AgeRange> AgeRanges { get; set; } = [];
    public bool HasAgeRanges => AgeRanges.Count > 0;
}