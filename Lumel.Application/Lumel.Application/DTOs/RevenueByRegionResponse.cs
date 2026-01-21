using System;
using System.Collections.Generic;

namespace Lumel.Application.DTOs;

public class RevenueByRegionResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<RegionRevenue> Regions { get; set; } = new();
}

public class RegionRevenue
{
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int TotalOrders { get; set; }
}
