using System.Net.Http.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace ModShark.Services;

public interface ISharkeyHttpService
{
    Task ReportAbuse(string userId, string comment, CancellationToken stoppingToken);
    Task ReportAbuse(ReportAbuseRequest request, CancellationToken stoppingToken);

    Task<CreateNoteResponse> CreateNote(string text, string visibility, CancellationToken stoppingToken, bool localOnly = false, string? cw = null, IEnumerable<string>? visibleUserIds = null, string? inReplyTo = null);
    Task<CreateNoteResponse> CreateNote(CreateNoteRequest request, CancellationToken stoppingToken);
}

public class SharkeyHttpService(ILogger<SharkeyHttpService> logger, SharkeyConfig config, IHttpService http, IUserService userService) : ISharkeyHttpService
{
    public async Task ReportAbuse(string userId, string comment, CancellationToken stoppingToken)
        => await ReportAbuse(new ReportAbuseRequest
        {
            UserId = userId,
            Comment = comment
        }, stoppingToken);

    public async Task ReportAbuse(ReportAbuseRequest request, CancellationToken stoppingToken)
        => await PostAuthenticatedAsync("api/users/report-abuse", request, stoppingToken);

    public async Task<CreateNoteResponse> CreateNote(string text, string visibility, CancellationToken stoppingToken, bool localOnly = false, string? cw = null, IEnumerable<string>? visibleUserIds = null, string? inReplyTo = null)
        => await CreateNote(new CreateNoteRequest
        {
            Text = text,
            Visibility = visibility,
            LocalOnly = localOnly,
            ContentWarning = cw,
            VisibleUserIds = visibleUserIds,
            ReplyId = inReplyTo
        }, stoppingToken);

    public async Task<CreateNoteResponse> CreateNote(CreateNoteRequest request, CancellationToken stoppingToken)
        => await PostAuthenticatedAsync<CreateNoteRequest, CreateNoteResponse>("api/notes/create", request, stoppingToken);

    private async Task<TResponse> PostAuthenticatedAsync<TRequest, TResponse>(string action, TRequest request, CancellationToken stoppingToken)
        where TRequest : AuthenticatedRequestBase
    {
        await AuthenticateRequest(request, stoppingToken);
        return await PostAsync<TRequest, TResponse>(action, request, stoppingToken);
    }
    
    private async Task PostAuthenticatedAsync<TRequest>(string action, TRequest request, CancellationToken stoppingToken)
        where TRequest : AuthenticatedRequestBase
    {
        await AuthenticateRequest(request, stoppingToken);
        await PostAsync(action, request, stoppingToken);
    }

    private async Task AuthenticateRequest<TRequest>(TRequest request, CancellationToken stoppingToken)
        where TRequest : AuthenticatedRequestBase
    {
        // Populate the auto token, if not already set
        request.AuthToken
            ??= await userService.GetServiceAccountToken(stoppingToken) 
            ?? throw new ArgumentException("Authenticated request is missing auth token, and no service account token was found in the database", nameof(request));
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string action, TRequest request, CancellationToken stoppingToken)
    {
        var resp = await PostAsync(action, request, stoppingToken);

        return await resp.Content.ReadFromJsonAsync<TResponse>(stoppingToken)
               ?? throw new HttpRequestException($"Request failed, can't parse response as {typeof(TResponse).FullName}");
    }

    private async Task<HttpResponseMessage> PostAsync<TRequest>(string action, TRequest request, CancellationToken stoppingToken)
    {
        // Create the final request URL
        var url = $"{config.ApiEndpoint}/{action}";

        var resp = await http.PostAsync(url, request, stoppingToken);
        
        // We can't use EnsureSuccessStatusCode() because we lose the error details in the response
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(stoppingToken);
            logger.LogError("Request to {url} failed with HTTP/{code}: {phrase}. Got response: {body}", url, resp.StatusCode, resp.ReasonPhrase, body);
            throw new HttpRequestException($"Request to {url} failed with HTTP/{resp.StatusCode}: {resp.ReasonPhrase}");
        }

        return resp;
    }
}

public abstract class AuthenticatedRequestBase
{
    /// <summary>
    /// Authentication token for the user to make this request.
    /// If null, will be populated automatically by the <see cref="IUserService"/>.
    /// </summary>
    [JsonPropertyName("i")]
    public string? AuthToken { get; set; }
}

[PublicAPI]
public class ReportAbuseRequest : AuthenticatedRequestBase
{
    [JsonPropertyName("userId")] 
    public required string UserId { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = "";
}

[PublicAPI]
public class CreateNoteRequest : AuthenticatedRequestBase
{
    [JsonPropertyName("cw")]
    public string? ContentWarning { get; set; }
    
    [JsonPropertyName("localOnly")]
    public bool LocalOnly { get; set; }
    
    [JsonPropertyName("text")]
    public required string Text { get; set; }
    
    [JsonPropertyName("visibility")]
    public required string Visibility { get; set; }
    
    [JsonPropertyName("visibleUserIds")]
    public IEnumerable<string>? VisibleUserIds { get; set; }
    
    [JsonPropertyName("replyId")]
    public string? ReplyId { get; set; }
}

[PublicAPI]
public class CreateNoteResponse
{
    [JsonPropertyName("createdNote")]
    public required CreatedNote CreatedNote { get; set; }
}

[PublicAPI]
public class CreatedNote
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
}