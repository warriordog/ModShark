using FluentAssertions;
using ModShark.Reports.Document;
using ModShark.Reports.Render;

namespace ModShark.Tests.Reports.Render;

public class RenderExtensionsTests
{
    [Test]
    public void AppendFullFlaggedText_ShouldRenderMixed()
    {
        var ranges = new List<Range> { new(0, 5), new(11, 13) };
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendFullFlaggedText("hello, world!", ranges);

        var result = builder.ToString();
        result.Should().Be("||**`hello`**`, worl`**`d!`**||");
    }

    [Test]
    public void AppendFullFlaggedText_ShouldRenderAllText()
    {
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendFullFlaggedText("hello, world!", new List<Range>());

        var result = builder.ToString();
        result.Should().Be("||`hello, world!`||");
    }

    [Test]
    public void AppendFullFlaggedText_ShouldRenderAllFlagged()
    {
        var ranges = new List<Range> { Range.All };
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendFullFlaggedText("hello, world!", ranges);

        var result = builder.ToString();
        result.Should().Be("||**`hello, world!`**||");
    }

    [Test]
    public void AppendFullFlaggedText_ShouldIgnoreOutOfRange()
    {
        var ranges = new List<Range> { new(20, 25), Range.StartAt(23) };
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendFullFlaggedText("hello, world!", ranges);

        var result = builder.ToString();
        result.Should().Be("||`hello, world!`||");
    }

    [Test]
    public void AppendFullFlaggedText_ShouldDoNothing_ForEmptyInput()
    {
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendFullFlaggedText("", new List<Range>());

        var result = builder.ToString();
        result.Should().Be("");
    }

    [Test]
    public void AppendMinimalFlaggedText_ShouldIncludeAllSegments()
    {
        var ranges = new List<Range> { new(0, 5), new(11, 13) };
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendMinimalFlaggedText("hello, world!", ranges);

        var result = builder.ToString();
        result.Should().Be("||`hello`||, ||`d!`||");
    }

    [Test]
    public void AppendMinimalFlaggedText_ShouldCapBounds()
    {
        var ranges = new List<Range> { new(0, 5), new(11, 25) };
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendMinimalFlaggedText("hello, world!", ranges);

        var result = builder.ToString();
        result.Should().Be("||`hello`||, ||`d!`||");
    }

    [Test]
    public void AppendMinimalFlaggedText_ShouldIgnoreOutOfRange()
    {
        var ranges = new List<Range> { new(0, 5), new(11, 13), new (25, 30) };
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendMinimalFlaggedText("hello, world!", ranges);

        var result = builder.ToString();
        result.Should().Be("||`hello`||, ||`d!`||");
    }

    [Test]
    public void AppendMinimalFlaggedText_ShouldDoNothing_ForEmptyInput()
    {
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        
        builder.AppendMinimalFlaggedText("", new List<Range>());

        var result = builder.ToString();
        result.Should().Be("");
    }
}