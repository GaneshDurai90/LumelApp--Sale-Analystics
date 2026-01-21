using CsvHelper.Configuration.Attributes;

namespace Lumel.Infrastructure.Models;

/// <summary>
/// CSV record mapping for sales data import
/// </summary>
public class SalesCsvRecord
{
    [Name("Order ID")]
    public long OrderId { get; set; }

    [Name("Product ID")]
    public string ProductId { get; set; } = string.Empty;

    [Name("Customer ID")]
    public string CustomerId { get; set; } = string.Empty;

    [Name("Product Name")]
    public string ProductName { get; set; } = string.Empty;

    [Name("Category")]
    public string Category { get; set; } = string.Empty;

    [Name("Region")]
    public string Region { get; set; } = string.Empty;

    [Name("Date of Sale")]
    public DateTime DateOfSale { get; set; }

    [Name("Quantity Sold")]
    public int QuantitySold { get; set; }

    [Name("Unit Price")]
    public decimal UnitPrice { get; set; }

    [Name("Discount")]
    public decimal Discount { get; set; }

    [Name("Shipping Cost")]
    public decimal ShippingCost { get; set; }

    [Name("Payment Method")]
    public string PaymentMethod { get; set; } = string.Empty;

    [Name("Customer Name")]
    public string CustomerName { get; set; } = string.Empty;

    [Name("Customer Email")]
    public string CustomerEmail { get; set; } = string.Empty;

    [Name("Customer Address")]
    public string CustomerAddress { get; set; } = string.Empty;
}
