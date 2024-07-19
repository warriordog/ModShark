using SharkeyDB.Entities;

namespace ModShark.Utils;

public static class NoteExtensions
{
    /// <summary>
    /// Returns the note's emojis in longcode format.
    /// If the note is local, then returns the emoji list as-is.
    /// </summary>
    public static IEnumerable<string> GetEmojiLongcodes(this Note note)
        => note.UserHost != null
            ? note.Emojis.Select(shortcode => $"{shortcode}@{note.UserHost}")
            : note.Emojis;
}