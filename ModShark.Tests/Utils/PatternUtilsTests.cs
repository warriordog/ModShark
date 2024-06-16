using FluentAssertions;
using ModShark.Utils;

namespace ModShark.Tests.Utils;

public class PatternUtilsTests
{
    [Test]
    public void AnyOf_ShouldReturnEmptyString_WhenInputIsEmpty()
    {
        var result = PatternUtils.AnyOf(Array.Empty<string>());

        result.Should().Be("");
    }
    
    [Test]
    public void AnyOf_ShouldReturnSingleValue_WhenInputHasOnlyOne()
    {
        const string expected = "a";

        var actual = PatternUtils.AnyOf(new[] { expected });

        actual.Should().Be(expected);
    }
    
    [Test]
    public void AnyOf_ShouldReturnMergedPattern_WhenInputHasMultiple()
    {
        var result = PatternUtils.AnyOf(new[] { "a", "b" });

        result.Should().Be("(a)|(b)");
    }
    
    [TestCase("apple", true)]
    [TestCase("banana", true)]
    [TestCase("plum", false)]
    public void CreateMatcher_ShouldMatchSolePattern(string input, bool isMatch)
    {
        var matcher = PatternUtils.CreateMatcher(new[] { "a" }, 1000);
        var result = matcher.IsMatch(input);

        result.Should().Be(isMatch);

    }

    [TestCase("apple", false)]
    [TestCase("banana", true)]
    [TestCase("plum", true)]
    public void CreateMatcher_ShouldMatchAnyPattern(string input, bool isMatch)
    {
        var matcher = PatternUtils.CreateMatcher(new[] { "b", "lum" }, 1000);
        var result = matcher.IsMatch(input);

        result.Should().Be(isMatch);
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public void CreateMatcher_ShouldMatchDifferentCase_WhenIgnoreCaseIsTrue(bool ignoreCase, bool isMatch)
    {
        var matcher = PatternUtils.CreateMatcher(new[] { "A" }, 1000, ignoreCase);
        var result = matcher.IsMatch("apple");
        
        result.Should().Be(isMatch);
    }

    [Test]
    public void CreateMatcher_ShouldSetTimeout()
    {
        const int expectedMS = 1234;
        
        var matcher = PatternUtils.CreateMatcher(Array.Empty<string>(), expectedMS);

        matcher.MatchTimeout.TotalMilliseconds.Should().Be(expectedMS);
    }

    [Test]
    public void CreateMatcher_ShouldThrow_WhenTimeoutIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            PatternUtils.CreateMatcher(Array.Empty<string>(), -1);
        });
    }
}