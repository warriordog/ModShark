using System.Net;
using FluentAssertions;
using ModShark.Services;
using ModShark.Tests._Utils;
using Moq;

namespace ModShark.Tests.Services;

public class FetchServiceTests
{
    private SharkeyConfig FakeSharkeyConfig { get; set; } = null!;
    private Mock<IHttpService> MockHttpService { get; set; } = null!;
    private Mock<IFileService> MockFileService { get; set; } = null!;
    private FetchService ServiceUnderTest { get; set; } = null!;

    [SetUp]
    public void Setup()
    {
        MockHttpService = new Mock<IHttpService>();
        MockFileService = new Mock<IFileService>();
        FakeSharkeyConfig = ConfigFakes.MakeSharkeyConfig();
        ServiceUnderTest = new FetchService(FakeSharkeyConfig, MockHttpService.Object, MockFileService.Object);
    }

    [Test]
    public void FetchUrl_ShouldRejectInvalidUrl()
    {
        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await ServiceUnderTest.FetchUrl("http:://example.com/bad-url", CancellationToken.None);
        });
    }

    [TestCase("file")]
    [TestCase("ftp")]
    [TestCase("ssh")]
    public void FetchUrl_ShouldRejectInvalidSchemes(string scheme)
    {
        var testUrl = $"{scheme}://example.com/file";

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await ServiceUnderTest.FetchUrl(testUrl, CancellationToken.None);
        });
    }
    
    [TestCase("http")]
    [TestCase("HTTP")]
    [TestCase("https")]
    [TestCase("HTTPS")]
    public async Task FetchUrl_ShouldDownloadFile_WhenSchemeIsHttp(string scheme)
    {
        var testUrl = $"{scheme}://example.com/url";
        var expectedBytes = new byte[] { 1, 2, 3 };

        MockHttpService
            .Setup(h => h.GetAsync(testUrl, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<string, string?>?>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new ByteArrayContent(expectedBytes)
            });

        await using var stream = await ServiceUnderTest.FetchUrl(testUrl, CancellationToken.None);

        var buffer = new byte[3];
        var read = await stream.ReadAsync(buffer, CancellationToken.None);
        read.Should().Be(3);
        buffer.Should().BeEquivalentTo(expectedBytes);
    }

    [Test]
    public async Task FetchUrl_ShouldDownloadFile_WithCustomUserAgent()
    {
        const string testUrl = "https://example.com/url";

        IDictionary<string, string?>? actualHeaders = null;
        MockHttpService
            .Setup(h => h.GetAsync(testUrl, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<string, string?>?>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new ByteArrayContent([])
            })
            .Callback((string _, CancellationToken _, IDictionary<string, string?>? headers) =>
            {
                actualHeaders = headers;
            });

        await ServiceUnderTest.FetchUrl(testUrl, CancellationToken.None);

        actualHeaders.Should().Contain("User-Agent", null);
    }

    [Test]
    public void FetchUrl_ShouldReject_WhenFetchFails()
    {
        const string testUrl = "https://example.com/url";
        
        MockHttpService
            .Setup(h => h.GetAsync(testUrl, It.IsAny<CancellationToken>(), It.IsAny<IDictionary<string, string?>?>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await ServiceUnderTest.FetchUrl(testUrl, CancellationToken.None);
        });
    }

    [TestCase("123-456")]
    [TestCase("123-456.png")]
    [TestCase("123-456.tar.gz")]
    public async Task FetchUrl_ShouldReadFile_WhenUrlIsLocal(string suffix)
    {
        var testUrl = $"https://example.com/files/{suffix}";
        var expectedPath = Path.Join("/home/sharkey/files", suffix);
        var expectedStream = new MemoryStream([]);
        MockFileService
            .Setup(s => s.OpenRead(expectedPath))
            .Returns(expectedStream);
        
        await using var actualStream = await ServiceUnderTest.FetchUrl(testUrl, CancellationToken.None);
        
        actualStream.Should().BeSameAs(expectedStream);
    }

    [TestCase("https://example.com/files/abc-123", true)]
    [TestCase("https://example.com/files/abc-123.png", true)]
    [TestCase("https://example.com/files/abc-123.", false)]
    [TestCase("https://example.com/files/abc-123.a/", false)]
    [TestCase("https://example.com/files/abc-123/bad", false)]
    [TestCase("https://example.com/bad/abc-123", false)]
    [TestCase("https://other.com/files/abc-123", false)]
    [TestCase("https://bad.example.com/files/abc-123", false)]
    public async Task FetchUrl_ShouldReadFile_OnlyWhenUrlIsCorrect(string testUrl, bool expectLocal)
    {
        MockHttpService
            .Setup(h => h.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<string, string?>?>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new ByteArrayContent([])
            });
        MockFileService
            .Setup(s => s.OpenRead(It.IsAny<string>()))
            .Returns(new MemoryStream([]));
        
        await ServiceUnderTest.FetchUrl(testUrl, CancellationToken.None);

        if (expectLocal)
        {
            MockFileService.Verify(s => s.OpenRead(It.IsAny<string>()), Times.Once);
            MockHttpService.Verify(h => h.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<string, string?>?>()), Times.Never);
        }
        else
        {
            MockFileService.Verify(s => s.OpenRead(It.IsAny<string>()), Times.Never);
            MockHttpService.Verify(h => h.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<IDictionary<string, string?>?>()), Times.Once);
        }
    } 
}