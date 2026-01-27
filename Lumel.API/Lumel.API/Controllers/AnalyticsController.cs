using System;
using System.Threading;
using System.Threading.Tasks;
using Lumel.Application.DTOs;
using Lumel.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lumel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Get total revenue for a date range
    /// </summary>
    [HttpGet("revenue/total")]
    public async Task<ActionResult<TotalRevenueResponse>> GetTotalRevenue(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date must be before or equal to end date");
        }

        var result = await _analyticsService.GetTotalRevenueAsync(startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    /// Get revenue breakdown by product
    /// </summary>
    [HttpGet("revenue/by-product")]
    public async Task<ActionResult<RevenueByProductResponse>> GetRevenueByProduct(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date must be before or equal to end date");
        }

        var result = await _analyticsService.GetRevenueByProductAsync(startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    /// Get revenue breakdown by category
    /// </summary>
    [HttpGet("revenue/by-category")]
    public async Task<ActionResult<RevenueByCategoryResponse>> GetRevenueByCategory(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date must be before or equal to end date");
        }

        var result = await _analyticsService.GetRevenueByCategoryAsync(startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    /// Get revenue breakdown by region
    /// </summary>
    [HttpGet("revenue/by-region")]
    public async Task<ActionResult<RevenueByRegionResponse>> GetRevenueByRegion(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date must be before or equal to end date");
        }

        var result = await _analyticsService.GetRevenueByRegionAsync(startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    /// Get top N products by quantity sold
    /// </summary>
    [HttpGet("top-products")]
    public async Task<ActionResult<TopProductsResponse>> GetTopProducts(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int topN = 10,
        [FromQuery] string? category = null,
        [FromQuery] string? region = null)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date must be before or equal to end date");
        }

        if (topN < 1 || topN > 100)
        {
            return BadRequest("TopN must be between 1 and 100");
        }

        var request = new TopProductsRequest
        {
            StartDate = startDate,
            EndDate = endDate,
            TopN = topN,
            Category = category,
            Region = region
        };

        var result = await _analyticsService.GetTopProductsAsync(request);
        return Ok(result);
    }
}
