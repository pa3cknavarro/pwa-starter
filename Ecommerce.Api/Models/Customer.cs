using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Api.Models;

[PrimaryKey(nameof(CustomerId))]
public class Customer
{
    [Key]
    [Required(ErrorMessage = "Customer Id cannot be empty.")]
    [Display(Name = "Customer Id")]
    public Guid CustomerId { get; set; }

    [Required(ErrorMessage = "Company Name cannot be empty.")]
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; } = string.Empty;

    //[Required(ErrorMessage = "Customer Name cannot be empty.")]
    //[Display(Name = "Customer Name")]
    //public string CustomerName { get; set; } = string.Empty;

    //[Display(Name = "Nick Name")]
    //public string NickName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a Class.")]
    //[Range(1, int.MaxValue, ErrorMessage = "Please choose a Class.")]
    [Display(Name = "Class Id")]
    public Guid CustomerClassId { get; set; }

    [Display(Name = "Contact Person")]
    public string ContactPerson { get; set; } = string.Empty;

    [Display(Name = "Email Address")]
    [EmailAddress(ErrorMessage = "Please enter a valid Email Address.")]
    public string EmailAddress { get; set; } = string.Empty;

    [Display(Name = "Contact Number")]
    public string ContactNumber { get; set; } = string.Empty;



    [Required(ErrorMessage = "Please choose a Province.")]
    //[Range(1, int.MaxValue, ErrorMessage = "Please choose a Province.")]
    [Display(Name = "Province Id")]
    public Guid ProvinceId { get; set; } = Guid.Empty;

    [Required(ErrorMessage = "Please choose a City.")]
    //[Range(1, int.MaxValue, ErrorMessage = "Please choose a City.")]
    [Display(Name = "City Id")]
    public Guid CityId { get; set; } = Guid.Empty;

    [Required(ErrorMessage = "Please choose a Barangay.")]
    //[Range(1, int.MaxValue, ErrorMessage = "Please choose a Barangay.")]
    [Display(Name = "Barangay Id")]
    public Guid BarangayId { get; set; } = Guid.Empty;

    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;


    [Display(Name = "TIN No")]
    public string? TinNo { get; set; } = string.Empty;

    [Display(Name = "Notes")]
    public string? Notes { get; set; } = string.Empty;

    [Display(Name = "Active?")]
    public bool IsActive { get; set; } = true;


    // for Audit Trail
    [Display(Name = "Created By")]
    public string? CreatedBy { get; set; } = string.Empty;

    [Display(Name = "Created On")]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
    public DateTime? CreatedOn { get; set; } = DateTime.Now;


    // relationships
    [ForeignKey(nameof(CustomerClassId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    public CustomerClass CustomerClass { get; set; } = new();

    [ForeignKey(nameof(ProvinceId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    public Province Province { get; set; } = new();

    [ForeignKey(nameof(CityId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    public City City { get; set; } = new();

    [ForeignKey(nameof(BarangayId))] // bind Foreign Key column
    [DeleteBehavior(DeleteBehavior.NoAction)] // prevent cascade delete
    public Barangay Barangay { get; set; } = new();
}