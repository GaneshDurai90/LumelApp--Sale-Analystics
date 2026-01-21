using System;

namespace Lumel.Application.DTOs;

public class TotalRevenueResponse
{
    public decimal TotalRevenue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalOrders { get; set; }
}
