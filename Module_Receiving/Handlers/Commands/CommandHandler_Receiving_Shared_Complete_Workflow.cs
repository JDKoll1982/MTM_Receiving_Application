using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Requests.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Commands;

/// <summary>
/// <para>
/// This handler processes the CompleteWorkflowCommand to finalize a receiving workflow.
/// It completes the transaction by archiving it to the completed_transactions table,
/// updates the workflow session to mark it as finished, logs the entire process,
/// and returns a result indicating whether the completion succeeded or failed.
/// </para>
/// <para>
/// Dependencies:
/// - Dao_Receiving_Repository_Transaction: For completing and archiving the transaction
/// - Dao_Receiving_Repository_WorkflowSession: For updating the session status
/// - IService_LoggingUtility: For logging workflow completion attempts and results
/// - CommandRequest_Receiving_Shared_Complete_Workflow: The command this handler processes
/// - Model_Dao_Result: The result type returned by the handler
/// </para>
/// </summary>
public class CommandHandler_Receiving_Shared_Complete_Workflow : IRequestHandler<CommandRequest_Receiving_Shared_Complete_Workflow, Model_Dao_Result>
{
    private readonly Dao_Receiving_Repository_Transaction _transactionDao;
    private readonly Dao_Receiving_Repository_WorkflowSession _sessionDao;
    private readonly IService_LoggingUtility _logger;

    public CommandHandler_Receiving_Shared_Complete_Workflow(
        Dao_Receiving_Repository_Transaction transactionDao,
        Dao_Receiving_Repository_WorkflowSession sessionDao,
        IService_LoggingUtility logger)
    {
        _transactionDao = transactionDao;
        _sessionDao = sessionDao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result> Handle(
        CommandRequest_Receiving_Shared_Complete_Workflow request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Completing workflow: Transaction={request.TransactionId}, Session={request.SessionId}");

            // Step 1: Complete the transaction (archives to completed_transactions table)
            var transactionResult = await _transactionDao.CompleteAsync(
                request.TransactionId,
                request.CompletedBy,
                request.CSVFilePath);

            if (!transactionResult.Success)
            {
                _logger.LogError($"Failed to complete transaction: {transactionResult.ErrorMessage}");
                return transactionResult;
            }

            // Step 2: Mark session as completed
            var session = await _sessionDao.SelectByIdAsync(request.SessionId);
            if (session.Success && session.Data != null)
            {
                session.Data.CurrentStep = Enum_Receiving_State_WorkflowStep.ReviewAndSave; // Final step
                var sessionUpdateResult = await _sessionDao.UpdateSessionAsync(session.Data);

                if (!sessionUpdateResult.Success)
                {
                    _logger.LogWarning($"Transaction completed but session update failed: {sessionUpdateResult.ErrorMessage}");
                }
            }

            _logger.LogInfo($"Workflow completed successfully: Transaction={request.TransactionId}");

            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in CompleteWorkflowCommandHandler: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error completing workflow: {ex.Message}", ex);
        }
    }
}
