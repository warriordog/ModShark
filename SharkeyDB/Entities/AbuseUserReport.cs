using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharkeyDB.Entities;

/// <summary>
/// Sharkey report
/// </summary>
[Table("abuse_user_report")]
public class AbuseUserReport : IEntity<string>
{
    /// <summary>
    /// ID of the report - must follow a specific format!
    /// This is used to encode the timestamp for some accursed reason. 
    /// </summary>
    [Column("id"), MaxLength(32), Key]
    public required string Id { get; set; }
    
    /// <summary>
    /// Hostname of the reported user 
    /// </summary>
    [Column("targetUserHost"), MaxLength(512)]
    public required string? TargetUserHost { get; set; }
    
    /// <summary>
    /// ID of the reported user
    /// </summary>
    [Column("targetUserId"), MaxLength(32)]
    public required string TargetUserId { get; set; }
    
    /// <summary>
    /// Reported user
    /// </summary>
    public User? TargetUser { get; set; }
    
    /// <summary>
    /// Hostname of the reporting user
    /// </summary>
    [Column("reporterHost"), MaxLength(512)]
    public required string? ReporterHost { get; set; }
    
    /// <summary>
    /// ID of the reporting user
    /// </summary>
    [Column("reporterId"), MaxLength(32)]
    public required string ReporterId { get; set; }
    
    /// <summary>
    /// Reporting user
    /// </summary>
    public User? Reporter { get; set; }
    
    /// <summary>
    /// ID of the assigned moderator
    /// </summary>
    [Column("assigneeId"), MaxLength(32)]
    public required string? AssigneeId { get; set; }
    
    /// <summary>
    /// Assigned moderator
    /// </summary>
    public User? Assignee { get; set; }
    
    /// <summary>
    /// Report reason
    /// </summary>
    [Column("comment"), MaxLength(2048)]
    public required string Comment { get; set; }
    
}