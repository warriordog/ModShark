using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ModShark.Reports;
using ModShark.Reports.Reporter;
using ModShark.Services;
using ModShark.Tests._Utils;
using Moq;

// This inspection gets confused by the moqs
// ReSharper disable StructuredMessageTemplateProblem

namespace ModShark.Tests.Reports.Reporter;

public class SendGridReporterTests
{
    private SendGridReporter ServiceUnderTest { get; set; } = null!;
    private SendGridConfig Config { get; set; } = null!;

    private Mock<ILogger<SendGridReporter>> MockLogger { get; set; } = null!;
    private Mock<IHttpService> MockHttpService { get; set; } = null!;

    [SetUp]
    public void Setup()
    {
        MockLogger = new Mock<ILogger<SendGridReporter>>();
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
        ServiceUnderTest = new SendGridReporter(MockLogger.Object, Config, MockHttpService.Object);
    }
    
    
    [Test]
    public async Task MakeReport_ShouldBail_WhenDisabled()
    {
        Config.Enabled = false;

        await ServiceUnderTest.MakeReport(new Report(), default);

        MockLogger.VerifyLog(LogLevel.Debug, "Skipping SendGrid - disabled in config", Times.Once());
        MockHttpService
            .Verify(s => s.PostAsync(
                It.IsAny<string>(),
                It.IsAny<SendGridSend>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
    }

    [Test]
    public async Task MakeReport_ShouldBail_WhenApiKeyIsMissing()
    {
        Config.ApiKey = "";

        await ServiceUnderTest.MakeReport(new Report(), default);
        
        MockLogger.VerifyLog(LogLevel.Warning, "Skipping SendGrid - API key is missing", Times.Once());
        MockHttpService
            .Verify(s => s.PostAsync(
                It.IsAny<string>(),
                It.IsAny<SendGridSend>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
    }
    
    [Test]
    public async Task MakeReport_ShouldBail_WhenFromAddressIsMissing()
    {
        Config.FromAddress = "";

        await ServiceUnderTest.MakeReport(new Report(), default);
        
        MockLogger.VerifyLog(LogLevel.Warning, "Skipping SendGrid - sender address is missing", Times.Once());
        MockHttpService
            .Verify(s => s.PostAsync(
                It.IsAny<string>(),
                It.IsAny<SendGridSend>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
    }
    
    [Test]
    public async Task MakeReport_ShouldBail_WhenToAddressesIsMissing()
    {
        Config.ToAddresses = [];
        
        await ServiceUnderTest.MakeReport(new Report(), default);
        
        MockLogger.VerifyLog(LogLevel.Warning, "Skipping SendGrid - no recipients specified", Times.Once());
        MockHttpService
            .Verify(s => s.PostAsync(
                It.IsAny<string>(),
                It.IsAny<SendGridSend>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
    }

    [Test]
    public async Task MakeReport_ShouldBail_WhenReportIsEmpty()
    {
        await ServiceUnderTest.MakeReport(new Report(), default);

        MockLogger.VerifyLog(LogLevel.Debug, "Skipping SendGrid - report is empty", Times.Once());
        MockHttpService
            .Verify(s => s.PostAsync(
                It.IsAny<string>(),
                It.IsAny<SendGridSend>(),
                It.IsAny<IDictionary<string, string>>(),
                It.IsAny<CancellationToken>()
            ), Times.Never);
    }

    [Test]
    public async Task MakeReport_ShouldMakeRequest()
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
        var report = new Report
        {
            InstanceReports =
            {
                new InstanceReport
                {
                    InstanceId = "abc123",
                    Hostname = "example.com"
                }
            }
        };
            
        await ServiceUnderTest.MakeReport(report, default);
        
        MockHttpService.Verify();
    }
}