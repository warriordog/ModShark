namespace ModShark.Reports;

public class Report
{
    public DateTime ReportDate { get; init; } = DateTime.UtcNow;
    public List<UserReport> UserReports { get; init; } = [];
    public List<InstanceReport> InstanceReports { get; init; } = [];
    
    public bool HasReports => HasUserReports || HasInstanceReports;
    public bool HasUserReports => UserReports.Count > 0;
    public bool HasInstanceReports => InstanceReports.Count > 0;
}

public class UserReport
{
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public required string? Hostname { get; set; }
    
    public bool IsLocal => Hostname == null;
}

public class InstanceReport
{
    public required string InstanceId { get; set; }
    public required string Hostname { get; set; }
}