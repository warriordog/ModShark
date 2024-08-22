using FluentAssertions;
using ModShark.Utils;

namespace ModShark.Tests.Utils;

public class TextExtensionsTests
{
    [Test]
    public void IndentShould_Throw_WhenLevelIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            "test".Indent(" ", -1)
        );
    }

    [Test]
    public void IndentShould_ReturnInput_WhenLevelIsZero()
    {
        var result = "hello".Indent("wrong", 0);

        result.Should().Be("hello");
    }

    [TestCase(" ", 1, " hello")]
    [TestCase(" ", 2, "  hello")]
    [TestCase("  ", 3, "      hello")]
    public void IndentShould_ReturnCorrectIndent(string spacer, int level, string expected)
    {
        var actual = "hello".Indent(spacer, level);

        actual.Should().Be(expected);
    }
}