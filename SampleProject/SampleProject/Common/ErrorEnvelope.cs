// Namespace for shared/common types used across the API
namespace SampleProject.Common;

/// <summary>
/// Standardized error response envelope returned by the API for all error cases.
/// Ensures consistent error shape for clients (Error Envelopes requirement).
/// </summary>
public class ErrorEnvelope
{
    // Unique correlation/trace identifier so clients and support can locate the log entry
    public string TraceId { get; set; } = string.Empty;

    // HTTP status code that was returned (mirrors the response status for convenience)
    public int Status { get; set; }

    // Short, human-readable title describing the error category (e.g. "Validation Error")
    public string Title { get; set; } = string.Empty;

    // Detailed message explaining what went wrong
    public string Detail { get; set; } = string.Empty;

    // Optional dictionary of field-level validation errors (key = property name, value = messages)
    public Dictionary<string, string[]>? Errors { get; set; }

    // Optional additional metadata that may help the client (e.g. retry-after seconds)
    public Dictionary<string, object>? Extensions { get; set; }
}
