using CommunityToolkit.Mvvm.ComponentModel;
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

    public ViewModel_Shared_SplashScreen(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
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
    }
}


