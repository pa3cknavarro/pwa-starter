namespace Ecommerce.Api.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[PrimaryKey(nameof(ProductId), nameof(UomId), nameof(UnitPriceId))] // composite primary key
public class UnitPrice
{
    [Key]
    [Required(ErrorMessage = "Unit Price Id is required.")]
    [Display(Name = "Unit Price Id")]
    public Guid UnitPriceId { get; set; }

    [Key]
    [Required(ErrorMessage = "Product Id is required.")]
    [Display(Name = "Product Id")]
    public Guid ProductId { get; set; }

    [Key]
    [Required(ErrorMessage = "Uom Id is required.")]
    [Display(Name = "Uom Id")]
    public Guid UomId { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Display(Name = "Price")]
    [Column(TypeName = "decimal(18,2)")]
    [RegularExpression("^-?(\\d+|\\d{1,3}(?:,\\d{3})+)?(\\.\\d+)?$", ErrorMessage = "Invalid number format.")]
    public decimal Price { get; set; } = 0;

    // Relationships
    [ForeignKey(nameof(ProductId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    public Product Product { get; set; } = new();

    [ForeignKey(nameof(UomId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    public UnitOfMeasure UnitOfMeasure { get; set; } = new();
}