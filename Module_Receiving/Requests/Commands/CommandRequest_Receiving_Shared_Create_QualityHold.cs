using System;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Commands;

/// <summary>
/// Command to create a quality hold record with initial user acknowledgment
/// Part of two-step acknowledgment workflow (Step 1 of 2)
/// User Requirements:
///   - Configurable part patterns (not hardcoded)
///   - Two-step acknowledgment tracking
///   - Full audit trail
/// </summary>
public record CommandRequest_Receiving_Shared_Create_QualityHold : IRequest<Model_Dao_Result<string>>
{
    /// <summary>
    /// Line ID this quality hold applies to
    /// </summary>
    public string LineId { get; init; } = string.Empty;

    /// <summary>
    /// Transaction ID (parent)
    /// </summary>
    public string TransactionId { get; init; } = string.Empty;

    /// <summary>
    /// Part number that triggered the quality hold
    /// </summary>
    public string PartNumber { get; init; } = string.Empty;

    /// <summary>
    /// Configurable part pattern that matched
    /// Example: "MMFSR*", "MMCSR*", "CUSTOM*"
    /// </summary>
    public string PartPattern { get; init; } = string.Empty;

    /// <summary>
    /// Type of restriction (configurable)
    /// Example: "Weight Sensitive", "Quality Control", "Special Handling"
    /// </summary>
    public string RestrictionType { get; init; } = string.Empty;

    /// <summary>
    /// Load number this quality hold applies to
    /// </summary>
    public int LoadNumber { get; init; }

    /// <summary>
    /// Total weight at time of hold
    /// </summary>
    public decimal? TotalWeight { get; init; }

    /// <summary>
    /// Package type at time of hold
    /// </summary>
    public string? PackageType { get; init; }

    /// <summary>
    /// Date/time of user acknowledgment (Step 1)
    /// </summary>
    public DateTime UserAcknowledgedDate { get; init; }

    /// <summary>
    /// Username who acknowledged (Step 1)
    /// </summary>
    public string UserAcknowledgedBy { get; init; } = string.Empty;

    /// <summary>
    /// Message displayed during first acknowledgment
    /// </summary>
    public string? UserAcknowledgmentMessage { get; init; }

    /// <summary>
    /// General notes about the quality hold
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Created by username (for audit trail)
    /// </summary>
    public string CreatedBy { get; init; } = string.Empty;
}
