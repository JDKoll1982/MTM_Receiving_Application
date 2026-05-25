using System;
using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;
using Windows.Graphics;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Main container page for the ShipRec Tools module.
/// Hosts the tool selection screen and individual tool views using DI-injected sub-views.
/// </summary>
public sealed partial class View_ShipRecTools_Main : Page
{
    private const int DefaultMainWindowWidth = 1450;
    private const int DefaultMainWindowHeight = 900;
    private const int CustomerPullPackWindowWidth = 1880;
    private const int CustomerPullPackWindowHeight = 950;

    private readonly ViewModel_ShipRecTools_ToolSelection _toolSelectionViewModel;
    private readonly ViewModel_Tool_OutsideServiceHistory _outsideServiceHistoryViewModel;
    private readonly ViewModel_Tool_MaterialAvailabilityBoard _materialAvailabilityBoardViewModel;
    private readonly ViewModel_Tool_CustomerPullPackReport _customerPullPackReportViewModel;

    public ViewModel_ShipRecTools_Main ViewModel { get; }

    public View_ShipRecTools_Main(
        ViewModel_ShipRecTools_Main viewModel,
        View_ShipRecTools_ToolSelection toolSelectionView,
        View_Tool_OutsideServiceHistory outsideServiceHistoryView,
        View_Tool_MaterialAvailabilityBoard materialAvailabilityBoardView,
        View_Tool_CustomerPullPackReport customerPullPackReportView
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(toolSelectionView);
        ArgumentNullException.ThrowIfNull(outsideServiceHistoryView);
        ArgumentNullException.ThrowIfNull(materialAvailabilityBoardView);
        ArgumentNullException.ThrowIfNull(customerPullPackReportView);

        ViewModel = viewModel;
        _toolSelectionViewModel = toolSelectionView.ViewModel;
        _outsideServiceHistoryViewModel = outsideServiceHistoryView.ViewModel;
        _materialAvailabilityBoardViewModel = materialAvailabilityBoardView.ViewModel;
        _customerPullPackReportViewModel = customerPullPackReportView.ViewModel;
        InitializeComponent();

        ToolSelectionHost.Content = toolSelectionView;
        OutsideServiceHistoryHost.Content = outsideServiceHistoryView;
        MaterialAvailabilityBoardHost.Content = materialAvailabilityBoardView;
        CustomerPullPackHost.Content = customerPullPackReportView;

        // Wire tool selection events to main ViewModel navigation
        toolSelectionView.ViewModel.ToolSelected += ViewModel.NavigateToTool;
        ViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        UpdateActiveViewStatus();
    }

    private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (
            e.PropertyName
            is nameof(ViewModel_ShipRecTools_Main.IsToolSelectionVisible)
                or nameof(ViewModel_ShipRecTools_Main.IsOutsideServiceHistoryVisible)
                or nameof(ViewModel_ShipRecTools_Main.IsMaterialAvailabilityBoardVisible)
                or nameof(ViewModel_ShipRecTools_Main.IsCustomerPullPackVisible)
        )
        {
            UpdateActiveViewStatus();
        }
    }

    private void UpdateActiveViewStatus()
    {
        if (ViewModel.IsToolSelectionVisible)
        {
            RestoreMainWindowFromCustomerPullPack();
            _toolSelectionViewModel.ActivateView();
            return;
        }

        if (ViewModel.IsOutsideServiceHistoryVisible)
        {
            RestoreMainWindowFromCustomerPullPack();
            _outsideServiceHistoryViewModel.ActivateView();
            return;
        }

        if (ViewModel.IsMaterialAvailabilityBoardVisible)
        {
            RestoreMainWindowFromCustomerPullPack();
            _materialAvailabilityBoardViewModel.ActivateView();
            return;
        }

        if (ViewModel.IsCustomerPullPackVisible)
        {
            ResizeMainWindowForCustomerPullPack();
            _customerPullPackReportViewModel.ActivateView();
        }
    }

    private static void ResizeMainWindowForCustomerPullPack()
    {
        if (App.MainWindow is not MTM_Receiving_Application.MainWindow mainWindow)
        {
            return;
        }

        var currentSize = mainWindow.AppWindow.Size;
        var desiredWidth = Math.Max(currentSize.Width, CustomerPullPackWindowWidth);
        var desiredHeight = Math.Max(currentSize.Height, CustomerPullPackWindowHeight);
        mainWindow.AppWindow.Resize(new SizeInt32(desiredWidth, desiredHeight));
    }

    private static void RestoreMainWindowFromCustomerPullPack()
    {
        if (App.MainWindow is not MTM_Receiving_Application.MainWindow mainWindow)
        {
            return;
        }

        mainWindow.AppWindow.Resize(new SizeInt32(DefaultMainWindowWidth, DefaultMainWindowHeight));
    }
}
