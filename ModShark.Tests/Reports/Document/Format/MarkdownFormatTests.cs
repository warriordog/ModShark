using FluentAssertions;
using ModShark.Reports.Document.Format;

namespace ModShark.Tests.Reports.Document.Format;

public class MarkdownFormatTests
{
    [TestCase("", "")]
    [TestCase("Hello, world", "Hello, world")]
    [TestCase("|#()<>[]\\*-", @"\|\#\(\)\<\>\[\]\\\*\-")]
    public void Text_ShouldEscapeMarkdown(string input, string expected)
    {
        var format = new MarkdownFormat();

        var actual = format.Text(input);

        actual.Should().Be(expected);
    }
}