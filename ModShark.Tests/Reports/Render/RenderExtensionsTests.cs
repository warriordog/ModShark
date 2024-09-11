using FluentAssertions;
using ModShark.Reports.Document;
using ModShark.Reports.Render;

namespace ModShark.Tests.Reports.Render;

public class RenderExtensionsTests
{
    [Test]
    public void AppendFlaggedText_ShouldRenderMixed()
    {
        var ranges = new List<Range> { new(0, 5), new(11, 13) };
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendFlaggedText("hello, world!", ranges);

        var result = builder.ToString();
        result.Should().Be("**`hello`**`, worl`**`d!`**");
    }

    [Test]
    public void AppendFlaggedText_ShouldRenderAllText()
    {
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendFlaggedText("hello, world!", new List<Range>());

        var result = builder.ToString();
        result.Should().Be("`hello, world!`");
    }

    [Test]
    public void AppendFlaggedText_ShouldRenderAllFlagged()
    {
        var ranges = new List<Range> { Range.All };
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendFlaggedText("hello, world!", ranges);

        var result = builder.ToString();
        result.Should().Be("**`hello, world!`**");
    }

    [Test]
    public void AppendFlaggedText_ShouldIgnoreOutOfRange()
    {
        var ranges = new List<Range> { new(20, 25), Range.StartAt(23) };
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendFlaggedText("hello, world!", ranges);

        var result = builder.ToString();
        result.Should().Be("`hello, world!`");
    }

    [Test]
    public void AppendFlaggedText_ShouldDoNothing_ForEmptyInput()
    {
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendFlaggedText("", new List<Range>());

        var result = builder.ToString();
        result.Should().Be("");
    }
}