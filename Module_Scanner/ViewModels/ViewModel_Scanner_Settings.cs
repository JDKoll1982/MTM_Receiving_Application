using System;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.ViewModels;

/// <summary>
/// Placeholder ViewModel for the scanner settings page.
/// </summary>
public partial class ViewModel_Scanner_Settings : ViewModel_Shared_Base
{
    private readonly IService_ScannerNavigation _navigationService;

    public ViewModel_Scanner_Settings(
        IService_ScannerNavigation navigationService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        _navigationService = navigationService;
    }

    [RelayCommand]
    private void NavigateToWorkbench()
    {
        _navigationService.ShowWorkbench();
    }

    [RelayCommand]
    private void NavigateToHistory()
    {
        _navigationService.ShowHistory();
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        _navigationService.ShowSettings();
    }
}