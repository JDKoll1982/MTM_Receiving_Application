using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Commands;

/// <summary>
/// Command to bulk copy fields from a source load to other empty cells.
/// </summary>
public class BulkCopyFieldsCommand : IRequest<Model_Dao_Result<int>>
{
    /// <summary>
    /// Transaction ID for the current workflow session.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Source load number to copy from.
    /// </summary>
    public int SourceLoadNumber { get; set; }

    /// <summary>
    /// List of fields to copy.
    /// </summary>
    public List<Enum_Receiving_CopyType_FieldSelection> FieldsToCopy { get; set; } = new();

    /// <summary>
    /// User who initiated the copy operation.
    /// </summary>
    public string ModifiedBy { get; set; } = string.Empty;
}
