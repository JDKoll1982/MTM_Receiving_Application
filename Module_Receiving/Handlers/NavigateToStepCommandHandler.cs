using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Commands;
using MTM_Receiving_Application.Module_Receiving.Data;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for navigating between workflow steps with validation.
/// Ensures current step is valid before allowing navigation.
/// </summary>
public class NavigateToStepCommandHandler : IRequestHandler<NavigateToStepCommand, Result>
{
    private readonly Dao_ReceivingWorkflowSession _sessionDao;
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public NavigateToStepCommandHandler(
        Dao_ReceivingWorkflowSession sessionDao,
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _sessionDao = sessionDao;
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result> Handle(NavigateToStepCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("Navigating session {SessionId} to step {TargetStep}", request.SessionId, request.TargetStep);

        // Get current session
        var sessionResult = await _sessionDao.GetSessionAsync(request.SessionId);
        if (!sessionResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve session {SessionId}: {Error}", request.SessionId, sessionResult.ErrorMessage);
            return Result.Failure(sessionResult.ErrorMessage);
        }

        var session = sessionResult.Data;

        // Validate target step
        if (request.TargetStep < 1 || request.TargetStep > 3)
        {
            _logger.Warning("Invalid target step {TargetStep} for session {SessionId}", request.TargetStep, request.SessionId);
            return Result.Failure($"Invalid step number: {request.TargetStep}. Must be between 1 and 3.");
        }

        // Prevent navigation forward if current step not complete
        if (request.TargetStep > session.CurrentStep && !request.AllowIncomplete)
        {
            // Validate current step completion
            var validationResult = await ValidateCurrentStep(session);
            if (!validationResult.IsSuccess)
            {
                _logger.Warning("Cannot navigate forward: current step incomplete for session {SessionId}", request.SessionId);
                return validationResult;
            }
        }

        // Update session current step
        session.CurrentStep = request.TargetStep;
        var updateResult = await _sessionDao.UpdateSessionAsync(session);

        if (!updateResult.IsSuccess)
        {
            _logger.Error("Failed to update session {SessionId} step: {Error}", request.SessionId, updateResult.ErrorMessage);
            return Result.Failure(updateResult.ErrorMessage);
        }

        _logger.Information("Successfully navigated session {SessionId} to step {TargetStep}", request.SessionId, request.TargetStep);
        return Result.Success();
    }

    private async Task<Result> ValidateCurrentStep(ReceivingWorkflowSession session)
    {
        switch (session.CurrentStep)
        {
            case 1:
                // Step 1: PO Number, Part, and Load Count must be set
                if (string.IsNullOrWhiteSpace(session.PONumber))
                    return Result.Failure("PO Number is required");
                if (session.PartId <= 0)
                    return Result.Failure("Part must be selected");
                if (session.LoadCount <= 0)
                    return Result.Failure("Load count must be greater than 0");
                return Result.Success();

            case 2:
                // Step 2: All loads must have required data
                var loadsResult = await _loadDao.GetLoadsBySessionAsync(session.SessionId);
                if (!loadsResult.IsSuccess)
                    return Result.Failure("Failed to validate loads: " + loadsResult.ErrorMessage);

                var loads = loadsResult.Data;
                if (loads == null || loads.Count == 0)
                    return Result.Failure("No loads found for session");

                var invalidLoads = loads.Where(l =>
                    l.Weight <= 0 ||
                    string.IsNullOrWhiteSpace(l.HeatLot) ||
                    string.IsNullOrWhiteSpace(l.PackageType) ||
                    l.PackagesPerLoad <= 0).ToList();

                if (invalidLoads.Any())
                {
                    var loadNumbers = string.Join(", ", invalidLoads.Select(l => l.LoadNumber));
                    return Result.Failure($"Incomplete data for load(s): {loadNumbers}");
                }
                return Result.Success();

            case 3:
                // Step 3: Ready for save (validation happens in save command)
                return Result.Success();

            default:
                return Result.Failure($"Unknown step: {session.CurrentStep}");
        }
    }
}
