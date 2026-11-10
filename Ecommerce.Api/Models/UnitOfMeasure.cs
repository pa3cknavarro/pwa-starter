using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Models;

[PrimaryKey(nameof(Id))]
public class UnitOfMeasure
{
    [Key]
    [Required(ErrorMessage = "Id cannot be empty.")]
    [Display(Name = "Id")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "UOM cannot be empty.")]
    [Display(Name = "UOM")]
    public string Uom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description cannot be empty.")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    // for Audit Trail
    [Display(Name = "Created By")]
    public string CreatedBy { get; set; } = string.Empty;

    [Display(Name = "Created On")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
    public DateTime CreatedOn { get; set; } = DateTime.Now;
}