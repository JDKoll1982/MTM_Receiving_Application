using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.ViewModels.Shared;

/// <summary>
/// ViewModel for the splash screen window.
/// Tracks startup progress and displays status messages.
/// </summary>
public partial class SplashScreenViewModel : BaseViewModel
{
    [ObservableProperty]
    private double _progressPercentage;

    [ObservableProperty]
    private bool _isIndeterminate = false;

    public SplashScreenViewModel(
        IService_ErrorHandler errorHandler,
        ILoggingService logger) 
        : base(errorHandler, logger)
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

