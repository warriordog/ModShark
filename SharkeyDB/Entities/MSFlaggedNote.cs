﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SharkeyDB.Entities;

/// <summary>
/// Note that has been flagged.
/// Used to track and avoid repeat notifications.
/// </summary>
[Table("ms_flagged_note"), Index(nameof(NoteId), IsUnique = true)]
public class MSFlaggedNote : IEntity<int>
{
    /// <summary>
    /// Unique ID of the queue entry.
    /// Can be used for pagination and transaction-free sync.
    /// </summary>
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// ID of the note - will map to <see cref="Note.Id"/> unless the note has been deleted.
    /// Not a foreign key! 
    /// </summary>
    [Column("note_id"), MaxLength(32)]
    public required string NoteId { get; set; }
    
    /// <summary>
    /// When the note was checked for flags.
    /// </summary>
    [Column("flagged_at")]
    public DateTime FlaggedAt { get; set; }
}