﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SharkeyDB.Entities;

/// <summary>
/// An instance that has been flagged for reporting
/// </summary>
[Table("ms_flagged_instance"), Index(nameof(InstanceId), IsUnique = true)]
public class MSFlaggedInstance : IEntity<int>
{
    /// <summary>
    /// Unique ID of the flagged instance.
    /// Can be used for pagination and transaction-free sync.
    /// </summary>
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// Sharkey instance ID.
    /// Maps to <see cref="Instance.Id"/> unless the instance has been deleted.
    /// </summary>
    [Column("instance_id"), MaxLength(32)]
    public required string InstanceId { get; set; }
    
    /// <summary>
    /// Instance that was flagged, if it still exists.
    /// </summary>
    public Instance? Instance { get; set; }
    
    /// <summary>
    /// Queue entry for the instance that was flagged.
    /// </summary>
    public MSQueuedInstance? QueuedInstance { get; set; }
    
    /// <summary>
    /// When the user was checked for flags.
    /// </summary>
    [Column("flagged_at")]
    public DateTime FlaggedAt { get; set; }
}