using System.Net.Http.Json;

namespace ModShark.Services;

public interface IHttpService
{
    Task<HttpResponseMessage> PostAsync(string url, CancellationToken stoppingToken, IDictionary<string, string>? headers = null);
    Task<HttpResponseMessage> PostAsync<TBody>(string url, TBody body, CancellationToken stoppingToken, IDictionary<string, string>? headers = null);
}

public class HttpService(HttpClient client) : IHttpService
{
    public async Task<HttpResponseMessage> PostAsync(string url, CancellationToken stoppingToken, IDictionary<string, string>? headers = null)
        => await PostAsync<object?>(url, null, false, headers, stoppingToken);

    public async Task<HttpResponseMessage> PostAsync<TBody>(string url, TBody body, CancellationToken stoppingToken, IDictionary<string, string>? headers = null)
        => await PostAsync(url, body, true, headers, stoppingToken);
    
    private async Task<HttpResponseMessage> PostAsync<TBody>(string url, TBody body, bool hasBody, IDictionary<string, string>? headers, CancellationToken stoppingToken)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, url);

        // Attach headers
        if (headers != null)
        {
            foreach (var (name, value) in headers)
            {
                message.Headers.Add(name, value);
            }
        }
        
        // Set default user-agent
        if (!message.Headers.Contains("User-Agent"))
        {
            message.Headers.Add("User-Agent", "ModShark (https://github.com/warriordog/ModShark)");
        }
        
        // Attach body
        if (hasBody)
        {
            message.Content = JsonContent.Create(body);
        }

        // Send it!
        return await client.SendAsync(message, stoppingToken);
    }
}