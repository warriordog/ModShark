using System.Diagnostics.CodeAnalysis;
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
}

public class InstanceReport
{
    public required Instance Instance { get; set; }
}

public class UserReport
{
    public Instance? Instance { get; set; }
    public required User User { get; set; }
    
    [MemberNotNullWhen(false, nameof(Instance))]
    public bool IsLocal => Instance == null;
}

public class NoteReport
{
    public Instance? Instance { get; set; }
    public required User User { get; set; }
    public required Note Note { get; set; }
    
    [MemberNotNullWhen(false, nameof(Instance))]
    public bool IsLocal => Instance == null;
}