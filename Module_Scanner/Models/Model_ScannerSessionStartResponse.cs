using System;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Response boundary for a scanner draft session create operation.
/// </summary>
public sealed class Model_ScannerSessionStartResponse
{
    public Guid SessionId { get; set; }

    public Enum_ScannerSessionStatus Status { get; set; } = Enum_ScannerSessionStatus.Draft;

    public DateTime CreatedUtc { get; set; }

    public string Message { get; set; } = string.Empty;
}
