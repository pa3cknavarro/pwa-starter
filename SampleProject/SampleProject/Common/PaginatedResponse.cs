// Namespace for shared types
namespace SampleProject.Common;

/// <summary>
/// Generic wrapper that returns a page of data together with pagination metadata and HATEOAS links.
/// </summary>
/// <typeparam name="T">Type of the items contained in the page</typeparam>
public class PaginatedResponse<T>
{
    // The actual data items for the current page
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    // Total number of items that match the filter (across all pages)
    public int TotalCount { get; set; }

    // Current page number (1-based)
    public int Page { get; set; }

    // Number of items requested per page
    public int PageSize { get; set; }

    // Total number of pages available
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    // Collection of HATEOAS links (self, next, prev, first, last, etc.)
    public List<HateoasLink> Links { get; set; } = new();
}
