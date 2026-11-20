using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Settings.Receiving.Models;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;
using Windows.System;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_WeldedParts : Page
{
    public ViewModel_Settings_Receiving_WeldedParts ViewModel { get; }

    public View_Settings_Receiving_WeldedParts(
        ViewModel_Settings_Receiving_WeldedParts viewModel
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void AddPartBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter)
        {
            return;
        }

        e.Handled = true;
        if (ViewModel.AddPartCommand.CanExecute(null))
        {
            ViewModel.AddPartCommand.Execute(null);
        }
    }

    private async void RemoveRowButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Model_Settings_ReceivingWeldedPart part })
        {
            await ViewModel.RemoveAsync(part);
        }
    }
}
