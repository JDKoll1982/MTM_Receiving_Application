using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.ViewModels;

/// <summary>
/// Main host ViewModel for the scanner module placeholder scaffold.
/// </summary>
public partial class ViewModel_Scanner_Main
    : ViewModel_Shared_Base,
        IViewModel_HeaderTitleProvider
{
    private readonly IService_ScannerNavigation _navigationService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentHeaderTitle))]
    private string _currentPageTitle = "Scanner - Workbench";

    [ObservableProperty]
    private bool _isWorkbenchVisible = true;

    [ObservableProperty]
    private bool _isHistoryVisible;

    [ObservableProperty]
    private bool _isSettingsVisible;

    [ObservableProperty]
    private bool _isAdvancedBulkMoveVisible;

    public string CurrentHeaderTitle => CurrentPageTitle;

    public ViewModel_Scanner_Main(
        IService_ScannerNavigation navigationService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        _navigationService = navigationService;
        _navigationService.PropertyChanged += OnNavigationChanged;
        UpdateVisiblePage();
    }

    private void OnNavigationChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IService_ScannerNavigation.CurrentPage))
        {
            UpdateVisiblePage();
        }
    }

    private void UpdateVisiblePage()
    {
        IsWorkbenchVisible = _navigationService.CurrentPage == Enum_ScannerPage.Workbench;
        IsHistoryVisible = _navigationService.CurrentPage == Enum_ScannerPage.History;
        IsSettingsVisible = _navigationService.CurrentPage == Enum_ScannerPage.Settings;
        IsAdvancedBulkMoveVisible =
            _navigationService.CurrentPage == Enum_ScannerPage.AdvancedBulkMove;

        CurrentPageTitle = _navigationService.CurrentPage switch
        {
            Enum_ScannerPage.Workbench => "Scanner - Workbench",
            Enum_ScannerPage.History => "Scanner - History",
            Enum_ScannerPage.Settings => "Scanner - Settings",
            Enum_ScannerPage.AdvancedBulkMove => "Scanner - Advanced Bulk Move",
            _ => "Scanner",
        };
    }
}