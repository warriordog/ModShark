using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharkeyDB.Entities;

/// <summary>
/// Instance configuration / metadata
/// </summary>
[Table("meta")]
public class Meta
{
    /// <summary>
    /// Meaningless - there is only ever one row
    /// </summary>
    [Column("id"), MaxLength(32), Key]
    public required string Id { get; set; }

    /// <summary>
    /// Array of blocked hostnames
    /// </summary>
    [Column("blockedHosts"), MaxLength(1024)]
    public string[] BlockedHosts { get; set; } = [];

    /// <summary>
    /// Array of silenced hostnames
    /// </summary>
    [Column("silencedHosts"), MaxLength(1024)]
    public string[] SilencedHosts { get; set; } = [];
}