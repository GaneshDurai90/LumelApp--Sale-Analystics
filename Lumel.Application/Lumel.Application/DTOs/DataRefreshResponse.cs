using System;

namespace Lumel.Application.DTOs;

public class DataRefreshResponse
{
    public int Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DataRefreshStatusResponse
{
    public bool IsRunning { get; set; }
    public DataRefreshResponse? CurrentJob { get; set; }
    public DataRefreshResponse? LastCompletedJob { get; set; }
}
