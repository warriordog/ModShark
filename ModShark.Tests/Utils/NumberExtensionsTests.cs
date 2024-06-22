using FluentAssertions;
using ModShark.Utils;

namespace ModShark.Tests.Utils;

public class NumberExtensionsTests
{
    [TestCase(0, "0")]
    [TestCase(10, "a")]
    [TestCase(35, "z")]
    [TestCase(36, "10")]
    [TestCase(9007199254740991, "2gosa7pa2gv")]
    [TestCase(-0, "0")]
    [TestCase(-10, "-a")]
    [TestCase(-35, "-z")]
    [TestCase(-36, "-10")]
    [TestCase(-9007199254740991, "-2gosa7pa2gv")]
    public void ToBase36String_ShouldConvertNumber(long number, string expectedString)
    {
        var actualString = number.ToBase36String();

        actualString.Should().Be(expectedString);
    }
}