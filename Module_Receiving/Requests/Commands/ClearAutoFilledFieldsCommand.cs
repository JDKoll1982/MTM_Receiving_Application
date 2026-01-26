using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Commands;

/// <summary>
/// Command to clear auto-filled fields for a specific load or all loads.
/// </summary>
public class ClearAutoFilledFieldsCommand : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Transaction ID for the current workflow session.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Target load number to clear (null = all loads).
    /// </summary>
    public int? TargetLoadNumber { get; set; }

    /// <summary>
    /// User who initiated the clear operation.
    /// </summary>
    public string ModifiedBy { get; set; } = string.Empty;
}
