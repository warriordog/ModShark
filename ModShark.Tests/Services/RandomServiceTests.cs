using FluentAssertions;
using ModShark.Services;

namespace ModShark.Tests.Services;

public class RandomServiceTests
{
    private RandomService ServiceUnderTest { get; set; } = null!;

    [SetUp]
    public void Setup()
    {
        ServiceUnderTest = new RandomService();
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void GetBytes_ShouldReturnCorrectLength(int length)
    {
        var result = ServiceUnderTest.GetBytes(length);
        result.Length.Should().Be(length);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void GetString_ShouldReturnCorrectLength(int length)
    {
        var result = ServiceUnderTest.GetString(['a'], length);
        result.Length.Should().Be(length);
    }

    [TestCase(1, true)]
    [TestCase(1, false)]
    [TestCase(2, true)]
    [TestCase(2, false)]
    [TestCase(3, true)]
    [TestCase(3, false)]
    public void GetHexString_ShouldReturnCorrectLength(int length, bool lowerCase)
    {
        var result = ServiceUnderTest.GetHexString(length, lowerCase);
        result.Length.Should().Be(length);
    }
}