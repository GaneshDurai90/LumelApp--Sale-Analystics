using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Lumel.Application.DTOs;
using Lumel.Application.Interfaces;
using Lumel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lumel.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly LumelDbContext _context;

    public AnalyticsService(LumelDbContext context)
    {
        _context = context;
    }

    public async Task<TotalRevenueResponse> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
    {
        var connection = _context.Database.GetDbConnection();
        
        var sql = @"
            SELECT 
                ISNULL(SUM(oi.Quantity * oi.UnitPrice * (1 - oi.Discount)), 0) AS TotalRevenue,
                COUNT(DISTINCT o.OrderId) AS TotalOrders
            FROM Orders o
            INNER JOIN OrderItems oi ON o.OrderId = oi.OrderId
            WHERE o.OrderDate >= @StartDate AND o.OrderDate <= @EndDate";

        var result = await connection.QueryFirstOrDefaultAsync<(decimal TotalRevenue, int TotalOrders)>(
            sql, 
            new { StartDate = startDate.Date, EndDate = endDate.Date });

        return new TotalRevenueResponse
        {
            TotalRevenue = result.TotalRevenue,
            TotalOrders = result.TotalOrders,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public async Task<RevenueByProductResponse> GetRevenueByProductAsync(DateTime startDate, DateTime endDate)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT 
                p.ProductId,
                p.ProductName,
                p.Category,
                SUM(oi.Quantity * oi.UnitPrice * (1 - oi.Discount)) AS Revenue,
                SUM(oi.Quantity) AS QuantitySold
            FROM Orders o
            INNER JOIN OrderItems oi ON o.OrderId = oi.OrderId
            INNER JOIN Products p ON oi.ProductId = p.ProductId
            WHERE o.OrderDate >= @StartDate AND o.OrderDate <= @EndDate
            GROUP BY p.ProductId, p.ProductName, p.Category
            ORDER BY Revenue DESC";

        var products = await connection.QueryAsync<ProductRevenue>(
            sql, 
            new { StartDate = startDate.Date, EndDate = endDate.Date });

        return new RevenueByProductResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            Products = products.AsList()
        };
    }

    public async Task<RevenueByCategoryResponse> GetRevenueByCategoryAsync(DateTime startDate, DateTime endDate)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT 
                p.Category,
                SUM(oi.Quantity * oi.UnitPrice * (1 - oi.Discount)) AS Revenue,
                SUM(oi.Quantity) AS TotalQuantity,
                COUNT(DISTINCT p.ProductId) AS ProductCount
            FROM Orders o
            INNER JOIN OrderItems oi ON o.OrderId = oi.OrderId
            INNER JOIN Products p ON oi.ProductId = p.ProductId
            WHERE o.OrderDate >= @StartDate AND o.OrderDate <= @EndDate
            GROUP BY p.Category
            ORDER BY Revenue DESC";

        var categories = await connection.QueryAsync<CategoryRevenue>(
            sql, 
            new { StartDate = startDate.Date, EndDate = endDate.Date });

        return new RevenueByCategoryResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            Categories = categories.AsList()
        };
    }

    public async Task<RevenueByRegionResponse> GetRevenueByRegionAsync(DateTime startDate, DateTime endDate)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT 
                r.RegionId,
                r.Name AS RegionName,
                SUM(oi.Quantity * oi.UnitPrice * (1 - oi.Discount)) AS Revenue,
                COUNT(DISTINCT o.OrderId) AS TotalOrders
            FROM Orders o
            INNER JOIN OrderItems oi ON o.OrderId = oi.OrderId
            INNER JOIN Regions r ON o.RegionId = r.RegionId
            WHERE o.OrderDate >= @StartDate AND o.OrderDate <= @EndDate
            GROUP BY r.RegionId, r.Name
            ORDER BY Revenue DESC";

        var regions = await connection.QueryAsync<RegionRevenue>(
            sql, 
            new { StartDate = startDate.Date, EndDate = endDate.Date });

        return new RevenueByRegionResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            Regions = regions.AsList()
        };
    }

    public async Task<TopProductsResponse> GetTopProductsAsync(TopProductsRequest request)
    {
        var connection = _context.Database.GetDbConnection();

        
        var sql = @"
            SELECT TOP (@TopN)
                p.ProductId,
                p.ProductName,
                p.Category,
                SUM(oi.Quantity) AS TotalQuantitySold,
                SUM(oi.Quantity * oi.UnitPrice * (1 - oi.Discount)) AS TotalRevenue
            FROM Orders o
            INNER JOIN OrderItems oi ON o.OrderId = oi.OrderId
            INNER JOIN Products p ON oi.ProductId = p.ProductId
            INNER JOIN Regions r ON o.RegionId = r.RegionId
            WHERE o.OrderDate >= @StartDate AND o.OrderDate <= @EndDate";

       
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            sql += " AND p.Category = @Category";
        }
        if (!string.IsNullOrWhiteSpace(request.Region))
        {
            sql += " AND r.Name = @Region";
        }

        sql += @"
            GROUP BY p.ProductId, p.ProductName, p.Category
            ORDER BY TotalQuantitySold DESC";

        var queryResult = await connection.QueryAsync<TopProductQueryResult>(sql, new
        {
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            TopN = request.TopN,
            Category = request.Category,
            Region = request.Region
        });

        // Map results with rank
        var products = new List<TopProduct>();
        int rank = 1;
        foreach (var item in queryResult)
        {
            products.Add(new TopProduct
            {
                Rank = rank++,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Category = item.Category,
                TotalQuantitySold = item.TotalQuantitySold,
                TotalRevenue = item.TotalRevenue
            });
        }

        return new TopProductsResponse
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RequestedCount = request.TopN,
            FilterCategory = request.Category,
            FilterRegion = request.Region,
            Products = products
        };
    }

  
    private class TopProductQueryResult
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
