namespace Ecommerce.Api.Models.Enumerations;

using System.ComponentModel;

public enum Roles
{
    [Description("User")]
    User = 0,
    [Description("Agent")]
    Agent = 1,
    [Description("Admin")]
    Admin = 2,
    [Description("Super Admin")]
    SuperAdmin = 3
}