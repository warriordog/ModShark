using Microsoft.Extensions.Logging;
using ModShark.Reports;
using ModShark.Reports.Reporter;
using Moq;
using SharkeyDB.Entities;

namespace ModShark.Tests.Reports;

public class ReportServiceTests
{
    private ReportService ServiceUnderTest { get; set; } = null!;

    private Mock<ILogger<ReportService>> MockLogger { get; set; } = null!;
    private Mock<ISendGridReporter> MockSendGridReporter { get; set; } = null!;
    private Mock<IConsoleReporter> MockConsoleReporter { get; set; } = null!;
    private Mock<INativeReporter> MockNativeReporter { get; set; } = null!;
    private Mock<IPostReporter> MockPostReporter { get; set; } = null!;

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
                    Instance = new Instance
                    {
                        Id = "abc123",
                        Host = "example.com"  ,
                        SuspensionState = "none"
                    }
                }
            }
        };
        
        MockLogger = new Mock<ILogger<ReportService>>();
        MockSendGridReporter = new Mock<ISendGridReporter>();
        MockConsoleReporter = new Mock<IConsoleReporter>();
        MockNativeReporter = new Mock<INativeReporter>();
        MockPostReporter = new Mock<IPostReporter>();
        
        ServiceUnderTest = new ReportService(MockLogger.Object, MockSendGridReporter.Object, MockConsoleReporter.Object, MockNativeReporter.Object, MockPostReporter.Object);
    }
    
    [Test]
    public async Task MakeReports_ShouldSkip_WhenReportIsEmpty()
    {
        await ServiceUnderTest.MakeReports(new Report(), default);
        
        MockSendGridReporter.Verify(r => r.MakeReport(It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Never);
        MockConsoleReporter.Verify(r => r.MakeReport(It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Never);
        MockNativeReporter.Verify(r => r.MakeReport(It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Never);
        MockPostReporter.Verify(r => r.MakeReport(It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Never);
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
    public async Task MakeReports_ShouldRunNativeReporter()
    {
        await ServiceUnderTest.MakeReports(FakeReport, default);
        
        MockNativeReporter.Verify(r => r.MakeReport(FakeReport, default), Times.Once);
    }
    
    [Test]
    public async Task MakeReports_ShouldRunPostReporter()
    {
        await ServiceUnderTest.MakeReports(FakeReport, default);
        
        MockPostReporter.Verify(r => r.MakeReport(FakeReport, default), Times.Once);
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
        MockNativeReporter
            .Setup(r => r.MakeReport(FakeReport, default))
            .Throws<ApplicationException>();
        MockPostReporter
            .Setup(r => r.MakeReport(FakeReport, default))
            .Throws<ApplicationException>();

        await ServiceUnderTest.MakeReports(FakeReport, default);
        
        MockSendGridReporter.Verify(r => r.MakeReport(FakeReport, default), Times.Once);
        MockConsoleReporter.Verify(r => r.MakeReport(FakeReport, default), Times.Once);
        MockNativeReporter.Verify(r => r.MakeReport(FakeReport, default), Times.Once);
        MockPostReporter.Verify(r => r.MakeReport(FakeReport, default), Times.Once);
        MockLogger.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<ApplicationException>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Exactly(4));
    }
}