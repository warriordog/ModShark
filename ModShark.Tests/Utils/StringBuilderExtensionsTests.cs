using System.Text;
using FluentAssertions;
using ModShark.Utils;

namespace ModShark.Tests.Utils;

public class StringBuilderExtensionsTests
{
    [TestCase("", "", "<a href=\"\" rel=\"noopener noreferer\" target=\"_blank\"></a>")]
    [TestCase("https://example.com", "", "<a href=\"https://example.com\" rel=\"noopener noreferer\" target=\"_blank\"></a>")]
    [TestCase("", "content", "<a href=\"\" rel=\"noopener noreferer\" target=\"_blank\">content</a>")]
    [TestCase("https://example.com", "content", "<a href=\"https://example.com\" rel=\"noopener noreferer\" target=\"_blank\">content</a>")]
    public void AppendHtmlAnchor_String_ShouldBuildCorrectString(string href, string content, string expected)
        => Call(b => b.AppendHtmlAnchor(href, content))
            .Should().Be(expected);

    [TestCase("", "", "<a href=\"\" rel=\"noopener noreferer\" target=\"_blank\"></a>")]
    [TestCase("https://example.com", "", "<a href=\"https://example.com\" rel=\"noopener noreferer\" target=\"_blank\"></a>")]
    [TestCase("", "content", "<a href=\"\" rel=\"noopener noreferer\" target=\"_blank\">content</a>")]
    [TestCase("https://example.com", "content", "<a href=\"https://example.com\" rel=\"noopener noreferer\" target=\"_blank\">content</a>")]
    public void AppendHtmlAnchor_Action_ShouldBuildCorrectString(string href, string content, string expected)
        => Call(b =>
                b.AppendHtmlAnchor(href, () =>
                    b.Append(content)))
            .Should().Be(expected);
    
    [TestCase("", "", "<span style=\"\"></span>")]
    [TestCase("color: red", "", "<span style=\"color: red\"></span>")]
    [TestCase("", "content", "<span style=\"\">content</span>")]
    [TestCase("color: red", "content", "<span style=\"color: red\">content</span>")]
    public void AppendHtmlStyle_String_ShouldBuildCorrectString(string style, string content, string expected)
        => Call(b => b.AppendHtmlStyle(style, content))
            .Should().Be(expected);
    
    [TestCase("", "", "<span style=\"\"></span>")]
    [TestCase("color: red", "", "<span style=\"color: red\"></span>")]
    [TestCase("", "content", "<span style=\"\">content</span>")]
    [TestCase("color: red", "content", "<span style=\"color: red\">content</span>")]
    public void AppendHtmlStyle_Action_ShouldBuildCorrectString(string style, string content, string expected)
        => Call(b
            => b.AppendHtmlStyle(style, () =>
                b.Append(content)))
        .Should().Be(expected);
    
    [TestCase("", "", "<></>")]
    [TestCase("div", "", "<div></div>")]
    [TestCase("", "content", "<>content</>")]
    [TestCase("div", "content", "<div>content</div>")]
    public void AppendHtml_String_ShouldBuildCorrectString(string tag, string content, string expected)
        => Call(b => b.AppendHtml(tag, content))
            .Should().Be(expected);
    
    [TestCase("", "", "<></>")]
    [TestCase("div", "", "<div></div>")]
    [TestCase("", "content", "<>content</>")]
    [TestCase("div", "content", "<div>content</div>")]
    public void AppendHtml_Action_ShouldBuildCorrectString(string tag, string content, string expected)
        => Call(b
            => b.AppendHtml(tag, () =>
                b.Append(content)))
        .Should().Be(expected);
    
    [TestCase("", null, "", "<></>")]
    [TestCase("div", null, "", "<div></div>")]
    [TestCase("", null, "content", "<>content</>")]
    [TestCase("div", null, "content", "<div>content</div>")]
    [TestCase("", "", "", "< ></>")]
    [TestCase("div", "", "", "<div ></div>")]
    [TestCase("", "", "content", "< >content</>")]
    [TestCase("div", "", "content", "<div >content</div>")]
    [TestCase("", "attr", "", "< attr></>")]
    [TestCase("div", "attr", "", "<div attr></div>")]
    [TestCase("", "attr", "content", "< attr>content</>")]
    [TestCase("div", "attr", "content", "<div attr>content</div>")]
    public void AppendHtml_StringString_ShouldBuildCorrectString(string tag, string? attributes, string content, string expected)
        => Call(b => b.AppendHtml(tag,  attributes, content))
            .Should().Be(expected);

    [TestCase("", null, "", "<></>")]
    [TestCase("div", null, "", "<div></div>")]
    [TestCase("", null, "content", "<>content</>")]
    [TestCase("div", null, "content", "<div>content</div>")]
    [TestCase("", "", "", "< ></>")]
    [TestCase("div", "", "", "<div ></div>")]
    [TestCase("", "", "content", "< >content</>")]
    [TestCase("div", "", "content", "<div >content</div>")]
    [TestCase("", "attr", "", "< attr></>")]
    [TestCase("div", "attr", "", "<div attr></div>")]
    [TestCase("", "attr", "content", "< attr>content</>")]
    [TestCase("div", "attr", "content", "<div attr>content</div>")]
    public void AppendHtml_StringAction_ShouldBuildCorrectString(string tag, string? attributes, string content, string expected)
        => Call(b
            => b.AppendHtml(tag, attributes, ()
                => b.Append(content)))
        .Should().Be(expected);

    [TestCase("", "**")]
    [TestCase("test", "*test*")]
    public void AppendMarkdownItalics_String_ShouldBuildCorrectString(string content, string expected)
        => Call(b => b.AppendMarkdownItalics(content))
            .Should().Be(expected);

    [TestCase("", "**")]
    [TestCase("test", "*test*")]
    public void AppendMarkdownItalics_Action_ShouldBuildCorrectString(string content, string expected)
        => Call(b
                => b.AppendMarkdownItalics(() 
                    => b.Append(content)))
            .Should().Be(expected);
    
    [TestCase(false, "", "**")]
    [TestCase(false, "test", "*test*")]
    [TestCase(true, "", "<i></i>")]
    [TestCase(true, "test", "<i>test</i>")]
    public void AppendMarkdownItalics_BoolString_ShouldUseHTML_WhenContentHasPunctuation(bool hasPunctuation, string content, string expected)
        => Call(b => b.AppendMarkdownItalics(hasPunctuation, content))
            .Should().Be(expected);

    [TestCase(false, "", "**")]
    [TestCase(false, "test", "*test*")]
    [TestCase(true, "", "<i></i>")]
    [TestCase(true, "test", "<i>test</i>")]
    public void AppendMarkdownItalics_BoolAction_ShouldUseHTML_WhenContentHasPunctuation(bool hasPunctuation, string content, string expected)
        => Call(b
                => b.AppendMarkdownItalics(hasPunctuation, () 
                    => b.Append(content)))
            .Should().Be(expected);

    [TestCase("", "****")]
    [TestCase("test", "**test**")]
    public void AppendMarkdownBold_String_ShouldBuildCorrectString(string content, string expected)
        => Call(b => b.AppendMarkdownBold(content))
            .Should().Be(expected);

    [TestCase("", "****")]
    [TestCase("test", "**test**")]
    public void AppendMarkdownBold_Action_ShouldBuildCorrectString(string content, string expected)
        => Call(b
                => b.AppendMarkdownBold(() 
                    => b.Append(content)))
            .Should().Be(expected);
    
    [TestCase(false, "", "****")]
    [TestCase(false, "test", "**test**")]
    [TestCase(true, "", "<b></b>")]
    [TestCase(true, "test", "<b>test</b>")]
    public void AppendMarkdownBold_BoolString_ShouldUseHTML_WhenContentHasPunctuation(bool hasPunctuation, string content, string expected)
        => Call(b => b.AppendMarkdownBold(hasPunctuation, content))
            .Should().Be(expected);

    [TestCase(false, "", "****")]
    [TestCase(false, "test", "**test**")]
    [TestCase(true, "", "<b></b>")]
    [TestCase(true, "test", "<b>test</b>")]
    public void AppendMarkdownBold_BoolAction_ShouldUseHTML_WhenContentHasPunctuation(bool hasPunctuation, string content, string expected)
        => Call(b
                => b.AppendMarkdownBold(hasPunctuation, () 
                    => b.Append(content)))
            .Should().Be(expected);

    [TestCase("", "``")]
    [TestCase("test", "`test`")]
    public void AppendMarkdownCode_String_ShouldBuildCorrectString(string content, string expected)
        => Call(b => b.AppendMarkdownCode(content))
            .Should().Be(expected);

    [TestCase("", "``")]
    [TestCase("test", "`test`")]
    public void AppendMarkdownCode_Action_ShouldBuildCorrectString(string content, string expected)
        => Call(b
            => b.AppendMarkdownCode(() 
                => b.Append(content)))
        .Should().Be(expected);
    
    [TestCase("", "", "[]()")]
    [TestCase("https://example.com", "", "[](https://example.com)")]
    [TestCase("", "content", "[content]()")]
    [TestCase("https://example.com", "content", "[content](https://example.com)")]
    public void AppendMarkdownLink_String_ShouldBuildCorrectString(string href, string content, string expected)
        => Call(b => b.AppendMarkdownLink(href, content))
            .Should().Be(expected);

    [TestCase("", "", "[]()")]
    [TestCase("https://example.com", "", "[](https://example.com)")]
    [TestCase("", "content", "[content]()")]
    [TestCase("https://example.com", "content", "[content](https://example.com)")]
    public void AppendMarkdownLink_Action_ShouldBuildCorrectString(string href, string content, string expected)
        => Call(b
            => b.AppendMarkdownLink(href, () =>
                b.Append(content)))
        .Should().Be(expected);
    
    private static string Call(Action<StringBuilder> action)
    {
        var builder = new StringBuilder();
        action(builder);
        return builder.ToString();
    }
}