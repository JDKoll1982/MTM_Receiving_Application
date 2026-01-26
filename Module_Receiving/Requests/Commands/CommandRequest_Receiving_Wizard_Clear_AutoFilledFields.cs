using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Commands;

/// <summary>
/// Command to clear auto-filled fields from loads
/// Used in Wizard Mode Step 2 - "Clear Auto-Filled" feature
/// </summary>
public class CommandRequest_Receiving_Wizard_Clear_AutoFilledFields : IRequest<Model_Dao_Result<int>>
{
    /// <summary>
    /// Transaction ID containing the loads
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Specific load number to clear (NULL = clear ALL auto-filled loads)
    /// </summary>
    public int? TargetLoadNumber { get; set; }

    /// <summary>
    /// User performing the clear operation
    /// </summary>
    public string ModifiedBy { get; set; } = string.Empty;
}
