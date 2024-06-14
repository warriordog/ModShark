using System.ComponentModel;
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
    /// When the user was checked for flags.
    /// </summary>
    [Column("checked_at")]
    public DateTime? CheckedAt { get; set; }
    
    /// <summary>
    /// True if the user was flagged.
    /// </summary>
    [Column("is_flagged"), DefaultValue(false)]
    public bool IsFlagged { get; set; } 
}