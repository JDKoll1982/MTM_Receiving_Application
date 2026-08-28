using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;
using Windows.System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Welded Coils tool view. Keeps business logic in the ViewModel; this file only
/// forwards the row-level active toggle and the Add-box Enter key to the ViewModel.
/// </summary>
public sealed partial class View_Tool_WeldedCoils : Page
{
    public ViewModel_Tool_WeldedCoils ViewModel { get; }

    public View_Tool_WeldedCoils(ViewModel_Tool_WeldedCoils viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    private async void ToggleActiveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Model_Tool_WeldedCoil coil)
        {
            await ViewModel.ToggleActiveAsync(coil);
        }
    }

    private void AddBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.AddCoilCommand.CanExecute(null))
        {
            ViewModel.AddCoilCommand.Execute(null);
        }
    }
}
