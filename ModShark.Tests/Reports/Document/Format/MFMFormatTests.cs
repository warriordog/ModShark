using FluentAssertions;
using ModShark.Reports.Document.Format;

namespace ModShark.Tests.Reports.Document.Format;

public class MFMFormatTests
{
    [TestCase("", "")]
    [TestCase("Hello, world", "Hello, world")]
    [TestCase("#()<>[]\\*-", "#()<>[]\\*-")]
    public void Text_ShouldNotEscape(string input, string expected)
    {
        var format = new MFMFormat();

        var actual = format.Text(input);

        actual.Should().Be(expected);
    }
}