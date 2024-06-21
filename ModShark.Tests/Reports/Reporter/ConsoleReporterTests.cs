using Microsoft.Extensions.Logging;
using ModShark.Reports;
using ModShark.Reports.Reporter;
using ModShark.Tests._Utils;
using Moq;

namespace ModShark.Tests.Reports.Reporter;

public class ConsoleReporterTests
{
    private ConsoleReporter ReporterUnderTest { get; set; } = null!;
    private Mock<ILogger<ConsoleReporter>> MockLogger { get; set; } = null!;

    [SetUp]
    public void Setup()
    {
        MockLogger = new Mock<ILogger<ConsoleReporter>>();
        ReporterUnderTest = new ConsoleReporter(MockLogger.Object);
    }
    
    [Test]
    public async Task MakeReport_ShouldSkip_WhenReportIsEmpty()
    {
        await ReporterUnderTest.MakeReport(new Report(), default);
        
        MockLogger.VerifyLog(LogLevel.Information, Times.Never());
        MockLogger.VerifyLog(LogLevel.Debug, "Skipping console - report is empty", Times.Once());
    }

    [Test]
    public async Task MakeReport_ShouldLogUserReports()
    {
        var report = new Report
        {
            UserReports = 
            {
                new UserReport
                {
                    UserId = "0001",
                    Username = "User1",
                    Hostname = "example.com"
                },
                new UserReport
                {
                    UserId = "0002",
                    Username = "User2",
                    Hostname = null
                }
            }
        };

        await ReporterUnderTest.MakeReport(report, default);
        
        MockLogger.VerifyLog(LogLevel.Information, "Flagged 2 new user(s)", Times.Once());
    }

    [Test]
    public async Task MakeReport_ShouldLogInstanceReports()
    {
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

        await ReporterUnderTest.MakeReport(report, default);
        
        MockLogger.VerifyLog(LogLevel.Information, "Flagged 1 new instance(s)", Times.Once());
    }
}