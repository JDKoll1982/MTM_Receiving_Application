using System;
using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Command to add a part to the current shipment (in-memory, not persisted until save/complete).
/// </summary>
public record AddPartToShipmentCommand : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Part number from Volvo master data.
    /// </summary>
    public string PartNumber { get; init; } = string.Empty;

    /// <summary>
    /// Number of skids received for this part.
    /// </summary>
    public int ReceivedSkidCount { get; init; }

    /// <summary>
    /// Expected number of skids (when HasDiscrepancy = true).
    /// </summary>
    public int? ExpectedSkidCount { get; init; }

    /// <summary>
    /// Indicates if there is a discrepancy between received and expected counts.
    /// </summary>
    public bool HasDiscrepancy { get; init; }

    /// <summary>
    /// Note explaining the discrepancy (required when HasDiscrepancy = true).
    /// </summary>
    public string DiscrepancyNote { get; init; } = string.Empty;

    /// <summary>
    /// Optional pending shipment ID (for editing pending shipments).
    /// </summary>
    public int? PendingShipmentId { get; init; }
}
