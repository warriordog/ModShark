namespace ModShark.Utils;

public static class CollectionExtensions
{
    /// <summary>
    /// Executes a callback for each element in a stream.
    /// The callback receives each value in parameter 1 and the index in parameter 2.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> stream, Action<T, int> callback)
    {
        var i = 0;
        foreach (var value in stream)
        {
            callback(value, i);
            i++;
        }
    }
}