using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Commands;
using MTM_Receiving_Application.Module_Receiving.Data;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for clearing auto-filled data from loads.
/// Only clears fields marked as auto-filled. Never affects manually entered data.
/// </summary>
public class Handler_Receiving_Wizard_Copy_ClearAutoFilledFields : IRequestHandler<Command_ReceivingWizard_Copy_ClearAutoFilledFields, Result>
{
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public Handler_Receiving_Wizard_Copy_ClearAutoFilledFields(
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result> Handle(Command_ReceivingWizard_Copy_ClearAutoFilledFields request, CancellationToken cancellationToken)
    {
        _logger.Information("Clearing auto-filled data: Session={SessionId}, LoadCount={LoadCount}, Fields={Fields}",
            request.SessionId, request.TargetLoadNumbers.Count, request.FieldsToClear);

        // If no target loads specified, clear all
        var targetLoads = request.TargetLoadNumbers.Count > 0 ? request.TargetLoadNumbers : null;

        var clearResult = await _loadDao.ClearAutoFilledAsync(
            request.SessionId,
            targetLoads ?? new System.Collections.Generic.List<int>(),
            request.FieldsToClear);

        if (!clearResult.IsSuccess)
        {
            _logger.Error("Failed to clear auto-filled data: {Error}", clearResult.ErrorMessage);
            return Result.Failure(clearResult.ErrorMessage);
        }

        _logger.Information("Successfully cleared auto-filled data for session {SessionId}",
            request.SessionId);

        return Result.Success();
    }
}
