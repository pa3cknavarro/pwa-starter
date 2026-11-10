using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce.Api.Models.Enumerations;

namespace Ecommerce.Api.Models;

[PrimaryKey(nameof(OrderId))]
public class Order
{
    [Key]
    [Required(ErrorMessage = "Order Id cannot be empty.")]
    [Display(Name = "Order Id")]
    public Guid OrderId { get; set; }

    [Required(ErrorMessage = "Please choose a Date.")]
    [Display(Name = "Order Date")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
    public DateTime OrderDate { get; set; } = DateTime.Now;

    [Required]
    [Display(Name = "Customer Id")]
    //[Range(1, int.MaxValue, ErrorMessage = "Please choose a Customer.")]
    public Guid CustomerId { get; set; }

    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

    [Display(Name = "Subtotal")]
    [Column(TypeName = "decimal(18,2)")]
    [RegularExpression("^-?(\\d+|\\d{1,3}(?:,\\d{3})+)?(\\.\\d+)?$", ErrorMessage = "Invalid number format.")]
    public decimal SubTotal { get; set; } = 0;

    [Display(Name = "Freight")]
    [Column(TypeName = "decimal(18,2)")]
    [RegularExpression("^-?(\\d+|\\d{1,3}(?:,\\d{3})+)?(\\.\\d+)?$", ErrorMessage = "Invalid number format.")]
    public decimal OrderFreight { get; set; } = 0;

    [Display(Name = "Discount Amount")]
    [Column(TypeName = "decimal(18,2)")]
    [RegularExpression("^-?(\\d+|\\d{1,3}(?:,\\d{3})+)?(\\.\\d+)?$", ErrorMessage = "Invalid number format.")]
    public decimal DiscountAmount { get; set; } = 0;

    [Display(Name = "Grand Total")]
    [Column(TypeName = "decimal(18,2)")]
    [RegularExpression("^-?(\\d+|\\d{1,3}(?:,\\d{3})+)?(\\.\\d+)?$", ErrorMessage = "Invalid number format.")]
    public decimal GrandTotal { get; set; } = 0;

    [Display(Name = "Comment")]
    public string Comment { get; set; } = string.Empty;

    [Display(Name = "Created By")]
    public string CreatedBy { get; set; } = string.Empty;

    [Display(Name = "Created On")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
    public DateTime CreatedOn { get; set; } = DateTime.Now;

    [Display(Name = "Modified By")]
    public string ModifiedBy { get; set; } = string.Empty;

    [Display(Name = "Modified On")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
    public DateTime ModifiedOn { get; set; } = DateTime.Now;


    // relationships
    [ForeignKey(nameof(CustomerId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    public Customer Customer { get; set; } = new();

    [DeleteBehavior(DeleteBehavior.Cascade)] // cascade delete
    public List<OrderDetail> OrderDetail { get; set; } = [];
}