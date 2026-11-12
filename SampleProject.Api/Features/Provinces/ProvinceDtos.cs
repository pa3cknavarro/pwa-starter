// Namespace for the Provinces vertical slice
namespace Ecommerce.Api.Features.Provinces;

// Import HATEOAS link type
using Ecommerce.Api.Common;

/// <summary>
/// Data transfer object returned for a single Province (response shape).
/// Uses consistent camelCase naming via JSON options.
/// </summary>
public class ProvinceResponse
{
    // Unique identifier of the province
    public Guid ProvinceId { get; set; }

    // Human-readable name of the province
    public string ProvinceName { get; set; } = string.Empty;

    // Audit: who created the record
    public string CreatedBy { get; set; } = string.Empty;

    // Audit: when the record was created (UTC)
    public DateTime CreatedOn { get; set; }

    // HATEOAS links that clients can follow for related actions
    public List<HateoasLink> Links { get; set; } = new();
}

/// <summary>
/// Request body used when creating a new Province.
/// </summary>
public class CreateProvinceRequest
{
    // Required name of the new province
    public string ProvinceName { get; set; } = string.Empty;

    // Optional creator name; defaults to "system" if omitted
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Request body used when updating an existing Province.
/// </summary>
public class UpdateProvinceRequest
{
    // New name for the province
    public string ProvinceName { get; set; } = string.Empty;
}
