using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;
using Windows.System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Welded Coils tool view. Keeps business logic in the ViewModel; this file only
/// forwards the row-level delete (after confirming it) and the Add-box Enter key
/// to the ViewModel.
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

    private async void OnHelpClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var helpService = App.GetService<MTM_Receiving_Application.Module_Core.Contracts.Services.IService_Help>();
        await helpService.ShowHelpAsync("ShipRecTools.WeldedCoils", XamlRoot);
    }

    private async void DeleteRowButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Model_Tool_WeldedCoil coil)
        {
            if (await ConfirmDeleteAsync(coil))
            {
                await ViewModel.DeleteCoilAsync(coil);
            }
        }
    }

    /// <summary>
    /// Asks before removing a row because the delete permanently removes the part
    /// from the welded coils table.
    /// </summary>
    private async Task<bool> ConfirmDeleteAsync(Model_Tool_WeldedCoil coil)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "Delete welded coil",
            Content = $"Delete part {coil.PartId} from the welded coils list? This cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
        };

        MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
            dialog,
            XamlRoot
        );

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    private void AddBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && ViewModel.AddCoilCommand.CanExecute(null))
        {
            ViewModel.AddCoilCommand.Execute(null);
        }
    }
}
