using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Commands;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Queries;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for navigating between workflow steps.
/// Validates current step before allowing advancement.
/// </summary>
public class Handler_Receiving_Wizard_Navigation_GoToStep : IRequestHandler<Command_Receiving_Wizard_Navigation_GoToStep, Result>
{
    private readonly Dao_Receiving_Repository_WorkflowSession _sessionDao;
    private readonly IRequestHandler<Query_Receiving_Wizard_Validate_CurrentStep, Result<ValidationStatus>> _validationHandler;
    private readonly ILogger _logger;

    public Handler_Receiving_Wizard_Navigation_GoToStep(
        Dao_Receiving_Repository_WorkflowSession sessionDao,
        IRequestHandler<Query_Receiving_Wizard_Validate_CurrentStep, Result<ValidationStatus>> validationHandler,
        ILogger logger)
    {
        _sessionDao = sessionDao;
        _validationHandler = validationHandler;
        _logger = logger;
    }

    public async Task<Result> Handle(Command_Receiving_Wizard_Navigation_GoToStep request, CancellationToken cancellationToken)
    {
        _logger.Information("Navigating session {SessionId} to step {TargetStep} (EditMode: {EditMode})",
            request.SessionId, request.TargetStep, request.IsEditMode);

        // Validate session exists
        var sessionResult = await _sessionDao.GetSessionAsync(request.SessionId);
        if (!sessionResult.IsSuccess)
        {
            _logger.Error("Session {SessionId} not found", request.SessionId);
            return Result.Failure($"Session not found: {request.SessionId}");
        }

        var session = sessionResult.Data;

        // Validate target step is valid (1, 2, or 3)
        if (request.TargetStep < 1 || request.TargetStep > 3)
        {
            return Result.Failure("Invalid target step. Must be 1, 2, or 3");
        }

        // If advancing (not edit mode), validate current step
        if (!request.IsEditMode && request.TargetStep > session.CurrentStep)
        {
            var validationResult = await ValidateCurrentStepAsync(session);
            if (!validationResult.IsSuccess)
            {
                _logger.Warning("Cannot advance from step {CurrentStep}: {Error}",
                    session.CurrentStep, validationResult.ErrorMessage);
                return validationResult;
            }
        }

        // Update session step and edit mode
        session.CurrentStep = request.TargetStep;
        session.IsEditMode = request.IsEditMode;

        var updateResult = await _sessionDao.UpdateSessionAsync(session);
        if (!updateResult.IsSuccess)
        {
            _logger.Error("Failed to update session step: {Error}", updateResult.ErrorMessage);
            return Result.Failure($"Failed to navigate: {updateResult.ErrorMessage}");
        }

        _logger.Information("Successfully navigated to step {TargetStep}", request.TargetStep);
        return Result.Success();
    }

    private async Task<Result> ValidateCurrentStepAsync(Model_Receiving_Entity_WorkflowSession session)
    {
        var validationQuery = new Query_Receiving_Wizard_Validate_CurrentStep(session.SessionId, session.CurrentStep);
        var validationResult = await _validationHandler.Handle(validationQuery, CancellationToken.None);

        if (!validationResult.IsSuccess)
            return Result.Failure(validationResult.ErrorMessage);

        if (validationResult.Value == null || !validationResult.Value.IsValid)
        {
            var errorCount = validationResult.Value?.ErrorCount ?? 0;
            var warningCount = validationResult.Value?.WarningCount ?? 0;
            var errorMessage = $"Step {session.CurrentStep} has validation errors: {errorCount} error(s), {warningCount} warning(s)";
            return Result.Failure(errorMessage);
        }

        return Result.Success();
    }
}
