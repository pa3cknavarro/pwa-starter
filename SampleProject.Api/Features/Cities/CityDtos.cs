// Namespace for the Cities vertical slice
namespace Ecommerce.Api.Features.Cities;

// Import HATEOAS link type used on responses
using Ecommerce.Api.Common;

/// <summary>
/// Response DTO returned for a single City resource.
/// Includes HATEOAS links so clients can discover related actions.
/// </summary>
public class CityResponse
{
    // Parent province identifier (part of the composite key)
    public Guid ProvinceId { get; set; }

    // City identifier (part of the composite key)
    public Guid CityId { get; set; }

    // Human-readable city name
    public string CityName { get; set; } = string.Empty;

    // Optional postal / ZIP code
    public string ZipCode { get; set; } = string.Empty;

    // Audit: who created the record
    public string CreatedBy { get; set; } = string.Empty;

    // Audit: when the record was created (UTC)
    public DateTime CreatedOn { get; set; }

    // HATEOAS links (self, province, update, delete, barangays, …)
    public List<HateoasLink> Links { get; set; } = new();
}

/// <summary>
/// Request body used when creating a new City.
/// </summary>
public class CreateCityRequest
{
    // Required: the province this city belongs to
    public Guid ProvinceId { get; set; }

    // Required: display name of the city
    public string CityName { get; set; } = string.Empty;

    // Optional ZIP / postal code
    public string? ZipCode { get; set; }

    // Optional creator identity (defaults to "system")
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Request body used when updating an existing City.
/// </summary>
public class UpdateCityRequest
{
    // New display name
    public string CityName { get; set; } = string.Empty;

    // New ZIP code (can be empty)
    public string? ZipCode { get; set; }
}
