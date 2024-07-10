﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharkeyDB.Entities;

/// <summary>
/// Sharkey user
/// </summary>
[Table("user")]
public class User : IEntity<string>
{
    [Column("id"), MaxLength(32), Key]
    public required string Id { get; set; }
    
    [Column("username"), MaxLength(128)]
    public required string Username { get; set; }
    
    [Column("usernameLower"), MaxLength(128)]
    public required string UsernameLower { get; set; }
    
    [Column("host"), MaxLength(500)]
    public string? Host { get; set; }
    
    [Column("isDeleted")]
    public bool IsDeleted { get; set; }
    
    [Column("isSuspended")]
    public bool IsSuspended { get; set; }
    
    [Column("isSilenced")]
    public bool IsSilenced { get; set; }
    
    [Column("token"), MaxLength(16)]
    public string? Token { get; set; }
    
    public MSQueuedUser? MSQueuedUser { get; set; }
    public Instance? Instance { get; set; }
    
    public ICollection<AbuseUserReport> ReportsBy { get; set; } = new List<AbuseUserReport>();
    public ICollection<AbuseUserReport> ReportsAgainst { get; set; } = new List<AbuseUserReport>();
    public ICollection<AbuseUserReport> ReportsAssignedTo { get; set; } = new List<AbuseUserReport>();
}