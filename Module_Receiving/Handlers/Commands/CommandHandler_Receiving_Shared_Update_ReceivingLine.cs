using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;
using MTM_Receiving_Application.Module_Receiving.Requests.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Commands;

/// <summary>
/// <para>
/// This handler takes an update command, maps the provided data to a receiving line entity,
/// and performs a partial update in the database (only changing fields that were provided,
/// while leaving other fields unchanged). It logs success or failure and returns a result
/// indicating whether the update worked.
/// </para>
/// <para>
/// Dependencies:
/// - Dao_Receiving_Repository_Line: For database operations on receiving lines
/// - IService_LoggingUtility: For logging update attempts and results
/// - CommandRequest_Receiving_Shared_Update_ReceivingLine: The command this handler processes
/// - Model_Dao_Result: The result type returned by the handler
/// </para>
/// </summary>
public class CommandHandler_Receiving_Shared_Update_ReceivingLine : IRequestHandler<CommandRequest_Receiving_Shared_Update_ReceivingLine, Model_Dao_Result>
{
    private readonly Dao_Receiving_Repository_Line _dao;
    private readonly IService_LoggingUtility _logger;

    public CommandHandler_Receiving_Shared_Update_ReceivingLine(
        Dao_Receiving_Repository_Line dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result> Handle(
        CommandRequest_Receiving_Shared_Update_ReceivingLine request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Updating receiving line: {request.LineId}");

            // Map command to entity (partial update - only set provided values)
            var line = new Model_Receiving_TableEntitys_ReceivingLoad
            {
                LoadId = int.TryParse(request.LineId, out var loadId) ? loadId : 0,
                PartId = request.PartNumber ?? string.Empty,
                LoadNumber = request.LoadNumber ?? 0,
                Quantity = request.Quantity ?? 0,
                HeatLotNumber = request.HeatLot,
                PackageType = request.PackageType,
                PackagesPerLoad = request.PackagesPerLoad ?? 1,
                WeightPerPackage = request.WeightPerPackage,
                ReceivingLocation = request.ReceivingLocation
            };

            var result = await _dao.UpdateLineAsync(line);

            if (result.Success)
            {
                _logger.LogInfo($"Line {request.LineId} updated successfully");
            }
            else
            {
                _logger.LogError($"Failed to update line {request.LineId}: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in UpdateReceivingLineCommandHandler: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error updating line: {ex.Message}", ex);
        }
    }
}
