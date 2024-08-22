using System.Net.Http.Headers;
using System.Numerics;

namespace ModShark.Utils;

public static class HttpExtensions
{
    /// <summary>
    /// Gets a single header value by name.
    /// Returns null if the header does not exist.
    /// Throws an exception if multiple values exist.
    /// </summary>
    public static string? Get(this HttpResponseHeaders headers, string header)
    {
        if (!headers.TryGetValues(header, out var values))
            return null;

        return values.Single();
    }

    /// <summary>
    /// Gets a single header as a number.
    /// Returns null if the header does not exist.
    /// Throws an exception if multiple values exist.
    /// </summary>
    public static T? GetNumeric<T>(this HttpResponseHeaders headers, string header)
        where T : struct, INumber<T>
    {
        var value = headers.Get(header);
        if (value == null)
            return null;
        
        return T.Parse(value, null);
    }
}