using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Lumel.Application.Interfaces;
using Lumel.Domain.Entities;
using Lumel.Infrastructure.Models;
using Lumel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lumel.Infrastructure.Services;

public class CsvImportService : ICsvImportService
{
    private readonly LumelDbContext _db;
    private readonly ILogger<CsvImportService> _logger;

    public CsvImportService(LumelDbContext db, ILogger<CsvImportService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<int> ImportSalesDataAsync(Stream stream, int logId, CancellationToken ct = default)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, csvConfig);

        // Load lookup data upfront
        var regions = await _db.Regions.ToDictionaryAsync(r => r.Name, r => r.RegionId, ct);
        var products = await _db.Products.ToDictionaryAsync(p => p.ProductId, p => p.ProductId, ct);
        var customers = await _db.Customers.ToDictionaryAsync(c => c.CustomerId, c => c.CustomerId, ct);
        var existingOrders = await _db.Orders.Select(o => o.OrderId).ToHashSetAsync(ct);

        int count = 0;
        var ordersToAdd = new List<Order>();
        var itemsToAdd = new List<OrderItem>();

        await foreach (var row in csv.GetRecordsAsync<SalesCsvRecord>(ct))
        {
            try
            {
                // Get or create region
                if (!regions.TryGetValue(row.Region, out int regionId))
                {
                    var newRegion = new Region { Name = row.Region };
                    _db.Regions.Add(newRegion);
                    await _db.SaveChangesAsync(ct);
                    regionId = newRegion.RegionId;
                    regions[row.Region] = regionId;
                }

                // Get or create product
                int prodId = ParseId(row.ProductId);
                if (!products.ContainsKey(prodId))
                {
                    var newProduct = new Product
                    {
                        ProductId = prodId,
                        ProductName = row.ProductName,
                        Category = row.Category
                    };
                    _db.Products.Add(newProduct);
                    await _db.SaveChangesAsync(ct);
                    products[prodId] = prodId;
                }

                // Get or create customer
                int custId = ParseId(row.CustomerId);
                if (!customers.ContainsKey(custId))
                {
                    var newCust = new Customer
                    {
                        Name = row.CustomerName,
                        Email = row.CustomerEmail,
                        Address = row.CustomerAddress
                    };
                    _db.Customers.Add(newCust);
                    await _db.SaveChangesAsync(ct);
                    custId = newCust.CustomerId;
                    customers[custId] = custId;
                }

                // Add order if not exists
                if (!existingOrders.Contains(row.OrderId))
                {
                    ordersToAdd.Add(new Order
                    {
                        OrderId = row.OrderId,
                        CustomerId = custId,
                        RegionId = regionId,
                        OrderDate = DateOnly.FromDateTime(row.DateOfSale)
                    });
                    existingOrders.Add(row.OrderId);
                }

                // Add order item
                itemsToAdd.Add(new OrderItem
                {
                    OrderId = row.OrderId,
                    ProductId = prodId,
                    Quantity = row.QuantitySold,
                    UnitPrice = row.UnitPrice,
                    Discount = row.Discount,
                    ShippingCost = row.ShippingCost
                });

                count++;

                // Batch save every 500 records
                if (count % 500 == 0)
                {
                    await SaveBatchAsync(ordersToAdd, itemsToAdd, ct);
                    ordersToAdd.Clear();
                    itemsToAdd.Clear();
                    
                    // Update progress
                    var log = await _db.DataRefreshLogs.FindAsync([logId], ct);
                    if (log != null)
                    {
                        log.RecordsProcessed = count;
                        await _db.SaveChangesAsync(ct);
                    }

                    _logger.LogDebug("Imported {Count} records so far", count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to import row {OrderId}", row.OrderId);
            }
        }

        // Save remaining
        if (ordersToAdd.Count > 0 || itemsToAdd.Count > 0)
        {
            await SaveBatchAsync(ordersToAdd, itemsToAdd, ct);
        }

        return count;
    }

    private async Task SaveBatchAsync(List<Order> orders, List<OrderItem> items, CancellationToken ct)
    {
        if (orders.Count > 0)
            _db.Orders.AddRange(orders);

        if (items.Count > 0)
            _db.OrderItems.AddRange(items);

        await _db.SaveChangesAsync(ct);
    }

    private static int ParseId(string value)
    {
        
        if (string.IsNullOrEmpty(value)) return 0;
        
        var numPart = value.TrimStart('P', 'C', 'p', 'c');
        return int.TryParse(numPart, out int id) ? id : 0;
    }
}
