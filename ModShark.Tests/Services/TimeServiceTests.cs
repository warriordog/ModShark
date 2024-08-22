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

        actualTime.Should().BeCloseTo(expectedTime, TimeSpan.FromSeconds(10));
    }

    [Test]
    public void UtcNow_ShouldReturnCurrentUtcTime()
    {
        var actualTime = ServiceUnderTest.UtcNow;
        var expectedTime = DateTime.UtcNow;

        actualTime.Should().BeCloseTo(expectedTime, TimeSpan.FromSeconds(10));
    }

    [Test]
    public void Delay_ShouldNotCompleteUntilTime()
    {
        var task = ServiceUnderTest.Delay(10, CancellationToken.None);
        
        task.IsCompleted.Should().BeFalse();
    }
}