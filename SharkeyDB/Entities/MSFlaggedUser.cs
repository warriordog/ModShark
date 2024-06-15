using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharkeyDB.Entities;

/// <summary>
/// ModShark user
/// </summary>
[Table("ms_flagged_user")]
public class MSFlaggedUser
{
    /// <summary>
    /// ID of the user - will map to <see cref="User.Id"/> unless the user has been deleted. 
    /// </summary>
    [Column("user_id"), MaxLength(32), Key]
    public required string UserId { get; set; }
    
    /// <summary>
    /// When the user was checked for flags.
    /// </summary>
    [Column("flagged_at")]
    public DateTime FlaggedAt { get; set; }
}