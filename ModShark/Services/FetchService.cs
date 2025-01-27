using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ModShark.Services;

/// <summary>
/// Provides access to resources on local or remote systems.
/// </summary>
public interface IFetchService
{
    /// <summary>
    /// Opens a stream to a remote file.
    /// Optimistically uses file IO routines if the resource exists on the local system, falling back to HTTP fetch on failure.
    /// </summary>
    Task<Stream> FetchUrl(string url, CancellationToken stoppingToken);
}

public partial class FetchService(SharkeyConfig config, IHttpService httpService, IFileService fileService) : IFetchService
{
    public async Task<Stream> FetchUrl(string url, CancellationToken stoppingToken)
    {
        if (!TryOpenLocal(url, out var stream))
            stream = await OpenRemote(url, stoppingToken);

        return stream;
    }

    private bool TryOpenLocal(string url, [NotNullWhen(true)] out Stream? stream)
    {
        // Property is optional - only self-contained installations even support this.
        if (string.IsNullOrEmpty(config.FilesDirectoryPath))
        {
            stream = null;
            return false;
        }
        
        // Media might not be cached, in which case this will be a remote URL
        var match = LocalUrlRegex().Match(url);
        if (!match.Success || !match.Groups[1].Value.Equals(config.PublicHost, StringComparison.OrdinalIgnoreCase))
        {
            stream = null;
            return false;
        }

        try
        {
            var key = match.Groups[2].Value;
            var path = Path.Join(config.FilesDirectoryPath, key);
            
            // OpenRead implicitly does all existence/access checks
            stream = fileService.OpenRead(path);
            return true;
        }
        catch (IOException)
        {
            // OpenRead throws IOException (or a subtype) if the file is missing or inaccessible.
            stream = null;
            return false;
        }
    }
    
    private async Task<Stream> OpenRemote(string url, CancellationToken stoppingToken)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed))
            throw new ArgumentException($"Input is not a valid URL: \"{url}\"", nameof(url));
        if (parsed.Scheme != "http" && parsed.Scheme != "https")
            throw new InvalidOperationException($"Unsupported URL scheme: \"{url}\"");
        
        // Send without a User-Agent to prevent remote for serving fake responses to ModShark
        var response = await httpService.GetAsync(url, stoppingToken, new Dictionary<string, string?> { ["User-Agent"] = null });
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync(stoppingToken);
    }

    [GeneratedRegex(@"^https?://([^/]+)/files/([\w-]+(\.\w+)*)($|\?)", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex LocalUrlRegex();
}