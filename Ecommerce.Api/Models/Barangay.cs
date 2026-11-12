namespace Ecommerce.Api.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[PrimaryKey(nameof(ProvinceId), nameof(CityId), nameof(BarangayId))] // composite primary key
public class Barangay
{
    [Key]
    [Required(ErrorMessage = "Province Id is required.")]
    [Display(Name = "Province Id")]
    public Guid ProvinceId { get; set; }

    [Key]
    [Required(ErrorMessage = "City Id is required.")]
    [Display(Name = "City Id")]
    public Guid CityId { get; set; }

    [Key]
    [Required(ErrorMessage = "Barangay Id is required.")]
    [Display(Name = "Barangay Id")]
    public Guid BarangayId { get; set; }

    [Required(ErrorMessage = "Barangay Name is required.")]
    [Display(Name = "Barangay Name")]
    public string BarangayName { get; set; } = string.Empty;

    // for Audit Trail
    [Display(Name = "Created By")]
    public string CreatedBy { get; set; } = string.Empty;
    [Display(Name = "Created On")]
    [DisplayFormat(DataFormatString = "{0:d}")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    // Relationships
    [ForeignKey(nameof(ProvinceId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public Province Province { get; set; } = new();

    [ForeignKey(nameof(CityId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public City City { get; set; } = new();
}