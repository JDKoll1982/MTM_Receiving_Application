using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Commands;

/// <summary>
/// Command to complete and finalize a workflow
/// Archives transaction, marks session complete, generates CSV
/// </summary>
public class CommandRequest_Receiving_Shared_Complete_Workflow : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Transaction ID to complete
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Session ID to mark as completed
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Path where CSV file was exported
    /// </summary>
    public string CSVFilePath { get; set; } = string.Empty;

    /// <summary>
    /// User completing the workflow
    /// </summary>
    public string CompletedBy { get; set; } = string.Empty;
}
