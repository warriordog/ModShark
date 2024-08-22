using System.Net;
using ModShark.Services;
using Moq;
using Moq.Protected;

namespace ModShark.Tests.Services;

public class HttpServiceTests
{
    private HttpResponseMessage FakeResponse { get; set; } = null!;
    
    private HttpService ServiceUnderTest { get; set; } = null!;
    
    private HttpClient HttpClient { get; set; } = null!;
    private Mock<HttpMessageHandler> MockHttpMessageHandler { get; set; } = null!;
    
    [SetUp]
    public void Setup()
    {
        FakeResponse = new HttpResponseMessage(HttpStatusCode.OK);
        
        MockHttpMessageHandler = new Mock<HttpMessageHandler>();
        MockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage _, CancellationToken _) => FakeResponse)
            .Verifiable();
        
        HttpClient = new HttpClient(MockHttpMessageHandler.Object);
        ServiceUnderTest = new HttpService(HttpClient);
    }

    [TearDown]
    public void Teardown()
    {
        FakeResponse.Dispose();
        HttpClient.Dispose();
    }
    
    [Test]
    public async Task PostAsync_ShouldAttachDefaultUserAgent_WhenNotSet()
    {
        const string expected = "ModShark (https://github.com/warriordog/ModShark)";
        
        await ServiceUnderTest.PostAsync("https://example.com", CancellationToken.None);
        
        MockHttpMessageHandler
            .Protected()
            .Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>(m => string.Join(" ", m.Headers.GetValues("User-Agent")) == expected), ItExpr.IsAny<CancellationToken>());
    }
    
    [Test]
    public async Task PostAsync_ShouldNotAttachUserAgent_WhenAlreadySet()
    {
        const string expected = "Custom Agent";
        var headers = new Dictionary<string, string>
        {
            ["User-Agent"] = expected
        };
        
        await ServiceUnderTest.PostAsync("https://example.com", CancellationToken.None, headers: headers);
        
        MockHttpMessageHandler
            .Protected()
            .Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), ItExpr.Is<HttpRequestMessage>(m => string.Join(" ", m.Headers.GetValues("User-Agent")) == expected), ItExpr.IsAny<CancellationToken>());
    }
}