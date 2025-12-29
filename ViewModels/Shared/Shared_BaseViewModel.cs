using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.ViewModels.Shared;

/// <summary>
/// Base ViewModel providing common functionality for all ViewModels
/// </summary>
public abstract partial class Shared_BaseViewModel : ObservableObject
{
    protected readonly IService_ErrorHandler _errorHandler;
    protected readonly IService_LoggingUtility _logger;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    protected Shared_BaseViewModel(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
    {
        _errorHandler = errorHandler;
        _logger = logger;
    }
}
