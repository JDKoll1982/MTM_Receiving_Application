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
/// Handler for changing the copy source load number.
/// Updates session to track which load is used as the copy source.
/// </summary>
public class ChangeCopySourceCommandHandler : IRequestHandler<ChangeCopySourceCommand, Result>
{
    private readonly Dao_ReceivingWorkflowSession _sessionDao;
    private readonly ILogger _logger;

    public ChangeCopySourceCommandHandler(
        Dao_ReceivingWorkflowSession sessionDao,
        ILogger logger)
    {
        _sessionDao = sessionDao;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangeCopySourceCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("Changing copy source for session {SessionId} to load {NewSourceLoad}",
            request.SessionId, request.NewSourceLoadNumber);

        // Get current session
        var sessionResult = await _sessionDao.GetSessionAsync(request.SessionId);
        if (!sessionResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve session {SessionId}: {Error}", request.SessionId, sessionResult.ErrorMessage);
            return Result.Failure(sessionResult.ErrorMessage);
        }

        var session = sessionResult.Data;

        // Validate new source load number
        if (request.NewSourceLoadNumber < 1 || request.NewSourceLoadNumber > session.LoadCount)
        {
            _logger.Warning("Invalid source load number {SourceLoad} for session {SessionId} (max: {LoadCount})",
                request.NewSourceLoadNumber, request.SessionId, session.LoadCount);
            return Result.Failure($"Source load number must be between 1 and {session.LoadCount}");
        }

        // Update session copy source
        session.CopySourceLoadNumber = request.NewSourceLoadNumber;

        var updateResult = await _sessionDao.UpdateSessionAsync(session);
        if (!updateResult.IsSuccess)
        {
            _logger.Error("Failed to update session {SessionId}: {Error}", request.SessionId, updateResult.ErrorMessage);
            return Result.Failure(updateResult.ErrorMessage);
        }

        _logger.Information("Successfully changed copy source to load {SourceLoad} for session {SessionId}",
            request.NewSourceLoadNumber, request.SessionId);

        return Result.Success();
    }
}
