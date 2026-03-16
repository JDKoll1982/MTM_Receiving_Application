using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Bulk-upserts Volvo parts into the master table.
/// Each item is inserted if new, or updated if the part already exists.
/// </summary>
public record ImportPartsCommand : IRequest<Model_Dao_Result<ImportPartsResult>>
{
    /// <summary>Parts to import.  At least one item is required.</summary>
    public List<PartImportItem> Parts { get; init; } = new();
}

/// <summary>A single part to import or update.</summary>
public record PartImportItem
{
    public string PartNumber { get; init; } = string.Empty;
    public int QuantityPerSkid { get; init; }
}

/// <summary>
/// Result of import operation with success/failure counts and error details.
/// </summary>
public record ImportPartsResult
{
    /// <summary>
    /// Number of parts successfully imported.
    /// </summary>
    public int SuccessCount { get; init; }

    /// <summary>
    /// Number of parts that failed validation or import.
    /// </summary>
    public int FailureCount { get; init; }

    /// <summary>
    /// List of error messages for failed imports (includes row number and reason).
    /// </summary>
    public List<string> Errors { get; init; } = new();
}
