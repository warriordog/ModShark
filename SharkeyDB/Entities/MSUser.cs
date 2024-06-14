using System.ComponentModel.DataAnnotations;

namespace SharkeyDB.Entities;

/// <summary>
/// ModShark user
/// </summary>
public class MSUser
{
    /// <summary>
    /// ID of the user - will map to <see cref="User.Id"/> unless the user has been deleted. 
    /// </summary>
    [MaxLength(32), Key]
    public required string UserId { get; set; }
    
    /// <summary>
    /// When the user was last checked for flags.
    /// </summary>
    public DateTime? LastChecked { get; set; }
    
    /// <summary>
    /// When the user was last flagged.
    /// </summary>
    public DateTime? LastFlagged { get; set; } 
}