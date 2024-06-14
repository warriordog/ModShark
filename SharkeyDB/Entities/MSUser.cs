using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharkeyDB.Entities;

/// <summary>
/// ModShark user
/// </summary>
[Table("ms_user")]
public class MSUser
{
    /// <summary>
    /// ID of the user - will map to <see cref="User.Id"/> unless the user has been deleted. 
    /// </summary>
    [Column("user_id"), MaxLength(32), Key]
    public required string UserId { get; set; }
    
    /// <summary>
    /// When the user was last checked for flags.
    /// </summary>
    [Column("last_checked")]
    public DateTime? LastChecked { get; set; }
    
    /// <summary>
    /// When the user was last flagged.
    /// </summary>
    [Column("last_flagged")]
    public DateTime? LastFlagged { get; set; } 
}