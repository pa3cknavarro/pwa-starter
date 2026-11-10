namespace Ecommerce.Api.Models.Enumerations;

using System.ComponentModel;

public enum OrderStatus
{
    [Description("Pending")]
    Pending = 0,
    [Description("Processing")]
    Processing = 1,
    [Description("Shipped")]
    Shipped = 2,
    [Description("Delivered")]
    Delivered = 3,
    [Description("Cancelled")]
    Cancelled = 4
}