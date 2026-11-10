using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

[PrimaryKey(nameof(ProductId))]
public class Product
{
    [Key]
    [Required(ErrorMessage = "Product Id cannot be empty.")]
    [Display(Name = "Product Id")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Product Name cannot be empty.")]
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string ProductDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a Category.")]
    [ForeignKey(nameof(CategoryId))]
    [Display(Name = "Category Id")]
    //[Range(1, int.MaxValue, ErrorMessage = "Please choose a Category.")]
    public Guid CategoryId { get; set; }

    [Display(Name = "Image Url")]
    public string ProductImageUrl { get; set; } = string.Empty;

    [Display(Name = "In Stock?")]
    public bool InStock { get; set; } = true;

    [Display(Name = "Comments")]
    public string Comments { get; set; } = string.Empty;


    // for Audit Trail
    [Display(Name = "Created By")]
    public string CreatedBy { get; set; } = string.Empty;

    [Display(Name = "Created On")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
    public DateTime CreatedOn { get; set; } = DateTime.Now;


    // relationships
    [ForeignKey(nameof(CategoryId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    public Category Category { get; set; } = new();

    [DeleteBehavior(DeleteBehavior.Cascade)] // cascade delete
    public List<UnitPrice> UnitPrices { get; set; } = [];

    [DeleteBehavior(DeleteBehavior.Cascade)] // cascade delete
    public List<AreaAvailable> AreasAvailable { get; set; } = [];
}