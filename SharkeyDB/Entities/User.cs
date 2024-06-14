using System.ComponentModel.DataAnnotations;

namespace SharkeyDB.Entities;

/// <summary>
/// Sharkey user
/// </summary>
public class User
{
    [MaxLength(32), Key]
    public required string Id { get; set; }
    
    [MaxLength(128)]
    public required string Username { get; set; }
    
    [MaxLength(128)]
    public required string UsernameLower { get; set; }
    
    [MaxLength(500)]
    public string? Host { get; set; }
}