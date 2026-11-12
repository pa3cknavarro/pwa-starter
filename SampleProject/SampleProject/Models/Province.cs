// Namespace that groups all domain entity models for the Ecommerce API
namespace SampleProject.Models;

// DataAnnotations provide validation attributes and display metadata
using System.ComponentModel.DataAnnotations;
// EF Core attributes for key configuration
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Represents a Philippine province (top-level administrative division).
/// Primary key is a single Guid (ProvinceId).
/// </summary>
[PrimaryKey(nameof(ProvinceId))] // Explicitly declare the primary key for EF Core
public class Province
{
    // Primary key – unique identifier for this province
    [Key] // Marks the property as the primary key
    [Required(ErrorMessage = "Province Id is required.")] // Validation: must be present
    [Display(Name = "Province Id")] // Friendly name used by UI scaffolding
    public Guid ProvinceId { get; set; }

    // Human-readable name of the province (e.g. "Cebu", "Metro Manila")
    [Required(ErrorMessage = "Province Name is required.")]
    [Display(Name = "Province Name")]
    public string ProvinceName { get; set; } = string.Empty;

    // Audit trail: username or system identity that created the record
    [Display(Name = "Created By")]
    public string CreatedBy { get; set; } = string.Empty;

    // Audit trail: UTC timestamp when the record was created
    [Display(Name = "Created On")]
    [DisplayFormat(DataFormatString = "{0:d}")] // Short date format for display
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    // Navigation property: collection of cities that belong to this province
    // Initialized to an empty list so it is never null
    public List<City> Cities { get; set; } = [];
}
