using System;
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
/// Handler for starting a new receiving workflow session.
/// Creates a new session in the database with initial state.
/// </summary>
public class StartWorkflowCommandHandler : IRequestHandler<StartWorkflowCommand, Result<Guid>>
{
    private readonly Dao_ReceivingWorkflowSession _sessionDao;
    private readonly ILogger _logger;

    public StartWorkflowCommandHandler(
        Dao_ReceivingWorkflowSession sessionDao,
        ILogger logger)
    {
        _sessionDao = sessionDao;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("Starting new receiving workflow in {Mode} mode", request.Mode);

        var sessionId = Guid.NewGuid();
        var session = new ReceivingWorkflowSession
        {
            SessionId = sessionId,
            CreatedAt = DateTime.UtcNow,
            CurrentStep = 1,
            IsEditMode = false,
            IsSaved = false,
            HasUnsavedChanges = false
        };

        var result = await _sessionDao.CreateSessionAsync(session);

        if (!result.IsSuccess)
        {
            _logger.Error("Failed to create workflow session: {Error}", result.ErrorMessage);
            return Result<Guid>.Failure(result.ErrorMessage);
        }

        _logger.Information("Successfully created workflow session {SessionId}", sessionId);
        return Result<Guid>.Success(sessionId);
    }
}
