using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Commands;
using MTM_Receiving_Application.Module_Receiving.Data;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for clearing auto-filled data.
/// Removes data that was auto-filled via copy operation (preserves manually entered data).
/// </summary>
public class ClearAutoFilledDataCommandHandler : IRequestHandler<ClearAutoFilledDataCommand, Result>
{
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public ClearAutoFilledDataCommandHandler(
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result> Handle(ClearAutoFilledDataCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("Clearing auto-filled data for {LoadCount} loads in session {SessionId}, fields: {Fields}",
            request.LoadNumbers.Count, request.SessionId, request.Fields);

        // Perform clear operation via DAO
        var result = await _loadDao.ClearAutoFilledAsync(
            request.SessionId,
            request.LoadNumbers,
            request.Fields
        );

        if (!result.IsSuccess)
        {
            _logger.Error("Failed to clear auto-filled data for session {SessionId}: {Error}",
                request.SessionId, result.ErrorMessage);
            return Result.Failure(result.ErrorMessage);
        }

        _logger.Information("Successfully cleared auto-filled data for {LoadCount} loads in session {SessionId}",
            request.LoadNumbers.Count, request.SessionId);

        return Result.Success();
    }
}
