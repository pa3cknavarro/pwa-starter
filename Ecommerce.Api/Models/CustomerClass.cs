using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Models;

[PrimaryKey(nameof(CustomerClassId))]
public class CustomerClass
{
    [Key]
    [Required(ErrorMessage = "Customer Class Id cannot be empty.")]
    [Display(Name = "Customer Class Id")]
    public Guid CustomerClassId { get; set; }

    [Required(ErrorMessage = "Class Name cannot be empty.")]
    [Display(Name = "Class Name")]
    public string ClassName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description cannot be empty.")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    // for Audit Trail
    [Display(Name = "Created By")]
    public string? CreatedBy { get; set; } = string.Empty;

    [Display(Name = "Created On")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
    public DateTime? CreatedOn { get; set; } = DateTime.Now;

}