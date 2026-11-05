using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using MTM_Receiving_Application.Infrastructure.Configuration;
using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Shared.ViewModels;

/// <summary>
/// ViewModel for the splash screen window.
/// Tracks startup progress and displays status messages.
/// </summary>
public partial class ViewModel_Shared_SplashScreen : ViewModel_Shared_Base
{
    [ObservableProperty]
    private double _progressPercentage;

    [ObservableProperty]
    private bool _isIndeterminate = false;

    /// <summary>
    /// How far through the current step this is, from 0 to 100. The main bar keeps showing the whole
    /// startup; this one shows the work inside whichever step is running, so a step that takes ten seconds
    /// is not one unchanging message.
    /// </summary>
    [ObservableProperty]
    private double _stepProgressPercentage;

    /// <summary>What the current step is doing right now, e.g. "Saving your settings (12 of 49)".</summary>
    [ObservableProperty]
    private string _stepStatusMessage = string.Empty;

    /// <summary>Whether a step is running, which is when the second bar is shown at all.</summary>
    [ObservableProperty]
    private bool _isStepVisible;

    public string AppVersion { get; }

    public ViewModel_Shared_SplashScreen(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService,
        IOptions<ApplicationSettings> applicationSettings
    )
        : base(errorHandler, logger, notificationService)
    {
        AppVersion = $"Version {applicationSettings.Value.Version}";
        StatusMessage = "Initializing...";
    }

    /// <summary>
    /// Updates progress percentage and status message
    /// </summary>
    /// <param name="percentage">Progress percentage (0-100)</param>
    /// <param name="message">Status message to display</param>
    public void UpdateProgress(double percentage, string message)
    {
        ProgressPercentage = percentage;
        StatusMessage = message;
        IsIndeterminate = false;
    }

    /// <summary>
    /// Shows what the current step is doing, on a second bar under the first. Call
    /// <see cref="ClearStepProgress"/> when the step finishes so the bar goes away rather than sitting at
    /// 100% for the rest of startup.
    /// </summary>
    /// <param name="percentage">How far through the step this is, 0 to 100.</param>
    /// <param name="message">What is happening, in the operator's words.</param>
    public void UpdateStepProgress(double percentage, string message)
    {
        StepProgressPercentage = percentage;
        StepStatusMessage = message;
        IsStepVisible = true;
    }

    /// <summary>Removes the second bar, for when a step has finished.</summary>
    public void ClearStepProgress()
    {
        IsStepVisible = false;
        StepProgressPercentage = 0;
        StepStatusMessage = string.Empty;
    }

    /// <summary>
    /// Set splash screen to indeterminate/pulsing state
    /// Used when waiting for user input (dialogs)
    /// </summary>
    /// <param name="message">Status message to display</param>
    public void SetIndeterminate(string message)
    {
        StatusMessage = message;
        IsIndeterminate = true;
    }

    /// <summary>
    /// Reset progress to initial state
    /// </summary>
    public void Reset()
    {
        ProgressPercentage = 0;
        StatusMessage = "Initializing...";
        IsIndeterminate = false;
        ClearStepProgress();
    }
}
