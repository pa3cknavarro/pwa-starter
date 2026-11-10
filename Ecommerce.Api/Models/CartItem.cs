using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

[PrimaryKey(nameof(Id))]
public class CartItem
{
    [Key]
    [Required(ErrorMessage = "Id cannot be empty.")]
    [Display(Name = "Id")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "User Id cannot be empty.")]
    [Display(Name = "User Id")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Product Id cannot be empty.")]
    [Display(Name = "Product Id")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Unit Price cannot be empty.")]
    [Display(Name = "Unit Price")]
    [Column(TypeName = "decimal(18,2)")]
    [RegularExpression("^-?(\\d+|\\d{1,3}(?:,\\d{3})+)?(\\.\\d+)?$", ErrorMessage = "Invalid number format.")]
    public decimal UnitPrice { get; set; } = 0;

    [Required(ErrorMessage = "Uom Id cannot be empty.")]
    [Display(Name = "Uom Id")]
    public Guid UomId { get; set; }

    [Required(ErrorMessage = "Quantity cannot be empty.")]
    [Display(Name = "Quantity")]
    [Column(TypeName = "decimal(18,2)")]
    [RegularExpression("^-?(\\d+|\\d{1,3}(?:,\\d{3})+)?(\\.\\d+)?$", ErrorMessage = "Invalid number format.")]
    public decimal Quantity { get; set; } = 0;


    // for Audit Trail
    [Display(Name = "Created By")]
    public string? CreatedBy { get; set; } = string.Empty;

    [Display(Name = "Created On")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
    public DateTime? CreatedOn { get; set; } = DateTime.Now;


    // relationships
    [ForeignKey(nameof(UserId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    public User User { get; set; } = new();

    [ForeignKey(nameof(ProductId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    public Product Product { get; set; } = new();

    [ForeignKey(nameof(UomId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    public UnitOfMeasure Uom { get; set; } = new();
}