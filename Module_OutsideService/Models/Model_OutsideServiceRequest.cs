using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_OutsideService.Models;

/// <summary>
/// Represents an Outside Service request header and its draft lines.
/// </summary>
public class Model_OutsideServiceRequest
{
    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public int OutsideServiceRequestId { get; set; }

    /// <summary>
    /// Gets or sets the human-readable request number.
    /// </summary>
    public string RequestNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creator username.
    /// </summary>
    public string CreatedByUser { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creator display name.
    /// </summary>
    public string CreatedByDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the created UTC timestamp.
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets request-level notes.
    /// </summary>
    public string? RequestNotes { get; set; }

    /// <summary>
    /// Gets or sets the draft or persisted request lines.
    /// </summary>
    public List<Model_OutsideServiceRequestLine> Lines { get; set; } = new();
}
