using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SharkeyDB.Entities;

/// <summary>
/// User that has been flagged.
/// Used to track and avoid repeat notifications.
/// </summary>
[Table("ms_flagged_user"), Index(nameof(UserId), IsUnique = true)]
public class MSFlaggedUser : IEntity<int>
{
    /// <summary>
    /// Unique ID of the flagged user.
    /// Can be used for pagination and transaction-free sync.
    /// </summary>
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// ID of the user.
    /// Maps to <see cref="User.Id"/> unless the user has been deleted.
    /// </summary>
    [Column("user_id"), MaxLength(32)]
    public required string UserId { get; set; }

    /// <summary>
    /// User that was flagged, if it still exists.
    /// </summary>
    public User? User { get; set; }
    
    /// <summary>
    /// Queue entry for the user that was flagged.
    /// </summary>
    public MSQueuedUser? QueuedUser { get; set; }
    
    /// <summary>
    /// When the user was checked for flags.
    /// </summary>
    [Column("flagged_at")]
    public DateTime FlaggedAt { get; set; }
}