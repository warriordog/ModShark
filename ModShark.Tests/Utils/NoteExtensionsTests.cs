using FluentAssertions;
using ModShark.Utils;
using SharkeyDB.Entities;

namespace ModShark.Tests.Utils;

public class NoteExtensionsTests
{
    private static Note FakeNote(string? userHost)
        => new()
        {
            Id = "1",
            UserId = "1",
            Visibility = Note.VisibilityPublic,
            UserHost = userHost
        };
    
    [TestCase(null)]
    [TestCase("example.com")]
    public void GetEmojiLongcodes_ShouldReturnEmptyCollection_WhenNoteHasNoEmojis(string? userHost)
    {
        var note = FakeNote(userHost);

        var longcodes = note.GetEmojiLongcodes();

        longcodes.Should().BeEmpty();
    }
    
    [Test]
    public void GetEmojiLongcodes_ShouldReturnShortcodes_ForLocalNote()
    {
        var note = FakeNote(null);
        note.Emojis =
        [
            "foo",
            "bar",
            "neofox"
        ];

        var longcodes = note.GetEmojiLongcodes();

        longcodes.Should().BeEquivalentTo(note.Emojis);
    }
    
    [Test]
    public void GetEmojiLongcodes_ShouldReturnLongcodes_ForRemoteNote()
    {
        var note = FakeNote("example.com");
        note.Emojis =
        [
            "foo",
            "bar",
            "neofox"
        ];

        var longcodes = note.GetEmojiLongcodes();

        longcodes.Should().BeEquivalentTo(
            [
                "foo@example.com",
                "bar@example.com",
                "neofox@example.com"
            ]
        );
    }
}