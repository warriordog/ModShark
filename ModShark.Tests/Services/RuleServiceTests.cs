using Microsoft.Extensions.Logging;
using ModShark.Reports;
using ModShark.Rules;
using ModShark.Services;
using Moq;

namespace ModShark.Tests.Services;

#pragma warning disable CA2254
public class RuleServiceTests
{
    private RuleService ServiceUnderTest { get; set; } = null!;

    private Mock<ILogger<RuleService>> MockLogger { get; set; } = null!;
    private Mock<IFlaggedUserRule> MockFlaggedUsernameRule { get; set; } = null!;
    private Mock<IFlaggedInstanceRule> MockFlaggedHostnameRule { get; set; } = null!;
    private Mock<IFlaggedNoteRule> MockFlaggedNoteRule { get; set; } = null!;
    
    [SetUp]
    public void Setup()
    {
        MockLogger = new Mock<ILogger<RuleService>>();
        MockFlaggedUsernameRule = new Mock<IFlaggedUserRule>();
        MockFlaggedHostnameRule = new Mock<IFlaggedInstanceRule>();
        MockFlaggedNoteRule = new Mock<IFlaggedNoteRule>();

        ServiceUnderTest = new RuleService(MockLogger.Object, MockFlaggedUsernameRule.Object, MockFlaggedHostnameRule.Object, MockFlaggedNoteRule.Object);
    }

    [Test]
    public async Task RunRules_ShouldRunFlaggedUsernameRule()
    {
        var report = new Report();
        
        await ServiceUnderTest.RunRules(report, default);
        
        MockFlaggedUsernameRule.Verify(r => r.RunRule(report, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RunRules_ShouldRunFlaggedHostnameRule()
    {
        var report = new Report();
        
        await ServiceUnderTest.RunRules(report, default);
        
        MockFlaggedHostnameRule.Verify(r => r.RunRule(report, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RunRules_ShouldRunFlaggedNoteRule()
    {
        var report = new Report();
        
        await ServiceUnderTest.RunRules(report, default);
        
        MockFlaggedNoteRule.Verify(r => r.RunRule(report, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RunRules_ShouldHandleExceptions()
    {
        var report = new Report();

        MockFlaggedUsernameRule
            .Setup(r => r.RunRule(report, It.IsAny<CancellationToken>()))
            .Throws<ApplicationException>();
        MockFlaggedHostnameRule
            .Setup(r => r.RunRule(report, It.IsAny<CancellationToken>()))
            .Throws<ApplicationException>();
        MockFlaggedNoteRule
            .Setup(r => r.RunRule(report, It.IsAny<CancellationToken>()))
            .Throws<ApplicationException>();
        
        await ServiceUnderTest.RunRules(report, default);
        
        MockFlaggedUsernameRule.Verify(r => r.RunRule(report, It.IsAny<CancellationToken>()), Times.Once);
        MockFlaggedHostnameRule.Verify(r => r.RunRule(report, It.IsAny<CancellationToken>()), Times.Once);
        MockFlaggedNoteRule.Verify(r => r.RunRule(report, It.IsAny<CancellationToken>()), Times.Once);
        MockLogger.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<ApplicationException>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Exactly(3));

    }
}
#pragma warning restore CA2254