using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Shared.ViewModels;

/// <summary>
/// Base ViewModel providing common functionality for all ViewModels
/// </summary>
public abstract partial class ViewModel_Shared_Base : ObservableObject
{
    protected readonly IService_ErrorHandler _errorHandler;
    protected readonly IService_LoggingUtility _logger;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    protected ViewModel_Shared_Base(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
    {
        _errorHandler = errorHandler;
        _logger = logger;
    }
}

