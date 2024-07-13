using FluentAssertions;
using ModShark.Services;
using SharkeyDB.Entities;

namespace ModShark.Tests.Services;

public class LinkServiceTests
{
    private LinkService ServiceUnderTest { get; set; } = null!;
    private SharkeyConfig FakeSharkeyConfig { get; set; } = null!;

    private Instance FakeInstance { get; set; } = null!;
    private User FakeRemoteUser { get; set; } = null!;
    private Note FakeRemoteNote { get; set; } = null!;
    private User FakeLocalUser { get; set; } = null!;
    private Note FakeLocalNote { get; set; } = null!;

    [SetUp]
    public void Setup()
    {
        FakeInstance = new Instance
        {
            Id = "0",
            Host = "fake.example.com",
            SuspensionState = "none"
        };
        FakeRemoteUser = new User
        {
            Id = "0",
            Username = "User",
            UsernameLower = "user",
            Host = FakeInstance.Host,
            Uri = "https://fake.example.com/users/0"
        };
        FakeRemoteNote = new Note
        {
            Id = "0",
            UserId = FakeRemoteUser.Id,
            Visibility = "public",
            Url = "https://fake.example.com/notes/0"
        };
        FakeLocalUser = new User
        {
            Id = "1",
            Username = "User",
            UsernameLower = "user"
        };
        FakeLocalNote = new Note
        {
            Id = "1",
            UserId = FakeLocalUser.Id,
            Visibility = "public"
        };
        
        FakeSharkeyConfig = new SharkeyConfig
        {
            IdFormat = IdFormat.Aid,
            ApiEndpoint = "https://example.com/api",
            PublicHost = "example.com",
            ServiceAccount = "instance.actor"
        };
        ServiceUnderTest = new LinkService(FakeSharkeyConfig);
    }

    [Test]
    public void GetLinkToInstance_ShouldProduceRemoteLink()
    {
        var result = ServiceUnderTest.GetLinkToInstance(FakeInstance);

        result.Should().Be("https://fake.example.com");
    }

    [Test]
    public void GetLocalLinkToInstance_ShouldProduceLocalLink()
    {
        var result = ServiceUnderTest.GetLocalLinkToInstance(FakeInstance);

        result.Should().Be("https://example.com/instance-info/fake.example.com");
    }

    [Test]
    public void GetLinkToUser_ShouldProduceRemoteLink_ForRemoteUser()
    {
        var result = ServiceUnderTest.GetLinkToUser(FakeRemoteUser);

        result.Should().Be("https://fake.example.com/users/0");
    }

    [Test]
    public void GetLinkToUser_ShouldProduceLocalLink_ForLocalUser()
    {
        var result = ServiceUnderTest.GetLinkToUser(FakeLocalUser);

        result.Should().Be("https://example.com/@User");
    }

    [Test]
    public void GetLinkToNote_ShouldProduceRemoteLink_ForRemoteNote()
    {
        var result = ServiceUnderTest.GetLinkToNote(FakeRemoteNote);

        result.Should().Be("https://fake.example.com/notes/0");
    }

    [Test]
    public void GetLinkToNote_ShouldProduceLocalLink_ForLocalNote()
    {
        var result = ServiceUnderTest.GetLinkToNote(FakeLocalNote);

        result.Should().Be("https://example.com/notes/1");
    }

    [Test]
    public void GetLocalLinkToUser_ShouldProduceLocalLink_ForRemoteUser()
    {
        var result = ServiceUnderTest.GetLocalLinkToUser(FakeRemoteUser);

        result.Should().Be("https://example.com/@User@fake.example.com");
    }

    [Test]
    public void GetLocalLinkToUser_ShouldProduceLocalLink_ForLocalUser()
    {
        var result = ServiceUnderTest.GetLocalLinkToUser(FakeLocalUser);

        result.Should().Be("https://example.com/@User");
    }

    [Test]
    public void GetLocalLinkToNote_ShouldProduceLocalLink_ForRemoteNote()
    {
        var result = ServiceUnderTest.GetLocalLinkToNote(FakeRemoteNote);

        result.Should().Be("https://example.com/notes/0");
    }

    [Test]
    public void GetLocalLinkToNote_ShouldProduceLocalLink_ForLocalNote()
    {
        var result = ServiceUnderTest.GetLocalLinkToNote(FakeLocalNote);

        result.Should().Be("https://example.com/notes/1");
    }
}