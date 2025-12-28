using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.ViewModels.Shared;

/// <summary>
/// Base ViewModel providing common functionality for all ViewModels
/// </summary>
public abstract class Shared_BaseViewModel : ObservableObject
{
    protected readonly IService_ErrorHandler _errorHandler;
    protected readonly IService_LoggingUtility _logger;

    protected Shared_BaseViewModel(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
    {
        _errorHandler = errorHandler;
        _logger = logger;
    }

    private bool _isBusy;
    /// <summary>
    /// Indicates if the ViewModel is currently performing an operation
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string _statusMessage = string.Empty;
    /// <summary>
    /// Status message displayed to the user
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
}
