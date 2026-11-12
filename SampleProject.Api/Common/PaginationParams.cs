// Namespace for shared request/response helpers
namespace Ecommerce.Api.Common;

/// <summary>
/// Query-string parameters that control pagination, filtering and sorting.
/// Bound from the request query string in Carter endpoints.
/// </summary>
public class PaginationParams
{
    // 1-based page number requested by the client (default = 1)
    public int Page { get; set; } = 1;

    // Number of items to return per page (default = 10, clamped later)
    public int PageSize { get; set; } = 10;

    // Optional free-text search term applied to name fields
    public string? Search { get; set; }

    // Optional sort field name (e.g. "name", "createdOn")
    public string? SortBy { get; set; }

    // Sort direction: "asc" or "desc" (default = asc)
    public string? SortOrder { get; set; } = "asc";
}
