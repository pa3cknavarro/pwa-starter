// Namespace for domain models
namespace SampleProject.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Represents a city or municipality that belongs to a province.
/// Uses a composite primary key (ProvinceId + CityId).
/// </summary>
[PrimaryKey(nameof(ProvinceId), nameof(CityId))] // Composite primary key declaration
public class City
{
    // Part of the composite primary key – references the parent province
    [Key]
    [Required(ErrorMessage = "Province Id is required.")]
    [Display(Name = "Province Id")]
    public Guid ProvinceId { get; set; }

    // Part of the composite primary key – unique within the province
    [Key]
    [Required(ErrorMessage = "City Id is required.")]
    [Display(Name = "City Id")]
    public Guid CityId { get; set; }

    // Display name of the city (e.g. "Makati", "Cebu City")
    [Required(ErrorMessage = "City Name is required.")]
    [Display(Name = "City Name")]
    public string CityName { get; set; } = string.Empty;

    // Optional postal / ZIP code for the city
    [Display(Name = "Zip Code")]
    public string ZipCode { get; set; } = string.Empty;

    // Audit: creator identity
    [Display(Name = "Created By")]
    public string CreatedBy { get; set; } = string.Empty;

    // Audit: creation timestamp (UTC)
    [Display(Name = "Created On")]
    [DisplayFormat(DataFormatString = "{0:d}")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    // Navigation property back to the parent Province
    // ForeignKey attribute tells EF which column is the FK
    [ForeignKey(nameof(ProvinceId))]
    [DeleteBehavior(DeleteBehavior.NoAction)] // Prevent cascade delete
    public Province Province { get; set; } = new();

    // Navigation property: collection of barangays inside this city
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public List<Barangay> Barangays { get; set; } = [];
}
