using System.Collections;

namespace ModShark.Utils;

/// <summary>
/// Dictionary-like class that maps a key to one or more unique values.
/// </summary>
/// <remarks>
/// Ported from a TypeScript project, so the conventions are a little different.
/// </remarks>
public class MultiMap<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>
    where TKey: notnull
{
    private Dictionary<TKey, HashSet<TValue>> Map { get; } = new();

    /// <summary>
    /// Gets or sets the collection of values associated with a key.
    /// </summary>
    public IReadOnlySet<TValue>? this[TKey key]
    {
        get => Get(key);
        set
        {
            if (value == null)
                Delete(key);
            else
                Set(key, value);
        }
    }

    /// <summary>
    /// Gets or sets a key/value mapping.
    /// </summary>
    public bool this[TKey key, TValue val]
    {
        get => Has(key, val);
        set
        {
            if (value)
                Add(key, val);
            else
                Delete(key, val);
        }
    }
    
    /// <summary>
    /// Adds a key/value pair.
    /// Returns false if the pair already existed.
    /// </summary>
    public bool Add(KeyValuePair<TKey, TValue> pair)
        => Add(pair.Key, pair.Value);
    
    /// <inheritdoc />
    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> pair)
        => Add(pair);

    /// <summary>
    /// Adds a key/value pair.
    /// Returns false if the pair already existed.
    /// </summary>
    public bool Add(TKey key, TValue value)
    {
        if (!Map.TryGetValue(key, out var keySet))
        {
            keySet = [];
            Map[key] = keySet;
        }

        return keySet.Add(value);
    }

    /// <summary>
    /// Adds multiple values to a key.
    /// Returns the number of values that were not already mapped.
    /// </summary>
    public int AddRange(TKey key, IEnumerable<TValue> values)
    {
        if (!Map.TryGetValue(key, out var keySet))
        {
            keySet = [];
            Map[key] = keySet;
        }

        // Add all the values and count the ones that were new
        return values.Count(value => keySet.Add(value));
    }

    /// <summary>
    /// Replaces the mapped values for a given key.
    /// All existing values are removed, and all the provided values are added.
    /// </summary>
    public void Set(TKey key, IEnumerable<TValue> values)
        => Map[key] = values.ToHashSet();
    
    /// <summary>
    /// Checks if the key/value pair exists in the map.
    /// </summary>
    public bool Contains(KeyValuePair<TKey, TValue> item)
        => Has(item.Key, item.Value);
    
    /// <summary>
    /// Checks if the key exists in the map.
    /// </summary>
    public bool Has(TKey key)
        => Map.ContainsKey(key);

    /// <summary>
    /// Checks if the key/value pair exists in the map.
    /// </summary>
    public bool Has(TKey key, TValue value)
        => Map.TryGetValue(key, out var keySet) && keySet.Contains(value);

    /// <summary>
    /// Deletes a specified key/value pair from the map and returns true.
    /// Returns false if the pair was not found.
    /// </summary>
    public bool Remove(KeyValuePair<TKey, TValue> item)
        => Delete(item.Key, item.Value);
    
    /// <summary>
    /// Deletes all values associated with a given key and returns true.
    /// Returns false if no values were found.
    /// </summary>
    public bool Delete(TKey key)
        => Map.Remove(key);

    /// <summary>
    /// Deletes a specified key/value pair from the map and returns true.
    /// Returns false if the pair was not found.
    /// </summary>
    public bool Delete(TKey key, TValue value)
        => Map.TryGetValue(key, out var keySet) && keySet.Remove(value);

    /// <summary>
    /// Returns all unique values associated with the given key.
    /// Returns null if the key is not mapped.
    /// </summary>
    public IReadOnlySet<TValue>? Get(TKey key)
        => Map.GetValueOrDefault(key);

    /// <summary>
    /// Finds all unique values associated with the given key.
    /// Returns false if they key is not mapped.
    /// </summary>
    public bool TryGet(TKey key, out IReadOnlySet<TValue>? value)
    {
        if (Map.TryGetValue(key, out var keySet))
        {
            value = keySet;
            return true;
        }

        value = null;
        return false;
    }
    
    /// <inheritdoc cref="TotalSize"/>
    public int Count => TotalSize();

    /// <summary>
    /// Returns the number of keys in the map.
    /// </summary>
    public int Size()
        => Map.Count;
    
    /// <summary>
    /// Returns the number of values associated with a particular key.
    /// </summary>
    public int Size(TKey key)
        => Map.TryGetValue(key, out var set) ? set.Count : 0;

    /// <summary>
    /// Returns the number of key/value pairs in the map.
    /// </summary>
    public int TotalSize()
        => Map.Values.Aggregate(0, (sum, v) => sum + v.Count);

    /// <summary>
    /// Returns all unique keys in the map.
    /// </summary>
    public IEnumerable<TKey> Keys => Map.Keys;
    
    /// <summary>
    /// Returns all unique values in the map.
    /// </summary>
    public IEnumerable<TValue> Values => Map
        .Values
        .SelectMany(v => v)
        .Distinct();

    /// <summary>
    /// Returns all entries in the map as key/value pairs.
    /// </summary>
    public IEnumerable<KeyValuePair<TKey, TValue>> Entries => Map
        .SelectMany(pair => pair.Value
            .Select(v => new KeyValuePair<TKey, TValue>(pair.Key, v)));

    /// <summary>
    /// Returns all entries in the map as one-to-many pairings
    /// </summary>
    public IEnumerable<KeyValuePair<TKey, IReadOnlySet<TValue>>> Mappings => Map
        .Select(pair => new KeyValuePair<TKey, IReadOnlySet<TValue>>(pair.Key, pair.Value));
    
    /// <summary>
    /// Removes all entries from the map.
    /// </summary>
    public void Clear() => Map.Clear();

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Array index must be at least zero");

        if (Count > array.Length - arrayIndex)
            throw new ArgumentException("Array is not large enough to store all data", nameof(array));

        Entries.ForEach((value, i) => array[arrayIndex + i] = value);
    }
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Entries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
}