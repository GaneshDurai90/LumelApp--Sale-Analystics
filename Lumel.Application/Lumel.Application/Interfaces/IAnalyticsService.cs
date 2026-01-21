using System;
using System.Threading;
using System.Threading.Tasks;
using Lumel.Application.DTOs;

namespace Lumel.Application.Interfaces;

public interface IAnalyticsService
{
    Task<TotalRevenueResponse> GetTotalRevenueAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    Task<RevenueByProductResponse> GetRevenueByProductAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    Task<RevenueByCategoryResponse> GetRevenueByCategoryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    Task<RevenueByRegionResponse> GetRevenueByRegionAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    Task<TopProductsResponse> GetTopProductsAsync(TopProductsRequest request, CancellationToken cancellationToken = default);
}
