using FluentAssertions;
using ModShark.Reports;
using ModShark.Reports.Document;
using ModShark.Reports.Render;
using ModShark.Services;
using Moq;
using SharkeyDB.Entities;

namespace ModShark.Tests.Reports.Render;

public class RenderServiceTests
{
    private Mock<ILinkService> MockLinkService { get; set; } = null!;
    private RenderService ServiceUnderTest { get; set; } = null!;
    private Report FakeReport { get; set; } = null!;
    
    [SetUp]
    public void Setup()
    {
        MockLinkService = new Mock<ILinkService>();
        ServiceUnderTest = new RenderService(MockLinkService.Object);

        var instance = new Instance
        {
            Id = "instance1",
            Host = "example.com"
        };
        var remoteUser = new User
        {
            Id = "user1",
            Username = "user1",
            UsernameLower = "user1",
            
            Host = "example.com",
            Instance = instance
        };
        var remoteNote = new Note
        {
            Id = "note1",
            Visibility = Note.VisibilityPublic,

            UserId = remoteUser.Id,
            UserHost = remoteUser.Host,
            User = remoteUser
        };
        var localUser = new User
        {
            Id = "user2",
            Username = "user2",
            UsernameLower = "user2"
        };
        var localNote = new Note
        {
            Id = "note2",
            Visibility = Note.VisibilityPublic,

            UserId = remoteUser.Id,
            UserHost = remoteUser.Host,
            User = remoteUser
        };
        FakeReport = new Report
        {
            InstanceReports =
            {
                new InstanceReport
                {
                    Instance = instance
                }
            },
            UserReports =
            {
                new UserReport
                {
                    User = remoteUser,
                    Instance = instance
                },
                new UserReport
                {
                    User = localUser
                }
            },
            NoteReports =
            {
                new NoteReport
                {
                    Note = remoteNote,
                    User = remoteUser,
                    Instance = instance
                },
                new NoteReport
                {
                    Note = localNote,
                    User = localUser
                }
            }
        };
    }

    [Test]
    public void RenderReport_ShouldRenderTitle()
    {
        var document = ServiceUnderTest
            .RenderReport(FakeReport, DocumentFormat.HTML)
            .ToString();

        document.Should().Contain("<h1>ModShark Report</h1>");
    }

    [Test]
    public void RenderReport_ShouldRenderInstanceReports()
    {
        var document = ServiceUnderTest
            .RenderReport(FakeReport, DocumentFormat.HTML)
            .ToString();

        document.Should().Contain("<h2>Flagged 1 instance:</h2>");
    }

    [Test]
    public void RenderReport_ShouldRenderUserReports()
    {
        var document = ServiceUnderTest
            .RenderReport(FakeReport, DocumentFormat.HTML)
            .ToString();

        document.Should().Contain("<h2>Flagged 2 users:</h2>");
    }

    [Test]
    public void RenderReport_ShouldRenderNoteReports()
    {
        var document = ServiceUnderTest
            .RenderReport(FakeReport, DocumentFormat.HTML)
            .ToString();

        document.Should().Contain("<h2>Flagged 2 notes:</h2>");
    }
}