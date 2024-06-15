using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharkeyDB.Entities;

/// <summary>
/// Sharkey instance
/// </summary>
[Table("instance")]
public class Instance
{
    /// <summary>
    /// Unique ID of the instance
    /// </summary>
    [Column("id"), MaxLength(32), Key]
    public required string Id { get; set; }
    
    /// <summary>
    /// Hostname of the instance
    /// </summary>
    [Column("host"), MaxLength(512)]
    public required string Host { get; set; }
    
    public MSQueuedInstance? MSQueuedInstance { get; set; }
}