namespace Ecommerce.Api.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[PrimaryKey(nameof(CityId))]
public class City
{
    [Key]
    [Required(ErrorMessage = "Province Id is required.")]
    [Display(Name = "Province Id")]
    public Guid ProvinceId { get; set; }

    [Key]
    [Required(ErrorMessage = "City Id is required.")]
    [Display(Name = "City Id")]
    public Guid CityId { get; set; }

    [Required(ErrorMessage = "City Name is required.")]
    [Display(Name = "City Name")]
    public string CityName { get; set; } = string.Empty;

    [Display(Name = "Zip Code")]
    public string ZipCode { get; set; } = string.Empty;

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

    [DeleteBehavior(DeleteBehavior.NoAction)]
    public List<Barangay> Barangays { get; set; } = [];
}