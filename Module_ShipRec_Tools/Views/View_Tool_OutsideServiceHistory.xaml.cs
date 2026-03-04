using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Outside Service Provider History lookup tool view.
/// Displays dispatch history for a part number retrieved from Infor Visual (read-only).
/// </summary>
public sealed partial class View_Tool_OutsideServiceHistory : Page
{
    public ViewModel_Tool_OutsideServiceHistory ViewModel { get; }

    public View_Tool_OutsideServiceHistory(ViewModel_Tool_OutsideServiceHistory viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        DataContext = ViewModel;
        this.InitializeComponent();
    }
}
