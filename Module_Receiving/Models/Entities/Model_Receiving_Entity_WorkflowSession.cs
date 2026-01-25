using System;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Models.Entities;

/// <summary>
/// Represents the workflow session state for Guided Mode (Wizard)
/// Persists user progress through the 3-step wizard
/// </summary>
public class Model_Receiving_Entity_WorkflowSession
{
    /// <summary>
    /// Unique session identifier
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// User ID who owns this session
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Current workflow step (1, 2, or 3)
    /// </summary>
    public Enum_Receiving_State_WorkflowStep CurrentStep { get; set; }

    /// <summary>
    /// Workflow mode (Wizard, Manual, Edit)
    /// </summary>
    public Enum_Receiving_Mode_WorkflowMode WorkflowMode { get; set; }

    /// <summary>
    /// Whether this is a Non-PO receiving session
    /// </summary>
    public bool IsNonPO { get; set; }

    /// <summary>
    /// Purchase Order number (NULL for non-PO)
    /// </summary>
    public string? PONumber { get; set; }

    /// <summary>
    /// Selected part number/ID
    /// </summary>
    public string? PartId { get; set; }

    /// <summary>
    /// Number of loads to create
    /// </summary>
    public int LoadCount { get; set; }

    /// <summary>
    /// Session-scoped receiving location override
    /// </summary>
    public string? ReceivingLocationOverride { get; set; }

    /// <summary>
    /// Serialized load details (JSON)
    /// Stores the DataGrid state for Step 2
    /// </summary>
    public string? LoadDetailsJson { get; set; }

    /// <summary>
    /// Session creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Session last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Session expiration timestamp
    /// Sessions expire after 24 hours of inactivity
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
