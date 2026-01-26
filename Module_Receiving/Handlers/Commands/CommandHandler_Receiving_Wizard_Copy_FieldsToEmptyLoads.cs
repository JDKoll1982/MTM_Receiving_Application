using System;
using System.Linq;
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
/// This handler processes a command to copy selected field values from a source load to all other loads 
/// in the same transaction where those fields are empty. It retrieves all lines for the transaction, 
/// identifies the source load, and selectively copies fields (such as heat lot number, package type, 
/// packages per load, or quantity) based on the specified selection criteria. Only loads with empty 
/// target fields are updated, and the handler logs the operation and returns the count of successfully 
/// updated loads.
/// </para>
/// <para>
/// Dependencies:
/// - Dao_Receiving_Repository_Line: For retrieving and updating receiving line data in the database
/// - IService_LoggingUtility: For logging the bulk copy operation details and results
/// - CommandRequest_Receiving_Wizard_Copy_FieldsToEmptyLoads: The command containing the source load number, 
///   transaction ID, and fields to copy
/// - Model_Dao_Result&lt;int&gt;: The result type returned by the handler, containing the update count 
///   or error details
/// - Enum_Receiving_CopyType_FieldSelection: Defines the available field selection options for copying
/// - Model_Dao_Result_Factory: Provides factory methods for creating success or failure results
/// </para>
/// </summary>
public class CommandHandler_Receiving_Wizard_Copy_FieldsToEmptyLoads : IRequestHandler<CommandRequest_Receiving_Wizard_Copy_FieldsToEmptyLoads, Model_Dao_Result<int>>
{
    private readonly Dao_Receiving_Repository_Line _dao;
    private readonly IService_LoggingUtility _logger;

    public CommandHandler_Receiving_Wizard_Copy_FieldsToEmptyLoads(
        Dao_Receiving_Repository_Line dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<int>> Handle(
        CommandRequest_Receiving_Wizard_Copy_FieldsToEmptyLoads request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Bulk copying fields from load {request.SourceLoadNumber} in transaction {request.TransactionId}");

            // Step 1: Get all lines for this transaction
            var linesResult = await _dao.SelectByTransactionAsync(request.TransactionId);
            if (!linesResult.Success || linesResult.Data == null || linesResult.Data.Count == 0)
            {
                return Model_Dao_Result_Factory.Failure<int>("No lines found for transaction");
            }

            // Step 2: Find the source load
            var sourceLoad = linesResult.Data.FirstOrDefault(l => l.LoadNumber == request.SourceLoadNumber);
            if (sourceLoad == null)
            {
                return Model_Dao_Result_Factory.Failure<int>($"Source load {request.SourceLoadNumber} not found");
            }

            // Step 3: Copy fields to other loads (where fields are empty)
            int updatedCount = 0;
            foreach (var targetLoad in linesResult.Data.Where(l => l.LoadNumber != request.SourceLoadNumber))
            {
                bool wasModified = false;

                foreach (var field in request.FieldsToCopy)
                {
                    switch (field)
                    {
                        case Enum_Receiving_CopyType_FieldSelection.HeatLotOnly:
                            if (string.IsNullOrWhiteSpace(targetLoad.HeatLotNumber) && !string.IsNullOrWhiteSpace(sourceLoad.HeatLotNumber))
                            {
                                targetLoad.HeatLotNumber = sourceLoad.HeatLotNumber;
                                wasModified = true;
                            }
                            break;

                        case Enum_Receiving_CopyType_FieldSelection.PackageTypeOnly:
                            if (string.IsNullOrWhiteSpace(targetLoad.PackageType) && !string.IsNullOrWhiteSpace(sourceLoad.PackageType))
                            {
                                targetLoad.PackageType = sourceLoad.PackageType;
                                wasModified = true;
                            }
                            break;

                        case Enum_Receiving_CopyType_FieldSelection.PackagesPerLoadOnly:
                            if (targetLoad.PackagesPerLoad <= 0 && sourceLoad.PackagesPerLoad > 0)
                            {
                                targetLoad.PackagesPerLoad = sourceLoad.PackagesPerLoad;
                                wasModified = true;
                            }
                            break;

                        case Enum_Receiving_CopyType_FieldSelection.WeightQuantityOnly:
                            if (targetLoad.Quantity <= 0 && sourceLoad.Quantity > 0)
                            {
                                targetLoad.Quantity = sourceLoad.Quantity;
                                wasModified = true;
                            }
                            break;

                        case Enum_Receiving_CopyType_FieldSelection.AllFields:
                            // Copy all supported fields
                            if (string.IsNullOrWhiteSpace(targetLoad.HeatLotNumber)) targetLoad.HeatLotNumber = sourceLoad.HeatLotNumber;
                            if (string.IsNullOrWhiteSpace(targetLoad.PackageType)) targetLoad.PackageType = sourceLoad.PackageType;
                            if (targetLoad.PackagesPerLoad <= 0) targetLoad.PackagesPerLoad = sourceLoad.PackagesPerLoad;
                            if (targetLoad.Quantity <= 0) targetLoad.Quantity = sourceLoad.Quantity;
                            wasModified = true;
                            break;
                    }
                }

                // Update load if any fields were modified
                if (wasModified)
                {
                    var updateResult = await _dao.UpdateLineAsync(targetLoad);
                    if (updateResult.Success)
                    {
                        updatedCount++;
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to update load {targetLoad.LoadNumber}: {updateResult.ErrorMessage}");
                    }
                }
            }

            _logger.LogInfo($"Bulk copy complete: {updatedCount} loads updated");

            return Model_Dao_Result_Factory.Success(updatedCount, updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in BulkCopyFieldsCommandHandler: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<int>($"Error during bulk copy: {ex.Message}", ex);
        }
    }
}
