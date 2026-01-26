using System;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Commands;

/// <summary>
/// Command to save or update workflow session state
/// Used when navigating between wizard steps
/// </summary>
public record CommandRequest_Receiving_Shared_Save_WorkflowSession : IRequest<Model_Dao_Result<Model_Receiving_TableEntitys_WorkflowSession>>
{
    /// <summary>
    /// Session ID (Guid.Empty for new session)
    /// </summary>
    public Guid SessionId { get; init; }

    /// <summary>
    /// User ID who owns the session
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// Current workflow step (1, 2, or 3)
    /// </summary>
    public int CurrentStep { get; init; }

    /// <summary>
    /// Whether this is non-PO receiving
    /// </summary>
    public bool IsNonPO { get; init; }

    /// <summary>
    /// PO Number (NULL for non-PO)
    /// </summary>
    public string? PONumber { get; init; }

    /// <summary>
    /// Part ID/Number
    /// </summary>
    public string? PartId { get; init; }

    /// <summary>
    /// Number of loads
    /// </summary>
    public int LoadCount { get; init; }

    /// <summary>
    /// Session-scoped receiving location override
    /// </summary>
    public string? ReceivingLocationOverride { get; init; }

    /// <summary>
    /// Serialized load details (JSON)
    /// </summary>
    public string? LoadDetailsJson { get; init; }
}
