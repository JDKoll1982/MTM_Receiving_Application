using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Receiving.Requests.Commands;

/// <summary>
/// Command to complete the workflow session and mark transaction as finalized.
/// </summary>
public class CompleteWorkflowCommand : IRequest<Model_Dao_Result>
{
    /// <summary>
    /// Transaction ID that was saved.
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// Workflow session ID.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Local CSV file path.
    /// </summary>
    public string CSVFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Network CSV file path.
    /// </summary>
    public string NetworkCSVPath { get; set; } = string.Empty;

    /// <summary>
    /// User who completed the workflow.
    /// </summary>
    public string CompletedBy { get; set; } = string.Empty;
}
