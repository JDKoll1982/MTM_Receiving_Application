using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Main container page for the ShipRec Tools module.
/// Hosts the tool selection screen and individual tool views using DI-injected sub-views.
/// </summary>
public sealed partial class View_ShipRecTools_Main : Page
{
    public ViewModel_ShipRecTools_Main ViewModel { get; }

    public View_ShipRecTools_Main(
        ViewModel_ShipRecTools_Main viewModel,
        View_ShipRecTools_ToolSelection toolSelectionView,
        View_Tool_OutsideServiceHistory outsideServiceHistoryView
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(toolSelectionView);
        ArgumentNullException.ThrowIfNull(outsideServiceHistoryView);

        ViewModel = viewModel;
        InitializeComponent();

        ToolSelectionHost.Content = toolSelectionView;
        OutsideServiceHistoryHost.Content = outsideServiceHistoryView;

        // Wire tool selection events to main ViewModel navigation
        toolSelectionView.ViewModel.ToolSelected += ViewModel.NavigateToTool;
    }
}
