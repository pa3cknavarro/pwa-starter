namespace Ecommerce.Api.Models;

using System.ComponentModel.DataAnnotations;
using Ecommerce.Api.Models.Enumerations;
using Microsoft.EntityFrameworkCore;

[PrimaryKey(nameof(Id))]
public class User
{
    [Key]
    [Display(Name = "User Id")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Company is required.")]
    [Display(Name = "Company Id")]
    public Guid CompanyId { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    [Display(Name = "Role")]
    public Roles Role { get; set; } = Roles.User;

    [Display(Name = "Is Active?")]
    public bool IsActive { get; set; } = true;

    // for Audit Trail
    [Display(Name = "Created By")]
    public string CreatedBy { get; set; } = string.Empty;
    [Display(Name = "Created On")]
    [DisplayFormat(DataFormatString = "{0:d}")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}