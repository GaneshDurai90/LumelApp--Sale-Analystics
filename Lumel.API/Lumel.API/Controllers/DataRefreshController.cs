using System.Threading;
using System.Threading.Tasks;
using Lumel.Application.DTOs;
using Lumel.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lumel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataRefreshController : ControllerBase
{
    private readonly IDataRefreshService _dataRefreshService;

    public DataRefreshController(IDataRefreshService dataRefreshService)
    {
        _dataRefreshService = dataRefreshService;
    }

    /// <summary>
    /// Trigger a data refresh from CSV file
    /// </summary>
    /// <param name="filePath">Path to the CSV file to import</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Data refresh status</returns>
    [HttpPost("trigger")]
    public async Task<ActionResult<DataRefreshResponse>> TriggerRefresh(
        [FromQuery] string filePath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return BadRequest("File path is required");
        }

        try
        {
            var result = await _dataRefreshService.TriggerRefreshAsync(filePath, cancellationToken);
            return Ok(result);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Get current data refresh status
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<DataRefreshStatusResponse>> GetStatus(CancellationToken cancellationToken)
    {
        var status = await _dataRefreshService.GetStatusAsync(cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Get data refresh history
    /// </summary>
    /// <param name="count">Number of records to return (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<DataRefreshResponse>>> GetHistory(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        if (count < 1 || count > 100)
        {
            count = 10; // Reset to default if out of range
        }

        var history = await _dataRefreshService.GetRefreshHistoryAsync(count, cancellationToken);
        return Ok(history);
    }
}
