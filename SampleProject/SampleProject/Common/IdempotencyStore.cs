using System.Collections.Concurrent;
// Namespace for shared infrastructure
namespace SampleProject.Common;

/// <summary>
/// Simple in-memory store that records processed Idempotency-Key values.
/// In production this would be backed by Redis or a database with TTL.
/// </summary>
public class IdempotencyStore
{
    // Thread-safe dictionary: key = Idempotency-Key header value, value = cached response body + status
    private readonly ConcurrentDictionary<string, IdempotencyEntry> _store = new();

    // How long an entry remains valid before it can be reused (24 hours for demo)
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(24);

    /// <summary>
    /// Attempts to retrieve a previously stored response for the given key.
    /// Returns true if a valid entry exists.
    /// </summary>
    public bool TryGet(string key, out IdempotencyEntry? entry)
    {
        // Try to fetch the entry; if missing or expired, treat as not found
        if (_store.TryGetValue(key, out var existing) && existing.ExpiresAt > DateTime.UtcNow)
        {
            entry = existing;
            return true;
        }

        // Clean up expired entry if present
        _store.TryRemove(key, out _);
        entry = null;
        return false;
    }

    /// <summary>
    /// Stores a successful response against the given Idempotency-Key.
    /// </summary>
    public void Set(string key, int statusCode, string responseBody)
    {
        // Create a new entry with an expiration timestamp
        var entry = new IdempotencyEntry
        {
            StatusCode = statusCode,
            ResponseBody = responseBody,
            ExpiresAt = DateTime.UtcNow.Add(DefaultTtl)
        };

        // Add or overwrite the entry
        _store[key] = entry;
    }
}

/// <summary>
/// Value object holding the cached response for an Idempotency-Key.
/// </summary>
public class IdempotencyEntry
{
    // HTTP status code that was originally returned
    public int StatusCode { get; set; }

    // Serialized response body that was originally returned
    public string ResponseBody { get; set; } = string.Empty;

    // UTC timestamp after which this entry is considered expired
    public DateTime ExpiresAt { get; set; }
}
