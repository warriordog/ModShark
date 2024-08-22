using Microsoft.Extensions.Logging;
using ModShark.Reports;
using ModShark.Reports.Reporter;
using ModShark.Reports.Reporter.WebHooks;
using ModShark.Tests._Utils;
using Moq;

namespace ModShark.Tests.Reports.Reporter;

public class WebHookReporterTests
{
    private WebHookReporterConfig ReporterConfig { get; set; } = null!;
    private WebHook Hook1 { get; set; } = null!;
    private WebHook Hook2 { get; set; } = null!;
    
    private Mock<ILogger<WebHookReporter>> MockLogger { get; set; } = null!;
    private Mock<IDiscordPublisher> MockDiscordPublisher { get; set; } = null!;
    
    private WebHookReporter ReporterUnderTest { get; set; } = null!;

    [SetUp]
    public void Setup()
    {
        Hook1 = new WebHook
        {
            MaxLength = 2000,
            Type = WebHookType.Discord,
            Url = "https://example.com/1"
        };
        Hook2 = new WebHook
        {
            MaxLength = 2000,
            Type = WebHookType.Discord,
            Url = "https://example.com/2"
        };
        ReporterConfig = new WebHookReporterConfig
        {
            Enabled = true,
            Hooks =
            [
                Hook1,
                Hook2
            ]
        };
        MockLogger = new Mock<ILogger<WebHookReporter>>();
        MockDiscordPublisher = new Mock<IDiscordPublisher>();

        ReporterUnderTest = new WebHookReporter(MockLogger.Object, ReporterConfig, MockDiscordPublisher.Object);
    }
    
    [Test]
    public async Task MakeReport_ShouldSkip_WhenDisabled()
    {
        ReporterConfig.Enabled = false;
        
        await ReporterUnderTest.MakeReport(new Report(), default);

        MockLogger.VerifyLog(LogLevel.Debug, "Skipping WebHooks - disabled in config", Times.Once());
    }
    
    [Test]
    public async Task MakeReport_ShouldSkip_WhenNoHooksDefined()
    {
        ReporterConfig.Hooks = [];
        
        await ReporterUnderTest.MakeReport(new Report(), default);

        MockLogger.VerifyLog(LogLevel.Warning, "Skipping WebHooks - No hooks defined", Times.Once());
    }
    
    [Test]
    public async Task MakeReport_ShouldRunAllHooks()
    {
        var report = new Report();
        
        await ReporterUnderTest.MakeReport(report, default);
        
        MockDiscordPublisher.Verify(d => d.SendReport(Hook1, report, It.IsAny<CancellationToken>()), Times.Once);
        MockDiscordPublisher.Verify(d => d.SendReport(Hook2, report, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test]
    public async Task MakeReport_ShouldSkipHook_WhenUrlIsMissing()
    {
        Hook1.Url = "";
        
        await ReporterUnderTest.MakeReport(new Report(), default);
        
        MockDiscordPublisher.Verify(d => d.SendReport(Hook1, It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Never);
        MockDiscordPublisher.Verify(d => d.SendReport(Hook2, It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test]
    public async Task MakeReport_ShouldSkipHook_WhenTypeIsInvalid()
    {
        Hook1.Type = (WebHookType)999;
        
        await ReporterUnderTest.MakeReport(new Report(), default);
        
        MockDiscordPublisher.Verify(d => d.SendReport(Hook1, It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Never);
        MockDiscordPublisher.Verify(d => d.SendReport(Hook2, It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [TestCase(99)]
    [TestCase(0)]
    [TestCase(-5)]
    public async Task MakeReport_ShouldSkipHook_WhenMaxLengthIsTooLow(int maxLength)
    {
        Hook1.MaxLength = maxLength;
        
        await ReporterUnderTest.MakeReport(new Report(), default);
        
        MockDiscordPublisher.Verify(d => d.SendReport(Hook1, It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Never);
        MockDiscordPublisher.Verify(d => d.SendReport(Hook2, It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test]
    public async Task MakeReport_ShouldCatchHookExceptions()
    {
        MockDiscordPublisher
            .Setup(d => d.SendReport(It.IsAny<WebHook>(), It.IsAny<Report>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApplicationException());
        
        await ReporterUnderTest.MakeReport(new Report(), default);
        
        MockDiscordPublisher.Verify(d => d.SendReport(Hook1, It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Once);
        MockDiscordPublisher.Verify(d => d.SendReport(Hook2, It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}