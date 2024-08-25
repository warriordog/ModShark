using FluentAssertions;
using ModShark.Reports.Document.Format;

namespace ModShark.Tests.Reports.Document.Format;

public class MarkdownFormatTests
{
    [TestCase("", "")]
    [TestCase("Hello, world", "Hello, world")]
    [TestCase("-text", "-text")]
    [TestCase("*text", "*text")]
    [TestCase(" - text", @" \- text")]
    [TestCase(" * text", @" \* text")]
    [TestCase("- text", @"\- text")]
    [TestCase("* text", @"\* text")]
    [TestCase("|", @"\|")]
    [TestCase("#", @"\#")]
    [TestCase(@"\", @"\\")]
    [TestCase(" |", @" \|")]
    [TestCase(" #", @" \#")]
    [TestCase(@" \", @" \\")]
    [TestCase("text |", "text |")]
    [TestCase("text #", "text #")]
    [TestCase(@"text \", @"text \\")]
    [TestCase("<tag>", @"\<tag>")]
    [TestCase("<tag/>", @"\<tag/>")]
    [TestCase("</tag>", @"\</tag>")]
    [TestCase("<tag >", "<tag >")]
    [TestCase("< tag>", "< tag>")]
    [TestCase("< tag >", "< tag >")]
    [TestCase("[not a link](https://example.com)", @"[not a link\](https://example.com)")]
    [TestCase("[not a link]", "[not a link]")]
    public void Text_ShouldEscapeMarkdown(string input, string expected)
    {
        var format = new MarkdownFormat();

        var actual = format.Text(input);

        actual.Should().Be(expected);
    }
}