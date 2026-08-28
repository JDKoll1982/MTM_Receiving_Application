using System;
using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
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

    private readonly ViewModel_ShipRecTools_ToolSelection _toolSelectionViewModel;
    private readonly ViewModel_Tool_OutsideServiceHistory _outsideServiceHistoryViewModel;
    private readonly ViewModel_Tool_MaterialAvailabilityBoard _materialAvailabilityBoardViewModel;
    private readonly ViewModel_Tool_POLineSpecSearch _poLineSpecSearchViewModel;
    private readonly ViewModel_Tool_DunnageBook _dunnageBookViewModel;
    private readonly ViewModel_Tool_WeldedCoils _weldedCoilsViewModel;

    public ViewModel_ShipRecTools_Main ViewModel { get; }

    public View_ShipRecTools_Main(
        ViewModel_ShipRecTools_Main viewModel,
        View_ShipRecTools_ToolSelection toolSelectionView,
        View_Tool_OutsideServiceHistory outsideServiceHistoryView,
        View_Tool_MaterialAvailabilityBoard materialAvailabilityBoardView,
        View_Tool_POLineSpecSearch poLineSpecSearchView,
        View_Tool_DunnageBook dunnageBookView,
        View_Tool_WeldedCoils weldedCoilsView
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(toolSelectionView);
        ArgumentNullException.ThrowIfNull(outsideServiceHistoryView);
        ArgumentNullException.ThrowIfNull(materialAvailabilityBoardView);
        ArgumentNullException.ThrowIfNull(poLineSpecSearchView);
        ArgumentNullException.ThrowIfNull(dunnageBookView);
        ArgumentNullException.ThrowIfNull(weldedCoilsView);

        ViewModel = viewModel;
        _toolSelectionViewModel = toolSelectionView.ViewModel;
        _outsideServiceHistoryViewModel = outsideServiceHistoryView.ViewModel;
        _materialAvailabilityBoardViewModel = materialAvailabilityBoardView.ViewModel;
        _poLineSpecSearchViewModel = poLineSpecSearchView.ViewModel;
        _dunnageBookViewModel = dunnageBookView.ViewModel;
        _weldedCoilsViewModel = weldedCoilsView.ViewModel;
        InitializeComponent();

        ToolSelectionHost.Content = toolSelectionView;
        OutsideServiceHistoryHost.Content = outsideServiceHistoryView;
        MaterialAvailabilityBoardHost.Content = materialAvailabilityBoardView;
        POLineSpecSearchHost.Content = poLineSpecSearchView;
        DunnageBookHost.Content = dunnageBookView;
        WeldedCoilsHost.Content = weldedCoilsView;

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
                or nameof(ViewModel_ShipRecTools_Main.IsPOLineSpecSearchVisible)
                or nameof(ViewModel_ShipRecTools_Main.IsDunnageBookVisible)
                or nameof(ViewModel_ShipRecTools_Main.IsWeldedCoilsVisible)
        )
        {
            UpdateActiveViewStatus();
        }
    }

    private void UpdateActiveViewStatus()
    {
        if (ViewModel.IsToolSelectionVisible)
        {
            RestoreMainWindowSize();
            _toolSelectionViewModel.ActivateView();
            return;
        }

        if (ViewModel.IsOutsideServiceHistoryVisible)
        {
            RestoreMainWindowSize();
            _outsideServiceHistoryViewModel.ActivateView();
            return;
        }

        if (ViewModel.IsMaterialAvailabilityBoardVisible)
        {
            RestoreMainWindowSize();
            _materialAvailabilityBoardViewModel.ActivateView();
            return;
        }

        if (ViewModel.IsPOLineSpecSearchVisible)
        {
            RestoreMainWindowSize();
            _poLineSpecSearchViewModel.ActivateView();
            return;
        }

        if (ViewModel.IsDunnageBookVisible)
        {
            RestoreMainWindowSize();
            _dunnageBookViewModel.ActivateView();
            return;
        }

        if (ViewModel.IsWeldedCoilsVisible)
        {
            RestoreMainWindowSize();
            _weldedCoilsViewModel.ActivateView();
        }
    }

    private static void RestoreMainWindowSize()
    {
        if (App.MainWindow is not MTM_Receiving_Application.MainWindow mainWindow)
        {
            return;
        }

        mainWindow.AppWindow.Resize(
            mainWindow.GetScaledWindowSize(DefaultMainWindowWidth, DefaultMainWindowHeight)
        );
    }
}
