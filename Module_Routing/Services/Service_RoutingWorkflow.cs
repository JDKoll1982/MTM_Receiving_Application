using System;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Routing.Enums;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Implementation of the routing workflow service.
/// </summary>
public class Service_RoutingWorkflow : IService_RoutingWorkflow
{
    private readonly IService_LoggingUtility _logger;

    public event EventHandler<string>? StatusMessageRaised;
    public event EventHandler? StepChanged;

    public Enum_Routing_WorkflowStep CurrentStep { get; private set; } = Enum_Routing_WorkflowStep.ModeSelection;

    public Service_RoutingWorkflow(IService_LoggingUtility logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void RaiseStatusMessage(string message)
    {
        StatusMessageRaised?.Invoke(this, message);
    }

    public void GoToStep(Enum_Routing_WorkflowStep step)
    {
        if (CurrentStep != step)
        {
            _logger.LogInfo($"Transitioning from {CurrentStep} to {step}");
            CurrentStep = step;
            StepChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ResetWorkflow()
    {
        _logger.LogInfo("Resetting routing workflow");
        GoToStep(Enum_Routing_WorkflowStep.ModeSelection);
    }
}
