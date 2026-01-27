using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Commands;

/// <summary>
/// Command to soft-delete a receiving line
/// Used in Edit Mode and Manual Mode for removing incorrect entries
/// </summary>
public class CommandRequest_Receiving_Shared_Delete_ReceivingLine : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Line ID to delete
    /// </summary>
    public string LineId { get; set; } = string.Empty;

    /// <summary>
    /// User performing the deletion
    /// </summary>
    public string ModifiedBy { get; set; } = string.Empty;

    /// <summary>
    /// Reason for deletion (for audit log)
    /// </summary>
    public string DeletionReason { get; set; } = string.Empty;
}
