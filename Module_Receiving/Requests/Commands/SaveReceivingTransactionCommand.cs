using System;
using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Commands;

/// <summary>
/// Command to save a complete receiving transaction (all loads)
/// Used in Step 3: Review &amp; Save
/// </summary>
public record SaveReceivingTransactionCommand : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Session ID containing the workflow state
    /// </summary>
    public Guid SessionId { get; init; }

    /// <summary>
    /// User ID saving the transaction
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// User name (for audit trail)
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// Purchase Order number (NULL for non-PO)
    /// </summary>
    public string? PONumber { get; init; }

    /// <summary>
    /// Whether this is a non-PO transaction
    /// </summary>
    public bool IsNonPO { get; init; }

    /// <summary>
    /// List of load details to save
    /// </summary>
    public List<LoadDetail> Loads { get; init; } = new();

    /// <summary>
    /// Nested class for load details
    /// </summary>
    public record LoadDetail
    {
        public int LoadNumber { get; init; }
        public string PartId { get; init; } = string.Empty;
        public decimal Quantity { get; init; }
        public string UnitOfMeasure { get; init; } = "LBS";
        public string? HeatLotNumber { get; init; }
        public string? PackageType { get; init; }
        public int PackagesPerLoad { get; init; } = 1;
        public decimal? WeightPerPackage { get; init; }
        public string? ReceivingLocation { get; init; }
        public bool QualityHoldAcknowledged { get; init; }
    }
}
