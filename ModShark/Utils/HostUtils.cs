namespace ModShark.Utils;

public static class HostUtils
{
    /// <summary>
    /// Returns true if a hostname matches any of the provided set of hostnames.
    /// Subdomains are matched if any parent domain is included. 
    /// </summary>
    /// <remarks>
    /// This routine is case-insensitive at the expense of a constant-time overhead. 
    /// A customized HashSet must be constructed from the input enumerable.
    /// </remarks>
    /// <example>
    /// Example results:
    ///   "example.com" x ["example.com", "foo.com", "some.bar.org"] => true
    ///   "an.example.com x ["example.com", "foo.com", "some.bar.org"] => true
    ///   "a.different.bar.org" x ["example.com", "foo.com", "some.bar.org"] => false
    ///   "com" x ["example.com", "foo.com", "some.bar.org"] => false
    ///
    /// Example flow:
    ///   "an.example.com" x ["com"] => continue
    ///   "example.com" x ["com"] => continue
    ///   "com" x ["com"] => true
    /// </example>
    public static bool Matches(string host, IEnumerable<string> targetHosts)
    {
        // Case-insensitive hashset provide efficient comparisons
        var targetHostSet = new HashSet<string>(targetHosts, StringComparer.OrdinalIgnoreCase);
        
        // Test the exact host and each parent domain
        while (!targetHostSet.Contains(host))
        {
            // Find the next segment divider
            var split = host.IndexOf('.') + 1;
            
            // If we run out of segments, then the host doesn't match.
            // * split <= 0 means there's no more dividers, and this is the last segment.
            // * split >= length means the last divider is at the end, and there is no last segment.
            if (split <= 0 || split >= host.Length)
                return false;

            // Discard the left-most segment to obtain the parent domain
            host = host[split..];
        }

        return true;
    }
}