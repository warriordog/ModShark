using FluentAssertions;
using ModShark.Services;
using Moq;

namespace ModShark.Tests.Services;

public class SharkeyIdServiceTests
{
    private SharkeyIdService ServiceUnderTest { get; set; } = null!;

    private Mock<IRandomService> MockRandomService { get; set; } = null!;
    private Mock<ITimeService> MockTimeService { get; set; } = null!;
    private SharkeyConfig FakeConfig { get; set; } = null!;
    private DateTime FakeUtcNow { get; set; }
    private DateTime FakeLocalNow { get; set; }

    [SetUp]
    public void Setup()
    {
        FakeConfig = new SharkeyConfig
        {
            IdFormat = IdFormat.AidX,
            ServiceAccount = "instance.actor",
            ApiEndpoint = "http://127.0.0.1:3000"
        };
        FakeUtcNow = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        FakeLocalNow = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Local);
        
        MockRandomService = new Mock<IRandomService>();
        MockRandomService
            .Setup(s => s.GetBytes(It.IsAny<int>()))
            .Returns((int length) => new byte[length]);
        MockRandomService
            .Setup(s => s.GetString(It.IsAny<char[]>(), It.IsAny<int>()))
            .Returns((char[] choices, int length) =>
            {
                var chars = new char[length];
                Array.Fill(chars, choices[0]);
                return new string(chars);
            });
        MockRandomService
            .Setup(s => s.GetHexString(It.IsAny<int>(), It.IsAny<bool>()))
            .Returns((int length, bool _) =>
            {
                var chars = new char[length];
                Array.Fill(chars, '0');
                return new string(chars);
            });
        
        MockTimeService = new Mock<ITimeService>();
        MockTimeService
            .SetupGet(s => s.UtcNow)
            .Returns(FakeUtcNow);
        MockTimeService
            .SetupGet(s => s.Now)
            .Returns(FakeLocalNow);
        
        ServiceUnderTest = new SharkeyIdService(FakeConfig, MockRandomService.Object, MockTimeService.Object);
    }

    [Test]
    public void GenerateId2_ShouldThrow_WhenFormatIsOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ServiceUnderTest.GenerateId(FakeUtcNow, (IdFormat)int.MaxValue);
        });
    }

    [Test]
    public void GenerateId2_ShouldThrow_WhenDateTimeIsLocal()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ServiceUnderTest.GenerateId(FakeLocalNow, IdFormat.AidX);
        });
    }

    [TestCase(IdFormat.Aid, "9tywutc001")]
    [TestCase(IdFormat.AidX, "9tywutc000000001")]
    [TestCase(IdFormat.MeId, "818fd1189400000000000000")]
    [TestCase(IdFormat.MeIdG, "g18fd1189400000000000000")]
    [TestCase(IdFormat.ULId, "01HZ8HH5000000000000000000")]
    [TestCase(IdFormat.ObjectId, "665a64800000000000000000")]
    public void GenerateId2_ShouldGenerateSupportedFormats(IdFormat format, string expectedId)
    {
        var actualId = ServiceUnderTest.GenerateId(FakeUtcNow, format);

        actualId.Should().Be(expectedId);
    }

    [Test]
    public void GenerateId2_ShouldTickCounterBetweenCalls()
    {
        var first = ServiceUnderTest.GenerateId(FakeUtcNow, IdFormat.AidX);
        var second = ServiceUnderTest.GenerateId(FakeUtcNow, IdFormat.AidX);

        first.Should().NotBe(second);
    }
    
    [Test]
    public void GenerateId1_ShouldUseCurrentFormat()
    {
        var actualId = ServiceUnderTest.GenerateId(FakeUtcNow);

        actualId.Should().Be("9tywutc000000001");
    }
    
    [Test]
    public void GenerateId0_ShouldUseCurrentTimeAndFormat()
    {
        var actualId = ServiceUnderTest.GenerateId();

        actualId.Should().Be("9tywutc000000001");
    }
}