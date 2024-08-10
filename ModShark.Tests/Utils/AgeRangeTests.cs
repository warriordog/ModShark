using FluentAssertions;
using ModShark.Utils;

namespace ModShark.Tests.Utils;

public class AgeRangeTests
{
    [Test]
    public void GetStartDate_ShouldReturnStartDate_WhenSet()
    {
        var birthday = new DateTime(2024, 1, 1);
        var start = new Age(1, 2, 3);
        var expected = start.ToDate(birthday);
            
        var range = new AgeRange(start, null);
        var actual = range.GetStartDate(birthday);

        actual.Should().Be(expected);
    }
    
    [Test]
    public void GetStartDate_ShouldReturnMinDate_WhenNotSet()
    {
        var birthday = new DateTime(2024, 1, 1);
            
        var range = new AgeRange(null, null);
        var actual = range.GetStartDate(birthday);

        actual.Should().Be(DateTime.MinValue);
    }
    
    [Test]
    public void GetEndDate_ShouldReturnEndDate_WhenSet()
    {
        var birthday = new DateTime(2024, 1, 1);
        var end = new Age(1, 2, 3);
        var expected = end.ToDate(birthday);
            
        var range = new AgeRange(null, end);
        var actual = range.GetEndDate(birthday);

        actual.Should().Be(expected);
    }
    
    [Test]
    public void GetEndDate_ShouldReturnMaxDate_WhenNotSet()
    {
        var birthday = new DateTime(2024, 1, 1);
            
        var range = new AgeRange(null, null);
        var actual = range.GetEndDate(birthday);

        actual.Should().Be(DateTime.MaxValue);
    }
    
    [TestCase("1y2m3d - 4y5m6d", 1, 2, 3, 4, 5, 6)]
    [TestCase("1y - 2y", 1, 0, 0, 2, 0, 0)]
    [TestCase("1m - 1y", 0, 1, 0, 1, 0, 0)]
    [TestCase("123y - 456y", 123, 0, 0, 456, 0, 0)]
    [TestCase(" 1y - 3y ", 1, 0, 0, 3, 0, 0)]
    public void Parse_ShouldParseStandardInputs(string input, int y1, int m1, int d1, int y2, int m2, int d2)
    {
        var start = new Age(y1, m1, d1);
        var end = new Age(y2, m2, d2);
        var expected = new AgeRange(start, end);

        var actual = AgeRange.Parse(input);

        actual.Should().Be(expected);
    }

    [Test]
    public void Parse_ShouldAllowEndToBeExcluded()
    {
        var start = new Age(1, 0, 0);
        
        var actual = AgeRange.Parse("1y");

        actual.Start.Should().Be(start);
        actual.End.Should().BeNull();
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

    [TestCase(true, 18, 0, 0)]
    [TestCase(true, 19, 0, 0)]
    [TestCase(true, 18, 1, 0)]
    [TestCase(true, 18, 0, 1)]
    [TestCase(false, 25, 0, 0)]
    [TestCase(false, 17, 11, 30)]
    [TestCase(false, 17, 0, 30)]
    [TestCase(false, 17, 11, 0)]
    [TestCase(false, 17, 1, 1)]
    [TestCase(false, 26, 0, 0)]
    [TestCase(false, 25, 1, 0)]
    [TestCase(false, 25, 0, 1)]
    public void IsInRange_Age_ShouldReturnTrueWhenAgeIsInRange(bool expected, int y, int m, int d)
    {
        var range = new AgeRange(new Age(18, 0, 0), new Age(25, 0, 0));
        var age = new Age(y, m, d);

        var actual = range.IsInRange(age);

        actual.Should().Be(expected);
    }

    [TestCase(true, 18, 0, 0)]
    [TestCase(true, 19, 0, 0)]
    [TestCase(true, 18, 1, 0)]
    [TestCase(true, 18, 0, 1)]
    [TestCase(false, 25, 0, 0)]
    [TestCase(false, 17, 11, 30)]
    [TestCase(false, 17, 0, 30)]
    [TestCase(false, 17, 11, 0)]
    [TestCase(false, 17, 1, 1)]
    [TestCase(false, 26, 0, 0)]
    [TestCase(false, 25, 1, 0)]
    [TestCase(false, 25, 0, 1)]
    public void IsInRange_Date_ShouldReturnTrueWhenInRange(bool expected, int y, int m, int d)
    {
        var range = new AgeRange(new Age(18, 0, 0), new Age(25, 0, 0));
        var birthday = new DateTime(2000, 1, 1);
        var today = birthday.AddYears(y).AddMonths(m).AddDays(d);

        var actual = range.IsInRange(birthday, today);

        actual.Should().Be(expected);
    }

    [Test]
    public void IsInRange_Date_ShouldThrow_WhenBirthdayIsFuture()
    {
        var range = new AgeRange(null, null);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            range.IsInRange(new DateTime(2024, 12, 31), new DateTime(2024, 1, 1));
        });
    }
}