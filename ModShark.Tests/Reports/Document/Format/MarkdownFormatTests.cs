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
    public void TextInline_ShouldEscapeMarkdown(string input, string expected)
    {
        var format = new MarkdownFormat();

        var actual = format.TextInline(input);

        actual.Should().Be(expected);
    }
    
    [TestCase("", "")]
    [TestCase(" ", " ")]
    [TestCase("\t", " ")]
    [TestCase("\r", " ")]
    [TestCase("\n", " ")]
    [TestCase("\v", " ")]
    public void Text_ShouldEscapeWhitespace(string input, string expected)
    {
        var format = new MarkdownFormat();

        var actual = format.TextInline(input);

        actual.Should().Be(expected);
    }
    
    [TestCase("  ", " ")]
    [TestCase("\t \t", " ")]
    [TestCase("\r \r", " ")]
    [TestCase("\n \n", " ")]
    [TestCase("\v \v", " ")]
    public void Text_ShouldEscapeConsecutiveWhitespace(string input, string expected)
    {
        var format = new MarkdownFormat();

        var actual = format.TextInline(input);

        actual.Should().Be(expected);
    }
}