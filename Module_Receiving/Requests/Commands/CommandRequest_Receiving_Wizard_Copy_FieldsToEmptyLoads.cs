using System.Collections.Generic;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Commands;

/// <summary>
/// Command to bulk copy field values from source load to all other loads (empty fields only)
/// Used in Wizard Mode Step 2 - "Copy to All Loads" feature
/// </summary>
public class CommandRequest_Receiving_Wizard_Copy_FieldsToEmptyLoads : IRequest<Model_Dao_Result<int>>
{
    /// <summary>
    /// Transaction ID containing the loads
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Source load number to copy FROM (e.g., Load #1)
    /// </summary>
    public int SourceLoadNumber { get; set; }

    /// <summary>
    /// Which fields to copy (HeatLot, PackageType, ReceivingLocation, etc.)
    /// </summary>
    public List<Enum_Receiving_CopyType_FieldSelection> FieldsToCopy { get; set; } = new();

    /// <summary>
    /// User performing the bulk copy
    /// </summary>
    public string ModifiedBy { get; set; } = string.Empty;
}
