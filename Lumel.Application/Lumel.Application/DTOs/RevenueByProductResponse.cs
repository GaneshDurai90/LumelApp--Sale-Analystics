using System;
using System.Collections.Generic;

namespace Lumel.Application.DTOs;

public class RevenueByProductResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ProductRevenue> Products { get; set; } = new();
}

public class ProductRevenue
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int QuantitySold { get; set; }
}
