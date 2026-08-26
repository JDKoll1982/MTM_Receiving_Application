using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Main container page for the scanner module scaffold.
/// Hosts the placeholder workbench, history, and settings views using DI-injected subviews.
/// </summary>
public sealed partial class View_Scanner_Main : Page
{
    private readonly IService_AdaptiveLayout _adaptiveLayout;

    public ViewModel_Scanner_Main ViewModel { get; }

    public View_Scanner_Main(
        ViewModel_Scanner_Main viewModel,
        IService_AdaptiveLayout adaptiveLayout,
        View_Scanner_Workbench workbenchView,
        View_Scanner_History historyView,
        View_Scanner_Settings settingsView,
        View_Scanner_AdvancedBulkMove advancedBulkMoveView
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(adaptiveLayout);
        ArgumentNullException.ThrowIfNull(workbenchView);
        ArgumentNullException.ThrowIfNull(historyView);
        ArgumentNullException.ThrowIfNull(settingsView);
        ArgumentNullException.ThrowIfNull(advancedBulkMoveView);

        ViewModel = viewModel;
        _adaptiveLayout = adaptiveLayout;
        InitializeComponent();
        DataContext = ViewModel;

        WorkbenchHost.Content = workbenchView;
        HistoryHost.Content = historyView;
        SettingsHost.Content = settingsView;
        AdvancedBulkMoveHost.Content = advancedBulkMoveView;

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyAdaptiveLayout();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyAdaptiveLayout();
    }

    private void ApplyAdaptiveLayout()
    {
        ScannerMainRootGrid.Margin = _adaptiveLayout.GetScannerContentPadding(ActualWidth);
    }
}