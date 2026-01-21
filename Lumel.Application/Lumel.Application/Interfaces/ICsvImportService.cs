using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lumel.Application.Interfaces;

/// <summary>
/// Interface for CSV data import operations
/// </summary>
public interface ICsvImportService
{
    /// <summary>
    /// Imports sales data from a CSV file stream
    /// </summary>
    /// <param name="stream">The stream containing CSV data</param>
    /// <param name="logId">The data refresh log ID for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of records processed</returns>
    Task<int> ImportSalesDataAsync(Stream stream, int logId, CancellationToken cancellationToken = default);
}
