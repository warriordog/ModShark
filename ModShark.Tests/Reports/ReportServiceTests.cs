using Microsoft.Extensions.Logging;
using ModShark.Reports;
using ModShark.Reports.Reporter;
using Moq;

namespace ModShark.Tests.Reports;

public class ReportServiceTests
{
    private ReportService ServiceUnderTest { get; set; } = null!;

    private Mock<ILogger<ReportService>> MockLogger { get; set; } = null!;
    private Mock<ISendGridReporter> MockSendGridReporter { get; set; } = null!;
    private Mock<IConsoleReporter> MockConsoleReporter { get; set; } = null!;

    private Report FakeReport { get; set; } = null!;
    
    [SetUp]
    public void Setup()
    {
        FakeReport = new Report
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
        
        MockLogger = new Mock<ILogger<ReportService>>();
        MockSendGridReporter = new Mock<ISendGridReporter>();
        MockConsoleReporter = new Mock<IConsoleReporter>();
        
        ServiceUnderTest = new ReportService(MockLogger.Object, MockSendGridReporter.Object, MockConsoleReporter.Object);
    }
    
    [Test]
    public async Task MakeReports_ShouldSkip_WhenReportIsEmpty()
    {
        await ServiceUnderTest.MakeReports(new Report(), default);
        
        MockSendGridReporter.Verify(r => r.MakeReport(It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Never);
        MockConsoleReporter.Verify(r => r.MakeReport(It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Test]
    public async Task MakeReports_ShouldRunConsoleReporter()
    {
        await ServiceUnderTest.MakeReports(FakeReport, default);
        
        MockConsoleReporter.Verify(r => r.MakeReport(FakeReport, default), Times.Once);
    }
    
    [Test]
    public async Task MakeReports_ShouldRunSendGridReporter()
    {
        await ServiceUnderTest.MakeReports(FakeReport, default);
        
        MockSendGridReporter.Verify(r => r.MakeReport(FakeReport, default), Times.Once);
    }
    
    [Test]
    public async Task MakeReports_ShouldHandleExceptions()
    {
        MockSendGridReporter
            .Setup(r => r.MakeReport(FakeReport, default))
            .Throws<ApplicationException>();
        MockConsoleReporter
            .Setup(r => r.MakeReport(FakeReport, default))
            .Throws<ApplicationException>();

        await ServiceUnderTest.MakeReports(FakeReport, default);
        
        MockSendGridReporter.Verify(r => r.MakeReport(FakeReport, default), Times.Once);
        MockConsoleReporter.Verify(r => r.MakeReport(FakeReport, default), Times.Once);
        MockLogger.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<ApplicationException>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Exactly(2));
    }
}