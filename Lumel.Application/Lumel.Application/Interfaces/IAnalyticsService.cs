using System;
using System.Threading;
using System.Threading.Tasks;
using Lumel.Application.DTOs;

namespace Lumel.Application.Interfaces;

public interface IAnalyticsService
{
    Task<TotalRevenueResponse> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
    
    Task<RevenueByProductResponse> GetRevenueByProductAsync(DateTime startDate, DateTime endDate);
    
    Task<RevenueByCategoryResponse> GetRevenueByCategoryAsync(DateTime startDate, DateTime endDate);
    
    Task<RevenueByRegionResponse> GetRevenueByRegionAsync(DateTime startDate, DateTime endDate);
    
    Task<TopProductsResponse> GetTopProductsAsync(TopProductsRequest request);
}
