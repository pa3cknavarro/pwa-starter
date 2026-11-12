// Namespace for domain models
namespace Ecommerce.Api.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Represents a barangay (smallest administrative unit) belonging to a city.
/// Uses a three-part composite primary key (ProvinceId + CityId + BarangayId).
/// </summary>
[PrimaryKey(nameof(ProvinceId), nameof(CityId), nameof(BarangayId))] // Composite PK
public class Barangay
{
    // First part of the composite key – province
    [Key]
    [Required(ErrorMessage = "Province Id is required.")]
    [Display(Name = "Province Id")]
    public Guid ProvinceId { get; set; }

    // Second part of the composite key – city
    [Key]
    [Required(ErrorMessage = "City Id is required.")]
    [Display(Name = "City Id")]
    public Guid CityId { get; set; }

    // Third part of the composite key – barangay itself
    [Key]
    [Required(ErrorMessage = "Barangay Id is required.")]
    [Display(Name = "Barangay Id")]
    public Guid BarangayId { get; set; }

    // Display name of the barangay
    [Required(ErrorMessage = "Barangay Name is required.")]
    [Display(Name = "Barangay Name")]
    public string BarangayName { get; set; } = string.Empty;

    // Audit fields
    [Display(Name = "Created By")]
    public string CreatedBy { get; set; } = string.Empty;

    [Display(Name = "Created On")]
    [DisplayFormat(DataFormatString = "{0:d}")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    // Navigation to parent Province
    [ForeignKey(nameof(ProvinceId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public Province Province { get; set; } = new();

    // Navigation to parent City (composite FK)
    [ForeignKey(nameof(CityId))] // Note: actual composite FK is configured in OnModelCreating
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public City City { get; set; } = new();
}
