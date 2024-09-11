using FluentAssertions;
using ModShark.Reports;
using ModShark.Reports.Document;
using ModShark.Reports.Render;
using ModShark.Services;
using ModShark.Utils;
using Moq;
using SharkeyDB.Entities;
using Range = System.Range;

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
        MockLinkService
            .Setup(s => s.GetLinkToInstance(It.IsAny<Instance>()))
            .Returns((Instance i) => $"https://example.com/instance/{i.Id}");
        MockLinkService
            .Setup(s => s.GetLocalLinkToInstance(It.IsAny<Instance>()))
            .Returns((Instance i) => $"https://example.com/local/instance/{i.Id}");
        MockLinkService
            .Setup(s => s.GetLinkToUser(It.IsAny<User>()))
            .Returns((User u) => $"https://example.com/user/{u.Id}");
        MockLinkService
            .Setup(s => s.GetLocalLinkToUser(It.IsAny<User>()))
            .Returns((User u) => $"https://example.com/local/user/{u.Id}");
        MockLinkService
            .Setup(s => s.GetLinkToNote(It.IsAny<Note>()))
            .Returns((Note n) => $"https://example.com/note/{n.Id}");
        MockLinkService
            .Setup(s => s.GetLocalLinkToNote(It.IsAny<Note>()))
            .Returns((Note n) => $"https://example.com/local/note/{n.Id}");
        
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
                    Instance = instance,
                    Flags =
                    {
                        Text =
                        {
                            ["software"] = new MultiMap<string, Range>
                            {
                                { "soapbox 1.2.3", Range.EndAt(7) }
                            },
                            ["description"] = new MultiMap<string, Range>
                            {
                                { "free speech community", Range.EndAt(12) }
                            }
                        }
                    }
                }
            },
            UserReports =
            {
                new UserReport
                {
                    User = remoteUser,
                    Instance = instance,
                    Flags =
                    {
                        Text =
                        {
                            ["text"] = new MultiMap<string, Range>
                            {
                                { "slur", Range.All }
                            }
                        }
                    }
                },
                new UserReport
                {
                    User = localUser,
                    Flags =
                    {
                        Text =
                        {
                            ["bio"] = new MultiMap<string, Range>
                            {
                                { "Age: 12", Range.All }
                            }
                        }
                    }
                }
            },
            NoteReports =
            {
                new NoteReport
                {
                    Note = remoteNote,
                    User = remoteUser,
                    Instance = instance,
                    Flags =
                    {
                        Text =
                        {
                            ["text"] = new MultiMap<string, Range>
                            {
                                { "kys", Range.All }
                            }
                        }
                    }
                },
                new NoteReport
                {
                    Note = localNote,
                    User = localUser,
                    Flags =
                    {
                        Text =
                        {
                            ["text"] = new MultiMap<string, Range>
                            {
                                { "https://forbidden-domain.example.com", Range.StartAt(9) }
                            },
                            ["emoji"] = new MultiMap<string, Range>
                            {
                                { "nsfw_emoji", Range.All }
                            }
                        }
                    }
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

        document.Should().Contain("ModShark Report");
    }

    [Test]
    public void RenderReport_ShouldRenderInstanceReports()
    {
        var document = ServiceUnderTest
            .RenderReport(FakeReport, DocumentFormat.HTML)
            .ToString();

        document.Should()
            .Contain("Flagged 1 instance:")
            .And.Contain("instance1")
            .And.Contain("example.com");
    }

    [Test]
    public void RenderReport_ShouldRenderUserReports()
    {
        var document = ServiceUnderTest
            .RenderReport(FakeReport, DocumentFormat.HTML)
            .ToString();

        document.Should()
            .Contain("Flagged 2 users:")
            .And.Contain("user1")
            .And.Contain("user2");
    }

    [Test]
    public void RenderReport_ShouldRenderNoteReports()
    {
        var document = ServiceUnderTest
            .RenderReport(FakeReport, DocumentFormat.HTML)
            .ToString();

        document.Should()
            .Contain("Flagged 2 notes:")
            .And.Contain("note1")
            .And.Contain("note2");
    }

    [Test]
    public void RenderReport_ShouldIncludeFlaggedText()
    {
        var document = ServiceUnderTest
            .RenderReport(FakeReport, DocumentFormat.HTML)
            .ToString();

        document.Should()
            .Contain("soapbox")
            .And.Contain("free speech")
            .And.Contain("slur")
            .And.Contain("Age: 12")
            .And.Contain("kys")
            .And.Contain("forbidden-domain.example.com");
    }
}