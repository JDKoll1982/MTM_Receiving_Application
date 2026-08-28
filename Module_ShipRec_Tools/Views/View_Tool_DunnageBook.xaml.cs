using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// View for the dunnage book generator tool.
/// </summary>
public sealed partial class View_Tool_DunnageBook : Page
{
    public ViewModel_Tool_DunnageBook ViewModel { get; }

    public View_Tool_DunnageBook(ViewModel_Tool_DunnageBook viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.ShowPreviewDialogAsync = ShowPreviewDialogAsync;
    }

    private async Task ShowPreviewDialogAsync(
        ViewModel_Dialog_DunnageBookPreview dialogViewModel
    )
    {
        var dialog = new Dialog_DunnageBookPreview(dialogViewModel) { XamlRoot = XamlRoot };
        await dialog.ShowAsync();
    }
}
