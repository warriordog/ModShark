﻿using Microsoft.Extensions.Logging;
using ModShark.Reports;
using ModShark.Reports.Reporter;
using ModShark.Tests._Utils;
using Moq;
using SharkeyDB.Entities;

namespace ModShark.Tests.Reports.Reporter;

public class ConsoleReporterTests
{
    private ConsoleReporter ReporterUnderTest { get; set; } = null!;
    private ConsoleReporterConfig FakeReporterConfig { get; set; } = null!;
    private Mock<ILogger<ConsoleReporter>> MockLogger { get; set; } = null!;
    

    [SetUp]
    public void Setup()
    {
        MockLogger = new Mock<ILogger<ConsoleReporter>>();
        FakeReporterConfig = new ConsoleReporterConfig();
        
        ReporterUnderTest = new ConsoleReporter(MockLogger.Object, FakeReporterConfig);
    }
    
    [Test]
    public async Task MakeReport_ShouldSkip_WhenDisabled()
    {
        FakeReporterConfig.Enabled = false;
        
        await ReporterUnderTest.MakeReport(new Report(), default);
        
        MockLogger.VerifyLog(LogLevel.Information, Times.Never());
        MockLogger.VerifyLog(LogLevel.Debug, "Skipping console - disabled in config", Times.Once());
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
                    User = new User
                    {
                        Id = "0001",
                        Username = "User1",
                        UsernameLower = "user1",
                        Host = "example.com"   
                    }
                },
                new UserReport
                {
                    User = new User
                    {
                        Id = "0002",
                        Username = "User2",
                        UsernameLower = "user2",
                        Host = null 
                    }
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
                    Instance = new Instance
                    {
                        Id = "abc123",
                        Host = "example.com"  ,
                        SuspensionState = "none"
                    }
                }
            }
        };

        await ReporterUnderTest.MakeReport(report, default);
        
        MockLogger.VerifyLog(LogLevel.Information, "Flagged 1 new instance(s)", Times.Once());
    }
}