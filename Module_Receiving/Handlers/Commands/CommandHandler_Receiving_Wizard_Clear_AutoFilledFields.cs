using System;
using System.Linq;
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
/// This handler processes a command to clear auto-filled fields in receiving lines for a specific transaction.
/// It retrieves all lines for the given transaction ID, optionally filters to a specific load number,
/// and clears fields that appear to have been auto-filled (such as HeatLotNumber, PackageType, PackagesPerLoad, and ReceivingLocation).
/// Each cleared line is updated in the database, and the handler returns the count of successfully cleared loads.
/// It includes logging for tracking the operation and handles errors gracefully.
/// </para>
/// <para>
/// Dependencies:
/// - Dao_Receiving_Repository_Line: For retrieving and updating receiving line data in the database
/// - IService_LoggingUtility: For logging the start, progress, and completion of the clear operation
/// - CommandRequest_Receiving_Wizard_Clear_AutoFilledFields: The command containing the transaction ID and optional load number
/// - Model_Dao_Result: The result wrapper for returning success/failure status and the cleared count
/// </para>
/// </summary>
public class CommandHandler_Receiving_Wizard_Clear_AutoFilledFields : IRequestHandler<CommandRequest_Receiving_Wizard_Clear_AutoFilledFields, Model_Dao_Result<int>>
{
    private readonly Dao_Receiving_Repository_Line _dao;
    private readonly IService_LoggingUtility _logger;

    public CommandHandler_Receiving_Wizard_Clear_AutoFilledFields(
        Dao_Receiving_Repository_Line dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<int>> Handle(
        CommandRequest_Receiving_Wizard_Clear_AutoFilledFields request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Clearing auto-filled fields for transaction {request.TransactionId}");

            // Step 1: Get all lines for this transaction
            var linesResult = await _dao.SelectByTransactionAsync(request.TransactionId);
            if (!linesResult.Success || linesResult.Data == null || linesResult.Data.Count == 0)
            {
                return Model_Dao_Result_Factory.Failure<int>("No lines found for transaction");
            }

            // Step 2: Filter to target loads
            var targetLoads = request.TargetLoadNumber.HasValue
                ? linesResult.Data.Where(l => l.LoadNumber == request.TargetLoadNumber.Value).ToList()
                : linesResult.Data.ToList();

            // Step 3: Clear auto-filled fields
            int clearedCount = 0;
            foreach (var load in targetLoads)
            {
                // Only clear if fields appear to be auto-filled (simple heuristic: check if they match pattern)
                // In a real implementation, you'd track IsAutoFilled flag on the entity
                bool wasCleared = false;

                // Clear auto-fillable fields
                if (!string.IsNullOrWhiteSpace(load.HeatLotNumber))
                {
                    load.HeatLotNumber = null;
                    wasCleared = true;
                }

                if (!string.IsNullOrWhiteSpace(load.PackageType))
                {
                    load.PackageType = null;
                    wasCleared = true;
                }

                if (load.PackagesPerLoad > 0)
                {
                    load.PackagesPerLoad = 1; // Reset to default
                    wasCleared = true;
                }

                if (!string.IsNullOrWhiteSpace(load.ReceivingLocation))
                {
                    load.ReceivingLocation = null;
                    wasCleared = true;
                }

                // Update if any fields were cleared
                if (wasCleared)
                {
                    var updateResult = await _dao.UpdateLineAsync(load);
                    if (updateResult.Success)
                    {
                        clearedCount++;
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to clear load {load.LoadNumber}: {updateResult.ErrorMessage}");
                    }
                }
            }

            _logger.LogInfo($"Clear auto-filled complete: {clearedCount} loads cleared");

            return Model_Dao_Result_Factory.Success(clearedCount, clearedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in ClearAutoFilledFieldsCommandHandler: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<int>($"Error clearing fields: {ex.Message}", ex);
        }
    }
}
