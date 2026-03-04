using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Tool selection screen for the ShipRec Tools module.
/// Displays tool cards grouped by category populated from IService_ShipRecTools_Navigation.
/// </summary>
public sealed partial class View_ShipRecTools_ToolSelection : UserControl
{
    public ViewModel_ShipRecTools_ToolSelection ViewModel { get; }

    public View_ShipRecTools_ToolSelection(ViewModel_ShipRecTools_ToolSelection viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.LoadTools();
    }
}
