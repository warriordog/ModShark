using System.Text.Json.Serialization;

namespace ModShark.Services;

public interface ISharkeyHttpService
{
    Task ReportAbuse(string userId, string comment, CancellationToken stoppingToken);
    Task ReportAbuse(ReportAbuseRequest request, CancellationToken stoppingToken);
}

public class SharkeyHttpService(ILogger<SharkeyHttpService> logger, SharkeyConfig config, IHttpService http, IServiceAccountService serviceAccountService) : ISharkeyHttpService
{
    public async Task ReportAbuse(string userId, string comment, CancellationToken stoppingToken)
        => await ReportAbuse(new ReportAbuseRequest { UserId = userId, Comment = comment }, stoppingToken);

    public async Task ReportAbuse(ReportAbuseRequest request, CancellationToken stoppingToken)
        => await PostAuthenticatedAsync("api/users/report-abuse", request, stoppingToken);

    private async Task PostAuthenticatedAsync<TRequest>(string action, TRequest request, CancellationToken stoppingToken)
        where TRequest : AuthenticatedRequestBase
    {
        // Populate the auto token, if not already set
        request.AuthToken
            ??= await serviceAccountService.GetServiceAccountToken(stoppingToken)
            ?? throw new ArgumentException("Authenticated request is missing auth token, and no service account token was found in the database", nameof(request));
        
        // Make the request
        await PostAsync(action, request, stoppingToken);
    }

    private async Task PostAsync<TRequest>(string action, TRequest request, CancellationToken stoppingToken)
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
    }
}

public abstract class AuthenticatedRequestBase
{
    /// <summary>
    /// Authentication token for the user to make this request.
    /// If null, will be populated automatically by the <see cref="IServiceAccountService"/>.
    /// </summary>
    [JsonPropertyName("i")]
    public string? AuthToken { get; set; }
}

public class ReportAbuseRequest : AuthenticatedRequestBase
{
    [JsonPropertyName("userId")] 
    public required string UserId { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = "";
}