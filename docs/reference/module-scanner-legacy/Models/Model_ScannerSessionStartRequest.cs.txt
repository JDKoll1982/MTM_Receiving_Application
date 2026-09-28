using System;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Request boundary for creating a new scanner draft session.
/// </summary>
public sealed class Model_ScannerSessionStartRequest
{
    public string OwnerUserId { get; set; } = string.Empty;

    public string OwnerDisplayName { get; set; } = string.Empty;

    public Guid ActiveProfileId { get; set; }

    public string AppWindowTitleSnapshot { get; set; } = string.Empty;

    public string AppWindowClassSnapshot { get; set; } = string.Empty;
}
