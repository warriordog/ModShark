using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SharkeyDB.Entities;

/// <summary>
/// Queue of new posts pending a check
/// </summary>
[Table("ms_queued_note"), Index(nameof(NoteId), IsUnique = true)]
public class MSQueuedNote : IEntity<int>
{
    /// <summary>
    /// Unique ID of the queue entry.
    /// Can be used for pagination and transaction-free sync.
    /// </summary>
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// ID of the note - FK to <see cref="Note.Id"/>.
    /// </summary>
    [Column("note_id"), MaxLength(32)]
    public required string NoteId { get; set; }
    
    /// <summary>
    /// Note that is queued
    /// </summary>
    public Note? Note { get; set; }
}