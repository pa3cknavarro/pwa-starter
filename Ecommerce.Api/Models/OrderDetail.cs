using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

[PrimaryKey(nameof(Id))]
public class OrderDetail
{
    [Key]
    [Required(ErrorMessage = "Id cannot be empty.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Order Id cannot be empty.")]
    [Display(Name = "Order Id")]
    public Guid OrderId { get; set; }

    [Required(ErrorMessage = "Please choose an Item.")]
    [Display(Name = "Item Id")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Please choose a UOM.")]
    [Display(Name = "UOM")]
    public Guid UomId { get; set; }

    [Required(ErrorMessage = "Quantity cannot be empty.")]
    [Display(Name = "Quantity")]
    [Column(TypeName = "decimal(18,2)")]
    [RegularExpression("^-?(\\d+|\\d{1,3}(?:,\\d{3})+)?(\\.\\d+)?$", ErrorMessage = "Invalid number format.")]
    public decimal Quantity { get; set; } = 0;

    [Required(ErrorMessage = "Unit Price cannot be empty.")]
    [Display(Name = "Unit Price")]
    [Column(TypeName = "decimal(18,2)")]
    [RegularExpression("^-?(\\d+|\\d{1,3}(?:,\\d{3})+)?(\\.\\d+)?$", ErrorMessage = "Invalid number format.")]
    public decimal UnitPrice { get; set; } = 0;

    [Required(ErrorMessage = "Extended Price cannot be empty.")]
    [Display(Name = "Extended Price")]
    [Column(TypeName = "decimal(18,2)")]
    [RegularExpression("^-?(\\d+|\\d{1,3}(?:,\\d{3})+)?(\\.\\d+)?$", ErrorMessage = "Invalid number format.")]
    public decimal ExtendedPrice { get; set; } = 0;

    [Display(Name = "Freight")]
    [Column(TypeName = "decimal(18,2)")]
    [RegularExpression("^-?(\\d+|\\d{1,3}(?:,\\d{3})+)?(\\.\\d+)?$", ErrorMessage = "Invalid number format.")]
    public decimal ItemFreight { get; set; } = 0;

    [Required(ErrorMessage = "Discount Amount cannot be empty.")]
    [Display(Name = "Discount Amount")]
    [Column(TypeName = "decimal(18,2)")]
    [RegularExpression("^-?(\\d+|\\d{1,3}(?:,\\d{3})+)?(\\.\\d+)?$", ErrorMessage = "Invalid number format.")]
    public decimal DiscountAmount { get; set; } = 0;

    [Display(Name = "Comment")]
    public string? Comment { get; set; } = string.Empty;

    // relationships
    [ForeignKey(nameof(ProductId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    public Product Product { get; set; } = new() ; // Required reference navigation to principal

    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    [ForeignKey(nameof(UomId))] // bind Foreign Key column
    public UnitOfMeasure UnitOfMeasure { get; set; } = new(); // Required reference navigation to principal

    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = new(); // Required reference navigation to principal
}