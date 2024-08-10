using FluentAssertions;
using ModShark.Reports.Document.Format;

namespace ModShark.Tests.Reports.Document.Format;

public class HTMLFormatTests
{
    [TestCase(null, null, "<a href=\"https://example.com\">")]
    [TestCase("rel", null, "<a href=\"https://example.com\" rel=\"rel\">")]
    [TestCase(null, "target", "<a href=\"https://example.com\" target=\"target\">")]
    [TestCase("rel", "target", "<a href=\"https://example.com\" rel=\"rel\" target=\"target\">")]
    public void LinkStart_ShouldIncludeRelAndTarget_WhenPopulated(string? rel, string? target, string expected)
    {
        var format = new HTMLFormat()
        {
            LinkRel = rel,
            LinkTarget = target
        };

        var link = format.LinkStart("https://example.com");

        link.Should().Be(expected);
    }
}