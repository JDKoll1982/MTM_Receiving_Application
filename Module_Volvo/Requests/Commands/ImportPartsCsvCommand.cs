using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Commands;

/// <summary>
/// Command to bulk import parts from CSV file.
/// </summary>
public record ImportPartsCsvCommand : IRequest<Model_Dao_Result<ImportPartsCsvResult>>
{
    /// <summary>
    /// Full file path to CSV file containing part data (PartNumber, QuantityPerSkid).
    /// </summary>
    public string CsvFilePath { get; init; } = string.Empty;
}

/// <summary>
/// Result of CSV import operation with success/failure counts and error details.
/// </summary>
public record ImportPartsCsvResult
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
