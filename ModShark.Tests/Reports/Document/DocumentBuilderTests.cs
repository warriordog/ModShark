using FluentAssertions;
using ModShark.Reports.Document;

namespace ModShark.Tests.Reports.Document;

public class DocumentBuilderTests
{
    [Test]
    public void ToString_ShouldRenderMarkdownDocument()
    {
        var builder = new DocumentBuilder(DocumentFormat.Markdown);

        RenderDocument(builder);
        var document = builder.ToString();
        
        document.Should().Be("# Title1\n# Title2\n\nSection1\n\n## Header1\n## Header2\n**Bold1** **Bold2** *Italics1* *Italics2* `Code1` `Code2`\n[Link1](https://example.com) [Link2](https://example.com)\n* Item1\n* Item2\n  * Item3\n\n");
    }
    
    [Test]
    public void ToString_ShouldRenderMFMDocument()
    {
        var builder = new DocumentBuilder(DocumentFormat.MFM);

        RenderDocument(builder);
        var document = builder.ToString();
        
        document.Should().Be("<b>Title1</b>\n<b>Title2</b>\n\nSection1\n\n<b>Header1</b>\n<b>Header2</b>\n<b>Bold1</b> <b>Bold2</b> <i>Italics1</i> <i>Italics2</i> `Code1` `Code2`\n[Link1](https://example.com) [Link2](https://example.com)\n- Item1\n- Item2\n  - Item3\n\n"); 
    }
    
    [Test]
    public void ToString_ShouldRenderHTMLDocument()
    {
        var builder = new DocumentBuilder(DocumentFormat.HTML);

        RenderDocument(builder);
        var document = builder.ToString();
        
        document.Should().Be("<h1>Title1</h1><h1>Title2</h1><div>Section1</div><div><h2>Header1</h2><h2>Header2</h2><span style=\"font-weight: bold\">Bold1</span> <span style=\"font-weight: bold\">Bold2</span> <span style=\"font-style: italic\">Italics1</span> <span style=\"font-style: italic\">Italics2</span> <code>Code1</code> <code>Code2</code><br><a href=\"https://example.com\">Link1</a> <a href=\"https://example.com\">Link2</a><ul><li>Item1</li><li>Item2</li><li><ul><li>Item3</li></ul></li></ul></div>");
    }
    
    [Test]
    public void ToString_ShouldRenderHTML5Document()
    {
        var builder = new DocumentBuilder(DocumentFormat.HTML5);

        RenderDocument(builder);
        var document = builder.ToString();
        
        document.Should().Be("<h1>Title1</h1><h1>Title2</h1><section>Section1</section><section><h2>Header1</h2><h2>Header2</h2><span style=\"font-weight: bold\">Bold1</span> <span style=\"font-weight: bold\">Bold2</span> <span style=\"font-style: italic\">Italics1</span> <span style=\"font-style: italic\">Italics2</span> <code>Code1</code> <code>Code2</code><br><a href=\"https://example.com\">Link1</a> <a href=\"https://example.com\">Link2</a><ul><li>Item1</li><li>Item2</li><li><ul><li>Item3</li></ul></li></ul></section>");
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ToStrings_ShouldThrow_WhenChunkIsLessThan1(int limit)
    {
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        RenderDocument(builder);
        
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            builder
                .ToStrings(limit)
                .Should()
                .BeEmpty();
        });
    }

    [Test]
    public void ToStrings_ShouldReturnChunksOfCorrectSize()
    {
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        RenderDocument(builder);
        
        var chunks = builder.ToStrings(32);

        chunks.Should().AllSatisfy(c =>
        {
            c.Length.Should().BeLessOrEqualTo(32);
        });
    }

    [Test]
    public void ToStrings_ShouldReturnEntireDocument()
    {
        var builder = new DocumentBuilder(DocumentFormat.Markdown);
        RenderDocument(builder);

        var expected = builder.ToString();
        var actual = string.Join("", builder.ToStrings(32));

        actual.Should().Be(expected);
    }
    
    private void RenderDocument(DocumentBuilder builder)
    {
        builder
            .AppendTitle("Title1")
            .BeginTitle()
                .Append("Title2")
            .End()
            .AppendSection("Section1")
            .BeginSection()
                .AppendHeader("Header1")
                .BeginHeader()
                    .Append("Header2")
                .End()
                .AppendBold("Bold1")
                .Append(" ")
                .BeginBold()
                    .Append("Bold2")
                .End()
                .Append(" ")
                .AppendItalics("Italics1")
                .Append(" ")
                .BeginItalics()
                    .Append("Italics2")
                .End()
                .Append(" ")
                .AppendCode("Code1")
                .Append(" ")
                .BeginCode()
                    .Append("Code2")
                .End()
                .AppendLineBreak()
                .AppendLink("https://example.com", "Link1")
                .Append(" ")
                .BeginLink("https://example.com")
                    .Append("Link2")
                .End()
                .BeginList()
                    .AppendListItem("Item1")
                    .BeginListItem()
                        .Append("Item2")
                    .End()
                    .BeginList()
                        .AppendListItem("Item3")
                    .End()
                .End()
            .End();
    }
}