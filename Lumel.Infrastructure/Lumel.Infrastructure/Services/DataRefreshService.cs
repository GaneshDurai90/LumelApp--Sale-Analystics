using Lumel.Application.DTOs;
using Lumel.Application.Interfaces;
using Lumel.Domain.Entities;
using Lumel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lumel.Infrastructure.Services;

public class DataRefreshService : IDataRefreshService
{
    private readonly LumelDbContext _db;
    private readonly ICsvImportService _importService;
    private readonly ILogger<DataRefreshService> _logger;
    
    private static bool _isRunning;
    private static readonly object _lock = new();

    public DataRefreshService(LumelDbContext db, ICsvImportService importService, ILogger<DataRefreshService> logger)
    {
        _db = db;
        _importService = importService;
        _logger = logger;
    }

    public async Task<DataRefreshResponse> TriggerRefreshAsync(string filePath, CancellationToken ct = default)
    {
        // lock check
        lock (_lock)
        {
            if (_isRunning)
                throw new InvalidOperationException("Refresh already running");
            _isRunning = true;
        }

        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            // Log start
            var log = new DataRefreshLog
            {
                StartedAt = DateTime.UtcNow,
                Status = "Running"
            };
            _db.DataRefreshLogs.Add(log);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Starting import from {Path}", filePath);

            try
            {
                using var fs = File.OpenRead(filePath);
                int count = await _importService.ImportSalesDataAsync(fs, log.Id, ct);

                log.Status = "Completed";
                log.RecordsProcessed = count;
                log.CompletedAt = DateTime.UtcNow;
                
                _logger.LogInformation("Import done. {Count} records", count);
            }
            catch (Exception ex)
            {
                log.Status = "Failed";
                log.ErrorMessage = ex.Message;
                log.CompletedAt = DateTime.UtcNow;
                _logger.LogError(ex, "Import failed");
            }

            await _db.SaveChangesAsync(ct);
            return ToDto(log);
        }
        finally
        {
            lock (_lock) { _isRunning = false; }
        }
    }

    public async Task<DataRefreshStatusResponse> GetStatusAsync(CancellationToken ct = default)
    {
        var running = await _db.DataRefreshLogs
            .Where(x => x.Status == "Running")
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(ct);

        var last = await _db.DataRefreshLogs
            .Where(x => x.Status != "Running")
            .OrderByDescending(x => x.CompletedAt)
            .FirstOrDefaultAsync(ct);

        return new DataRefreshStatusResponse
        {
            IsRunning = running != null,
            CurrentJob = running != null ? ToDto(running) : null,
            LastCompletedJob = last != null ? ToDto(last) : null
        };
    }

    public async Task<IEnumerable<DataRefreshResponse>> GetRefreshHistoryAsync(int count = 10, CancellationToken ct = default)
    {
        var logs = await _db.DataRefreshLogs
            .OrderByDescending(x => x.StartedAt)
            .Take(count)
            .ToListAsync(ct);

        return logs.Select(ToDto);
    }

    private static DataRefreshResponse ToDto(DataRefreshLog log) => new()
    {
        Id = log.Id,
        StartedAt = log.StartedAt,
        CompletedAt = log.CompletedAt,
        Status = log.Status,
        RecordsProcessed = log.RecordsProcessed,
        ErrorMessage = log.ErrorMessage
    };
}
