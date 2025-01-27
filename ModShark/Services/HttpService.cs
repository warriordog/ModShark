using System.Net.Http.Json;

namespace ModShark.Services;

public interface IHttpService
{
    Task<HttpResponseMessage> GetAsync(string url, CancellationToken stoppingToken, IDictionary<string, string?>? headers = null);
    Task<HttpResponseMessage> GetAsync<TBody>(string url, TBody body, CancellationToken stoppingToken, IDictionary<string, string?>? headers = null);
    
    Task<HttpResponseMessage> PostAsync(string url, CancellationToken stoppingToken, IDictionary<string, string?>? headers = null);
    Task<HttpResponseMessage> PostAsync<TBody>(string url, TBody body, CancellationToken stoppingToken, IDictionary<string, string?>? headers = null);
}

public class HttpService(HttpClient client) : IHttpService
{
    public async Task<HttpResponseMessage> GetAsync(string url, CancellationToken stoppingToken, IDictionary<string, string?>? headers = null)
        => await SendAsync<object?>(HttpMethod.Get, url, null, false, headers, stoppingToken);

    public async Task<HttpResponseMessage> GetAsync<TBody>(string url, TBody body, CancellationToken stoppingToken, IDictionary<string, string?>? headers = null)
        => await SendAsync(HttpMethod.Get, url, body, true, headers, stoppingToken);

    public async Task<HttpResponseMessage> PostAsync(string url, CancellationToken stoppingToken, IDictionary<string, string?>? headers = null)
        => await SendAsync<object?>(HttpMethod.Post, url, null, false, headers, stoppingToken);

    public async Task<HttpResponseMessage> PostAsync<TBody>(string url, TBody body, CancellationToken stoppingToken, IDictionary<string, string?>? headers = null)
        => await SendAsync(HttpMethod.Post, url, body, true, headers, stoppingToken);
    
    private async Task<HttpResponseMessage> SendAsync<TBody>(HttpMethod method, string url, TBody body, bool hasBody, IDictionary<string, string?>? headers, CancellationToken stoppingToken)
    {
        var message = new HttpRequestMessage(method, url);

        // Attach headers
        if (headers != null)
        {
            foreach (var (name, value) in headers)
            {
                if (value != null)
                    message.Headers.Add(name, value);
                else
                    message.Headers.Remove(name);
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