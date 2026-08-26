using System;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Request boundary for querying scanner History (completed/stopped sessions).
/// </summary>
public sealed class Model_ScannerHistoryQueryRequest
{
    public string OwnerUserId { get; set; } = string.Empty;

    public DateTime? DateFromUtc { get; set; }

    public DateTime? DateToUtc { get; set; }

    public Enum_ScannerSessionStatus? StatusFilter { get; set; }

    public int MaxResults { get; set; } = 100;
}
