namespace Ecommerce.Api.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[PrimaryKey(nameof(AreaAvailableId))]
public class AreaAvailable
{
    [Key]
    [Required(ErrorMessage = "Area Available Id is required.")]
    [Display(Name = "Area Available Id")]
    public Guid AreaAvailableId { get; set; }

    [Key]
    [Required(ErrorMessage = "Province Id is required.")]
    [Display(Name = "Province Id")]
    public Guid ProvinceId { get; set; }

    [Key]
    [Required(ErrorMessage = "Is Available Id is required.")]
    [Display(Name = "Is Available?")]
    public bool IsAvailable { get; set; }

    // Relationships
    [ForeignKey(nameof(ProvinceId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    public Province Province { get; set; } = new();
}