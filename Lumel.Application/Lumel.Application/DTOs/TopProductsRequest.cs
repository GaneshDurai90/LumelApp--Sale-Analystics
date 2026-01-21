using System;

namespace Lumel.Application.DTOs;

public class TopProductsRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TopN { get; set; } = 10;
    public string? Category { get; set; }
    public string? Region { get; set; }
}
