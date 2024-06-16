using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ModShark.Services;
using Moq;

// This inspection gets confused by the moqs
// ReSharper disable StructuredMessageTemplateProblem

namespace ModShark.Tests.Services;

public class SendGridServiceTests
{
    private SendGridService ServiceUnderTest { get; set; } = null!;
    private SendGridConfig Config { get; set; } = null!;

    private Mock<ILogger<SendGridService>> MockLogger { get; set; } = null!;
    private Mock<IHttpService> MockHttpService { get; set; } = null!;

    [SetUp]
    public void Setup()
    {
        MockLogger = new Mock<ILogger<SendGridService>>();
        MockHttpService = new Mock<IHttpService>();
        MockHttpService
            .Setup(h => h.PostAsync(It.IsAny<string>(), It.IsAny<SendGridSend>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted));

        Config = new SendGridConfig
        {
            Enabled = true,
            ApiKey = "1234",
            FromAddress = "from@example.com",
            ToAddresses =
            [
                "to@example.com"
            ]
        };
        ServiceUnderTest = new SendGridService(MockLogger.Object, Config, MockHttpService.Object);
    }

    // Based on https://stackoverflow.com/a/58697253
    private void VerifyLog(LogLevel level, string message, Times times)
    {
        MockLogger
            .Verify(l => l
                .Log(
                    level,
                    It.IsAny<EventId>(), 
                    It.Is<It.IsAnyType>((o, t) => o.ToString() == message), 
                    It.IsAny<ApplicationException>(), 
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                times
            );
    }
    
    [Test]
    public async Task SendReport_ShouldBail_WhenDisabled()
    {
        Config.Enabled = false;

        await ServiceUnderTest.SendReport("subject", "message", default);

        VerifyLog(LogLevel.Debug, "Skipping email - SendGrid is disabled in config", Times.Once());
    }

    [Test]
    public async Task SendReport_ShouldBail_WhenApiKeyIsMissing()
    {
        Config.ApiKey = "";

        await ServiceUnderTest.SendReport("subject", "message", default);
        
        VerifyLog(LogLevel.Warning, "Skipping email - API key is missing", Times.Once());
    }
    
    [Test]
    public async Task SendReport_ShouldBail_WhenFromAddressIsMissing()
    {
        Config.FromAddress = "";

        await ServiceUnderTest.SendReport("subject", "message", default);
        
        VerifyLog(LogLevel.Warning, "Skipping email - sender address is missing", Times.Once());
    }
    
    [Test]
    public async Task SendReport_ShouldBail_WhenToAddressesIsMissing()
    {
        Config.ToAddresses = [];
        
        await ServiceUnderTest.SendReport("subject", "message", default);
        
        VerifyLog(LogLevel.Warning, "Skipping email - no recipients specified", Times.Once());
    }

    [Test]
    public async Task SendReport_ShouldMakeRequest()
    {
        MockHttpService
            .Setup(h => h.PostAsync(
                It.IsAny<string>(),
                It.IsAny<SendGridSend>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<CancellationToken>()
            ))
            .Callback((string u, SendGridSend b, IDictionary<string, string> h, CancellationToken _) =>
            {
                u.Should().Be("https://api.sendgrid.com/v3/mail/send");

                foreach (var to in Config.ToAddresses)
                    b.Personalizations.Should().Contain(p => p.To.Any(a => a.Email == to));

                h.Should().Contain("Authorization", $"Bearer {Config.ApiKey}");

            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted))
            .Verifiable(Times.Once);
        
        await ServiceUnderTest.SendReport("subject", "message", default);
        
        MockHttpService.Verify();
    }
}