using FluentAssertions;
using ModShark.Services;

namespace ModShark.Tests.Services;

public class TimeServiceTests
{
    private TimeService ServiceUnderTest { get; set; } = null!;

    [SetUp]
    public void Setup()
    {
        ServiceUnderTest = new TimeService();
    }
    
    [Test]
    public void Now_ShouldReturnCurrentLocalTime()
    {
        var actualTime = ServiceUnderTest.Now;
        var expectedTime = DateTime.Now;

        actualTime.Should().Be(expectedTime);
    }

    [Test]
    public void UtcNow_ShouldReturnCurrentUtcTime()
    {
        var actualTime = ServiceUnderTest.UtcNow;
        var expectedTime = DateTime.UtcNow;

        actualTime.Should().Be(expectedTime);
    }
}