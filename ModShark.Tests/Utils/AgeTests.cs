using FluentAssertions;
using ModShark.Utils;

namespace ModShark.Tests.Utils;

public class AgeTests
{
    [Test]
    public void Comparisons_ShouldRespectMemberPriority()
    {
        var first = new Age(0, 4, 5);
        var second = new Age(1, 2, 3);
        var third = new Age(1, 3, 3);
        var fourth = new Age(1, 3, 4);
        var fourth2 = new Age(1, 3, 4);
        
        first.Should().BeLessThan(second);
        second.Should().BeLessThan(third);
        third.Should().BeLessThan(fourth);
        
        fourth.Should().BeGreaterThan(third);
        third.Should().BeGreaterThan(second);
        second.Should().BeGreaterThan(first);

        fourth2.Should().Be(fourth);
    }

    [Test]
    public void ToDate_ShouldAddYears()
    {
        var birthday = new DateTime(2024, 8, 1);
        var age = new Age(1);

        var result = age.ToDate(birthday);

        var expected = new DateTime(2025, 8, 1);
        result.Should().Be(expected);
    }

    [Test]
    public void ToDate_ShouldAddMonths()
    {
        var birthday = new DateTime(2024, 8, 1);
        var age = new Age(0, 1);

        var result = age.ToDate(birthday);

        var expected = new DateTime(2024, 9, 1);
        result.Should().Be(expected);
    }

    [Test]
    public void ToDate_ShouldAddDays()
    {
        var birthday = new DateTime(2024, 8, 1);
        var age = new Age(0, 0, 1);

        var result = age.ToDate(birthday);

        var expected = new DateTime(2024, 8, 2);
        result.Should().Be(expected);
    }

    [Test]
    public void ToDate_ShouldAddAllValues()
    {
        var birthday = new DateTime(2024, 8, 1);
        var age = new Age(1, 2, 3);

        var result = age.ToDate(birthday);

        var expected = new DateTime(2025, 10, 4);
        result.Should().Be(expected);
    }

    [TestCase("1y2m3d", 1, 2, 3)]
    [TestCase("1y", 1, 0, 0)]
    [TestCase("1m", 0, 1, 0)]
    [TestCase("1d", 0, 0, 1)]
    [TestCase("1y1m", 1, 1, 0)]
    [TestCase("1m1d", 0, 1, 1)]
    [TestCase("1y1d", 1, 0, 1)]
    [TestCase("123y", 123, 0, 0)]
    [TestCase("12y34m56d", 12, 34, 56)]
    [TestCase(" 1y ", 1, 0, 0)]
    public void Parse_ShouldParseValidInputs(string input, int years, int months, int days)
    {
        var expected = new Age(years, months, days);

        var actual = Age.Parse(input);

        actual.Should().Be(expected);
    }

    [TestCase("")]
    [TestCase("    ")]
    [TestCase("1z")]
    [TestCase("Xy")]
    [TestCase("1y2m3d4h")]
    [TestCase("-1y")]
    [TestCase("1 z")]
    [TestCase("1y 2m 3d")]
    public void Parse_ShouldThrow_WhenInputIsInvalid(string input)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            Age.Parse(input);
        });
    }
}