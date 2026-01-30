using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Requests.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Commands;

/// <summary>
/// <para>
/// Handles the deletion of a receiving line by performing a soft delete operation.
/// This command handler processes the DeleteReceivingLineCommand, logs the operation,
/// and delegates the actual deletion to the data access layer while handling any errors gracefully.
/// </para>
/// <para>
/// Dependencies:
/// - Dao_Receiving_Repository_Line: Repository for accessing receiving line data in the database.
/// - IService_LoggingUtility: Service for logging informational and error messages.
/// - MediatR.IRequestHandler: Framework for handling CQRS commands.
/// - Model_Dao_Result: Result model for database operations.
/// - Model_Dao_Result_Factory: Factory for creating failure results.
/// </para>
/// </summary>
public class CommandHandler_Receiving_Shared_Delete_ReceivingLine : IRequestHandler<CommandRequest_Receiving_Shared_Delete_ReceivingLine, Model_Dao_Result>
{
    private readonly Dao_Receiving_Repository_Line _dao;
    private readonly IService_LoggingUtility _logger;

    public CommandHandler_Receiving_Shared_Delete_ReceivingLine(
        Dao_Receiving_Repository_Line dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result> Handle(
        CommandRequest_Receiving_Shared_Delete_ReceivingLine request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Deleting receiving line: {request.LineId}, Reason: {request.DeletionReason}");

            var result = await _dao.DeleteAsync(request.LineId, request.ModifiedBy);

            if (result.Success)
            {
                _logger.LogInfo($"Line {request.LineId} deleted successfully");
            }
            else
            {
                _logger.LogError($"Failed to delete line {request.LineId}: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in DeleteReceivingLineCommandHandler: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error deleting line: {ex.Message}", ex);
        }
    }
}
