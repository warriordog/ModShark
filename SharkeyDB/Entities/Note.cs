using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace SharkeyDB.Entities;

/// <summary>
/// A post or message
/// </summary>
[Table("note")]
public class Note : IEntity<string>
{
    /// <summary>
    /// Unique ID of the note
    /// </summary>
    [Column("id"), MaxLength(32), Key]
    public required string Id { get; set; }
    
    /// <summary>
    /// ID of the author - FK to <see cref="User.Id"/>.
    /// </summary>
    [Column("userId"), MaxLength(32)]
    public required string UserId { get; set; }
    
    /// <summary>
    /// Author of the note
    /// </summary>
    public User? User { get; set; }
    
    /// <summary>
    /// Visibility of the note.
    /// MUST be one of "public", "home", "followers", or "specified".
    /// </summary>
    [Column("visibility")] 
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string Visibility { get; set; }
    
    /// <summary>
    /// Text content of the note.
    /// May be null if the note is empty.
    /// </summary>
    [Column("text")]
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? Text { get; set; }
    
    /// <summary>
    /// Content Warning / subject line of the note.
    /// May be null if the note has no content warning.
    /// </summary>
    [Column("cw"), MaxLength(512)]
    public string? CW { get; set; }
    
    /// <summary>
    /// Human-readable URL to the post.
    /// Will be null if this is a local post.
    /// </summary>
    [Column("url"), MaxLength(512)]
    public string? Url { get; set; }

    [MemberNotNullWhen(false, nameof(Url))]
    public bool IsLocal => Url == null;
    
    public MSQueuedNote? QueuedNote { get; set; }
    public MSFlaggedNote? FlaggedNote { get; set; }
}