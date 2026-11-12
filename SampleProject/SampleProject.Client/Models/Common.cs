// Shared response models that mirror the SampleProject contracts
namespace SampleProject.Client.Models;

/// <summary>
/// HATEOAS link returned by the API on resources and collections.
/// </summary>
public class HateoasLink
{
    public string Rel { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
}

/// <summary>
/// Standard error envelope returned by the API on failures.
/// </summary>
public class ErrorEnvelope
{
    public string TraceId { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Errors { get; set; }
    public Dictionary<string, object>? Extensions { get; set; }
}

/// <summary>
/// Generic paginated response wrapper.
/// </summary>
public class PaginatedResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public List<HateoasLink> Links { get; set; } = new();
}

/// <summary>
/// Query parameters for list endpoints (pagination, filter, sort).
/// </summary>
public class ListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "asc";
}
