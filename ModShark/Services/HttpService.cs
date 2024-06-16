using System.Net.Http.Json;

namespace ModShark.Services;

public interface IHttpService
{
    Task<HttpResponseMessage> PostAsync(string url, CancellationToken stoppingToken);
    Task<HttpResponseMessage> PostAsync(string url, IDictionary<string, string> headers, CancellationToken stoppingToken);
    Task<HttpResponseMessage> PostAsync<TBody>(string url, TBody body, CancellationToken stoppingToken);
    Task<HttpResponseMessage> PostAsync<TBody>(string url, TBody body, IDictionary<string, string> headers, CancellationToken stoppingToken);
}

public class HttpService(HttpClient client) : IHttpService
{
    public async Task<HttpResponseMessage> PostAsync(string url, CancellationToken stoppingToken)
        => await PostAsync<object?>(url, null, false, null, stoppingToken);

    public async Task<HttpResponseMessage> PostAsync(string url, IDictionary<string, string> headers, CancellationToken stoppingToken)
        => await PostAsync<object?>(url, null, false, headers, stoppingToken);

    public async Task<HttpResponseMessage> PostAsync<TBody>(string url, TBody body, CancellationToken stoppingToken)
        => await PostAsync(url, body, true, null, stoppingToken);

    public async Task<HttpResponseMessage> PostAsync<TBody>(string url, TBody body, IDictionary<string, string> headers, CancellationToken stoppingToken)
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
        
        // Attach body
        if (hasBody)
        {
            message.Content = JsonContent.Create(body);
        }

        // Send it!
        return await client.SendAsync(message, stoppingToken);
    }
}