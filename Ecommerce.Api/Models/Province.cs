namespace Ecommerce.Api.Models;

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[PrimaryKey(nameof(ProvinceId))]
public class Province
{
    [Key]
    [Required(ErrorMessage = "Province Id is required.")]
    [Display(Name = "Province Id")]
    public Guid ProvinceId { get; set; }

    [Required(ErrorMessage = "Province Name is required.")]
    [Display(Name = "Province Name")]
    public string ProvinceName { get; set; } = string.Empty;

    // for Audit Trail
    [Display(Name = "Created By")]
    public string CreatedBy { get; set; } = string.Empty;
    [Display(Name = "Created On")]
    [DisplayFormat(DataFormatString = "{0:d}")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    // Relationships
    public List<City> Cities { get; set; } = [];
}