using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace SharkeyDB.Entities;

/// <summary>
/// User's profile information
/// </summary>
[Table("user_profile")]
public class UserProfile
{
    /// <summary>
    /// ID of the user - FK to <see cref="User.Id"/>.
    /// </summary>
    [Column("userId"), MaxLength(32), Key]
    public required string UserId { get; set; }
    
    /// <summary>
    /// The user whose profile this is
    /// </summary>
    public User? User { get; set; }

    [MemberNotNullWhen(true, nameof(User))]
    public bool HasUser => User != null;
    
    /// <summary>
    /// User's bio text
    /// </summary>
    [Column("description"), MaxLength(2048)]
    public string? Description { get; set; }

    [MemberNotNullWhen(true, nameof(Description))]
    public bool HasDescription => !string.IsNullOrEmpty(Description);
    
    /// <summary>
    /// User's birthday, or null if not set.
    /// Time is ignored - only the date is preserved.
    /// </summary>
    [Column("birthday"), MaxLength(10)]
    public DateTime? Birthday { get; set; }
    
    [MemberNotNullWhen(true, nameof(Birthday))]
    public bool HasBirthday => Birthday != null;

}