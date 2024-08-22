using System.Net;
using Microsoft.Extensions.Logging;
using ModShark.Reports;
using ModShark.Reports.Document;
using ModShark.Reports.Render;
using ModShark.Reports.Reporter.WebHooks;
using ModShark.Services;
using ModShark.Tests._Utils;
using Moq;
using SharkeyDB.Entities;

namespace ModShark.Tests.Reports.Reporter.WebHooks;

public class DiscordPublisherTests
{
    private WebHook WebHook { get; set; } = null!;
    private Report FakeReport { get; set; } = null!;
    
    private Mock<ILogger<DiscordPublisher>> MockLogger { get; set; } = null!;
    private Mock<IRenderService> MockRenderService { get; set; } = null!;
    private Mock<ITimeService> MockTimeService { get; set; } = null!;
    
    
    private Mock<IHttpService> MockHttpService { get; set; } = null!;
    private HttpResponseMessage ResponseMessage { get; set; } = null!;

    private DiscordPublisher PublisherUnderTest { get; set; } = null!;

    [SetUp]
    public void Setup()
    {
        WebHook = new WebHook
        {
            MaxLength = 2000,
            Type = WebHookType.Discord,
            Url = "https://example.com/1"
        };
        FakeReport = new Report
        {
            InstanceReports =
            {
                new InstanceReport
                {
                    Instance = new Instance
                    {
                        Id = "123",
                        Host = "example.com"
                    }
                }
            }
        };
        
        MockLogger = new Mock<ILogger<DiscordPublisher>>();
        MockTimeService = new Mock<ITimeService>();
        
        ResponseMessage = new HttpResponseMessage(HttpStatusCode.NoContent);
        MockHttpService = new Mock<IHttpService>();
        MockHttpService
            .Setup(h => h.PostAsync(It.IsAny<string>(), It.IsAny<DiscordExecute>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<string, string>>()))
            .ReturnsAsync((string _, DiscordExecute _, CancellationToken _, IDictionary<string, string> _) => ResponseMessage)
            .Verifiable();
        
        MockRenderService = new Mock<IRenderService>();
        MockRenderService
            .Setup(r => r.RenderReport(It.IsAny<Report>(), It.IsAny<DocumentFormat>()))
            .Returns((Report r, DocumentFormat f) =>
            {
                var doc = new DocumentBuilder(f);
                r.InstanceReports.ForEach(i => doc.Append(i.Instance.Host));
                r.UserReports.ForEach(u => doc.Append(u.User.Username));
                r.NoteReports.ForEach(n => doc.Append(n.Note.Text ?? "null"));
                return doc;
            });

        PublisherUnderTest = new DiscordPublisher(MockLogger.Object, MockHttpService.Object, MockRenderService.Object, MockTimeService.Object);
    }

    [TearDown]
    public void Teardown()
    {
        ResponseMessage.Dispose();
    }
    
    [Test]
    public async Task SendReport_ShouldSkip_WhenMaxLengthIsTooHigh()
    {
        WebHook.MaxLength = 5000;

        await PublisherUnderTest.SendReport(WebHook, FakeReport, CancellationToken.None);
        
        MockLogger.VerifyLog(LogLevel.Warning, "Skipping Discord - MaxLength exceeds Discord's official limits", Times.Once());
    }

    [Test]
    public async Task SendReport_ShouldPostReport()
    {
        await PublisherUnderTest.SendReport(WebHook, FakeReport, CancellationToken.None);
        
        MockHttpService.Verify(h => h.PostAsync(WebHook.Url, It.Is<DiscordExecute>(e => e.Content.Contains("example.com")), CancellationToken.None, It.IsAny<IDictionary<string, string>>()), Times.Once);
    }

    [Test]
    public async Task SendReport_ShouldDisableMentions()
    {
        await PublisherUnderTest.SendReport(WebHook, FakeReport, CancellationToken.None);
        
        MockHttpService.Verify(h => h.PostAsync(WebHook.Url, It.Is<DiscordExecute>(e => e.AllowedMentions.Parse != null), CancellationToken.None, It.IsAny<IDictionary<string, string>>()), Times.Once);

    }

    [Test]
    public async Task SendReport_ShouldDisablePreviews()
    {
        await PublisherUnderTest.SendReport(WebHook, FakeReport, CancellationToken.None);
        
        MockHttpService.Verify(h => h.PostAsync(WebHook.Url, It.Is<DiscordExecute>(e => e.Flags == MessageFlags.SuppressEmbeds), CancellationToken.None, It.IsAny<IDictionary<string, string>>()), Times.Once);
    }

    [Test]
    public async Task SendReport_ShouldSendAllPages()
    {
        WebHook.MaxLength = 10;
        
        await PublisherUnderTest.SendReport(WebHook, FakeReport, CancellationToken.None);
        
        MockHttpService.Verify(h => h.PostAsync(WebHook.Url, It.IsAny<DiscordExecute>(), CancellationToken.None, It.IsAny<IDictionary<string, string>>()), Times.AtLeast(2));
    }

    [TestCase("2", "0", null)]
    [TestCase("1", "0", null)]
    [TestCase("0", "12.3", 12400)]
    [TestCase("-1", "1", 1100)]
    public async Task SendReport_ShouldRespectRateLimits(string remaining, string after, int? expectedDelay)
    {
        ResponseMessage.Headers.Add("X-RateLimit-Remaining", remaining);
        ResponseMessage.Headers.Add("X-RateLimit-Reset-After", after);
        
        await PublisherUnderTest.SendReport(WebHook, FakeReport, CancellationToken.None);
        
        if (expectedDelay == null)
            MockTimeService.Verify(t => t.Delay(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        else
            MockTimeService.Verify(t => t.Delay(expectedDelay.Value, It.IsAny<CancellationToken>()), Times.Once);
    }
}