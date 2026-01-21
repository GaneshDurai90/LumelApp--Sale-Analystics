using System;
using System.Collections.Generic;

namespace Lumel.Application.DTOs;

public class RevenueByCategoryResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CategoryRevenue> Categories { get; set; } = new();
}

public class CategoryRevenue
{
    public string Category { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int TotalQuantity { get; set; }
    public int ProductCount { get; set; }
}
