using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SharkeyDB.Entities;

/// <summary>
/// Queue of new instances pending a check
/// </summary>
[Table("ms_queued_instance"), Index(nameof(InstanceId), IsUnique = true)]
public class MSQueuedInstance : IEntity<int>
{
    /// <summary>
    /// Unique ID of the queued instance.
    /// Can be used for pagination and transaction-free sync.
    /// </summary>
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// Sharkey instance ID
    /// </summary>
    [Column("instance_id"), MaxLength(32)]
    public required string InstanceId { get; set; }
    
    /// <summary>
    /// Instance that is queued
    /// </summary>
    public Instance? Instance { get; set; }
    
    /// <summary>
    /// Flag record for the queued instance
    /// </summary>
    public MSFlaggedInstance? FlaggedInstance { get; set; }
}