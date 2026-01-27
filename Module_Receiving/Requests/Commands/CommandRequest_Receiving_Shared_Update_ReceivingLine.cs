using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Commands;

/// <summary>
/// Command to update an existing receiving line/load
/// Used in Edit Mode and Manual Mode for corrections
/// </summary>
public class CommandRequest_Receiving_Shared_Update_ReceivingLine : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Line ID to update
    /// </summary>
    public string LineId { get; set; } = string.Empty;

    /// <summary>
    /// Purchase Order number (optional for non-PO)
    /// </summary>
    public string? PONumber { get; set; }

    /// <summary>
    /// Part number/ID
    /// </summary>
    public string? PartNumber { get; set; }

    /// <summary>
    /// Load number within transaction
    /// </summary>
    public int? LoadNumber { get; set; }

    /// <summary>
    /// Total quantity for this load
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// Total weight for this load
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Heat lot number or batch identifier
    /// </summary>
    public string? HeatLot { get; set; }

    /// <summary>
    /// Package type (Skid, Pallet, Box, etc.)
    /// </summary>
    public string? PackageType { get; set; }

    /// <summary>
    /// Number of packages in this load
    /// </summary>
    public int? PackagesPerLoad { get; set; }

    /// <summary>
    /// Weight per individual package
    /// </summary>
    public decimal? WeightPerPackage { get; set; }

    /// <summary>
    /// Receiving location (warehouse location code)
    /// </summary>
    public string? ReceivingLocation { get; set; }

    /// <summary>
    /// User making the modification
    /// </summary>
    public string ModifiedBy { get; set; } = string.Empty;
}
