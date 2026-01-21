using System;
using System.Collections.Generic;

namespace Lumel.Application.DTOs;

public class TopProductsResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int RequestedCount { get; set; }
    public string? FilterCategory { get; set; }
    public string? FilterRegion { get; set; }
    public List<TopProduct> Products { get; set; } = new();
}

public class TopProduct
{
    public int Rank { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}
