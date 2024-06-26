﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SharkeyDB.Entities;

/// <summary>
/// Queue of new users pending a check
/// </summary>
[Table("ms_queued_user"), Index(nameof(UserId), IsUnique = true)]
public class MSQueuedUser
{
    /// <summary>
    /// Unique ID of the queue entry.
    /// Can be used for pagination and transaction-free sync.
    /// </summary>
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// ID of the user - FK to <see cref="User.Id"/>.
    /// </summary>
    [Column("user_id"), MaxLength(32)]
    public required string UserId { get; set; }
    
    /// <summary>
    /// User who is queued
    /// </summary>
    public required User User { get; set; }
}