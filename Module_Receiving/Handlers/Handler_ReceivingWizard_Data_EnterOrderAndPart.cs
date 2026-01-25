using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Commands;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for updating Step 1 (Order & Part Selection).
/// Initializes LoadDetail records for all loads.
/// </summary>
public class Handler_ReceivingWizard_Data_EnterOrderAndPart : IRequestHandler<Command_ReceivingWizard_Data_EnterOrderAndPart, Result>
{
    private readonly Dao_ReceivingWorkflowSession _sessionDao;
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public Handler_ReceivingWizard_Data_EnterOrderAndPart(
        Dao_ReceivingWorkflowSession sessionDao,
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _sessionDao = sessionDao;
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result> Handle(Command_ReceivingWizard_Data_EnterOrderAndPart request, CancellationToken cancellationToken)
    {
        _logger.Information("Updating Step 1 for session {SessionId}: PO={PO}, PartId={PartId}, LoadCount={LoadCount}",
            request.SessionId, request.PONumber, request.PartId, request.LoadCount);

        // Get session
        var sessionResult = await _sessionDao.GetSessionAsync(request.SessionId);
        if (!sessionResult.IsSuccess)
        {
            _logger.Error("Session {SessionId} not found", request.SessionId);
            return Result.Failure($"Session not found: {request.SessionId}");
        }

        var session = sessionResult.Data;

        // Update Step 1 data
        session.PONumber = request.PONumber;
        session.PartId = request.PartId;
        session.LoadCount = request.LoadCount;
        session.CurrentStep = 1;

        // Save session
        var updateResult = await _sessionDao.UpdateSessionAsync(session);
        if (!updateResult.IsSuccess)
        {
            _logger.Error("Failed to update session: {Error}", updateResult.ErrorMessage);
            return Result.Failure($"Failed to update: {updateResult.ErrorMessage}");
        }

        // Create LoadDetail records for each load
        var existingLoadsResult = await _loadDao.GetLoadsBySessionAsync(request.SessionId);
        var existingLoads = existingLoadsResult.IsSuccess ? existingLoadsResult.Data : new List<LoadDetail>();

        // Initialize missing loads
        for (int i = 1; i <= request.LoadCount; i++)
        {
            if (existingLoads.Find(l => l.LoadNumber == i) == null)
            {
                var newLoad = new LoadDetail
                {
                    LoadNumber = i,
                    WeightOrQuantity = null,
                    HeatLot = null,
                    PackageType = null,
                    PackagesPerLoad = null,
                    IsWeightAutoFilled = false,
                    IsHeatLotAutoFilled = false,
                    IsPackageTypeAutoFilled = false,
                    IsPackagesPerLoadAutoFilled = false
                };

                var createResult = await _loadDao.UpsertLoadDetailAsync(request.SessionId, newLoad);
                if (!createResult.IsSuccess)
                {
                    _logger.Error("Failed to create LoadDetail {LoadNumber}: {Error}", i, createResult.ErrorMessage);
                    return Result.Failure($"Failed to create load {i}: {createResult.ErrorMessage}");
                }
            }
        }

        _logger.Information("Successfully updated Step 1: Created {LoadCount} loads", request.LoadCount);
        return Result.Success();
    }
}
