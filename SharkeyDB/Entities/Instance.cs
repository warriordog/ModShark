using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace SharkeyDB.Entities;

/// <summary>
/// Sharkey instance
/// </summary>
[Table("instance")]
public class Instance : IEntity<string>
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
    
    /// <summary>
    /// Human-readable name of the instance
    /// </summary>
    [Column("name"), MaxLength(256)]
    public string? Name { get; set; }

    [MemberNotNullWhen(true, nameof(Name))]
    public bool HasName => !string.IsNullOrEmpty(Name);
    
    /// <summary>
    /// Human-readable description of the instance
    /// </summary>
    [Column("description"), MaxLength(4096)]
    public string? Description { get; set; }

    [MemberNotNullWhen(true, nameof(Description))]
    public bool HasDescription => !string.IsNullOrEmpty(Description);
    
    /// <summary>
    /// Name of the instance admin
    /// </summary>
    [Column("maintainerName"), MaxLength(128)]
    public string? MaintainerName { get; set; }

    [MemberNotNullWhen(true, nameof(MaintainerName))]
    public bool HasMaintainerName => !string.IsNullOrEmpty(MaintainerName);
    
    /// <summary>
    /// Email address of the instance admin
    /// </summary>
    [Column("maintainerEmail"), MaxLength(256)]
    public string? MaintainerEmail { get; set; }

    [MemberNotNullWhen(true, nameof(MaintainerEmail))]
    public bool HasMaintainerEmail => !string.IsNullOrEmpty(MaintainerEmail);
    
    /// <summary>
    /// Name of the backend software used by the instance
    /// </summary>
    [Column("softwareName"), MaxLength(64)]
    public string? SoftwareName { get; set; }

    [MemberNotNullWhen(true, nameof(SoftwareName))]
    public bool HasSoftwareName => !string.IsNullOrEmpty(SoftwareName);
    
    /// <summary>
    /// Version of the backend software used by the instance
    /// </summary>
    [Column("softwareVersion"), MaxLength(64)]
    public string? SoftwareVersion { get; set; }

    [MemberNotNullWhen(true, nameof(SoftwareVersion))]
    public bool HasSoftwareVersion => !string.IsNullOrEmpty(SoftwareVersion);

    /// <summary>
    /// Status and/or reason of the instance suspension.
    /// This is actually an enum, but we are *not* dealing with that.
    /// </summary>
    [Column("suspensionState"), MaxLength(256)]
    public string SuspensionState { get; set; } = "none";
    
    public MSQueuedInstance? QueuedInstance { get; set; }
    public MSFlaggedInstance? FlaggedInstance { get; set; }
    
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
}