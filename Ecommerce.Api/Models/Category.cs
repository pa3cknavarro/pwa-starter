using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Api.Models;

[PrimaryKey(nameof(CategoryId))]
public class Category
{
    [Key]
    [Required(ErrorMessage = "Category Id cannot be empty.")]
    [Display(Name = "Category Id")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Category Name cannot be empty.")]
    [Display(Name = "Category Name")]
    public string? CategoryName { get; set; } = string.Empty;

    // for Audit Trail
    [Display(Name = "Created By")]
    public string? CreatedBy { get; set; } = string.Empty;

    [Display(Name = "Created On")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
    public DateTime? CreatedOn { get; set; } = DateTime.Now;

}