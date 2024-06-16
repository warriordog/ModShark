﻿using Microsoft.Extensions.Logging;
using ModShark.Rules;
using ModShark.Services;
using Moq;

namespace ModShark.Tests.Services;

#pragma warning disable CA2254
public class RuleServiceTests
{
    private RuleService ServiceUnderTest { get; set; } = null!;

    private Mock<ILogger<RuleService>> MockLogger { get; set; } = null!;
    private Mock<IFlaggedUsernameRule> MockFlaggedUsernameRule { get; set; } = null!;
    private Mock<IFlaggedHostnameRule> MockFlaggedHostnameRule { get; set; } = null!;
    
    [SetUp]
    public void Setup()
    {
        MockLogger = new Mock<ILogger<RuleService>>();
        MockFlaggedUsernameRule = new Mock<IFlaggedUsernameRule>();
        MockFlaggedHostnameRule = new Mock<IFlaggedHostnameRule>();

        ServiceUnderTest = new RuleService(MockLogger.Object, MockFlaggedUsernameRule.Object, MockFlaggedHostnameRule.Object);
    }

    [Test]
    public async Task RunRules_ShouldRunFlaggedUsernameRule()
    {
        await ServiceUnderTest.RunRules(default);
        
        MockFlaggedUsernameRule.Verify(r => r.RunRule(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RunRules_ShouldRunFlaggedHostnameRule()
    {
        await ServiceUnderTest.RunRules(default);
        
        MockFlaggedHostnameRule.Verify(r => r.RunRule(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RunRules_ShouldHandleExceptions()
    {
        MockFlaggedUsernameRule
            .Setup(r => r.RunRule(It.IsAny<CancellationToken>()))
            .Throws<ApplicationException>();
        MockFlaggedHostnameRule
            .Setup(r => r.RunRule(It.IsAny<CancellationToken>()))
            .Throws<ApplicationException>();
        
        await ServiceUnderTest.RunRules(default);
        
        MockFlaggedUsernameRule.Verify(r => r.RunRule(It.IsAny<CancellationToken>()), Times.Once);
        MockFlaggedHostnameRule.Verify(r => r.RunRule(It.IsAny<CancellationToken>()), Times.Once);
        MockLogger.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<ApplicationException>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Exactly(2));

    }
}
#pragma warning restore CA2254