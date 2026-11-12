// Namespace for the Barangays vertical slice
namespace Ecommerce.Api.Features.Barangays;

// Import HATEOAS link type used on responses
using Ecommerce.Api.Common;

/// <summary>
/// Response DTO returned for a single Barangay resource.
/// Includes HATEOAS links so clients can discover related actions.
/// </summary>
public class BarangayResponse
{
    // Parent province identifier (part of the composite key)
    public Guid ProvinceId { get; set; }

    // Parent city identifier (part of the composite key)
    public Guid CityId { get; set; }

    // Barangay identifier (part of the composite key)
    public Guid BarangayId { get; set; }

    // Human-readable barangay name
    public string BarangayName { get; set; } = string.Empty;

    // Audit: who created the record
    public string CreatedBy { get; set; } = string.Empty;

    // Audit: when the record was created (UTC)
    public DateTime CreatedOn { get; set; }

    // HATEOAS links (self, city, province, update, delete, …)
    public List<HateoasLink> Links { get; set; } = new();
}

/// <summary>
/// Request body used when creating a new Barangay.
/// </summary>
public class CreateBarangayRequest
{
    // Required: the province this barangay belongs to
    public Guid ProvinceId { get; set; }

    // Required: the city this barangay belongs to
    public Guid CityId { get; set; }

    // Required: display name of the barangay
    public string BarangayName { get; set; } = string.Empty;

    // Optional creator identity (defaults to "system")
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Request body used when updating an existing Barangay.
/// </summary>
public class UpdateBarangayRequest
{
    // New display name
    public string BarangayName { get; set; } = string.Empty;
}
