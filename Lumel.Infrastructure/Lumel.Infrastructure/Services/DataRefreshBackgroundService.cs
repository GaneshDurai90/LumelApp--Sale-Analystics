using Lumel.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lumel.Infrastructure.Services;

public class DataRefreshBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DataRefreshBackgroundService> _logger;
    private readonly int _intervalHours;
    private readonly string _filePath;

    public DataRefreshBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<DataRefreshBackgroundService> logger,
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intervalHours = config.GetValue("DataRefresh:IntervalHours", 24);
        _filePath = config.GetValue<string>("DataRefresh:DefaultFilePath") ?? "Data/sales_data.csv";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background refresh service started. Interval: {Hours}h", _intervalHours);

        // Initial delay
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await TryRefreshAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromHours(_intervalHours), stoppingToken);
        }
    }

    private async Task TryRefreshAsync(CancellationToken ct)
    {
        if (!File.Exists(_filePath))
        {
            _logger.LogWarning("Skipping scheduled refresh - file not found: {Path}", _filePath);
            return;
        }

        try
        {
            _logger.LogInformation("Running scheduled refresh");

            using var scope = _scopeFactory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IDataRefreshService>();
            var result = await svc.TriggerRefreshAsync(_filePath, ct);

            _logger.LogInformation("Scheduled refresh {Status}. Records: {Count}", result.Status, result.RecordsProcessed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled refresh failed");
        }
    }
}
