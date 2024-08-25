﻿using FluentAssertions;
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
                            ["software", "soapbox 1.2.3"] = true,
                            ["description", "free speech community"] = true
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
                            ["text", "slur"] = true
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
                            ["bio", "Age: 12"] = true
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
                            ["text", "kys"] = true
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
                            ["text", "https://forbidden-domain.example.com"] = true,
                            ["emoji", "nsfw_emoji"] = true
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
            .Contain("soapbox 1.2.3")
            .And.Contain("free speech community")
            .And.Contain("slur")
            .And.Contain("Age: 12")
            .And.Contain("kys")
            .And.Contain("https://forbidden-domain.example.com");
    }
}