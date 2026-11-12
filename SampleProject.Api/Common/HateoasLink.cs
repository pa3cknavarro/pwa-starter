// Namespace for shared types
namespace Ecommerce.Api.Common;

/// <summary>
/// Represents a single hypermedia link following HATEOAS principles.
/// Clients can discover available actions without hard-coding URLs.
/// </summary>
public class HateoasLink
{
    // Relationship type (self, next, prev, create, update, delete, etc.)
    public string Rel { get; set; } = string.Empty;

    // Absolute or relative URL of the resource or action
    public string Href { get; set; } = string.Empty;

    // HTTP method that should be used when following the link (GET, POST, PUT, DELETE)
    public string Method { get; set; } = "GET";
}
