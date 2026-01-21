using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lumel.Application.DTOs;

namespace Lumel.Application.Interfaces;

public interface IDataRefreshService
{
    Task<DataRefreshResponse> TriggerRefreshAsync(string filePath, CancellationToken cancellationToken = default);
    
    Task<DataRefreshStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default);
    
    Task<IEnumerable<DataRefreshResponse>> GetRefreshHistoryAsync(int count = 10, CancellationToken cancellationToken = default);
}
