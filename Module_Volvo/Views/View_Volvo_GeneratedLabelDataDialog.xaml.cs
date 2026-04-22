using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Volvo.ViewModels;

namespace MTM_Receiving_Application.Module_Volvo.Views;

/// <summary>
/// Dialog for reviewing and clearing the active Volvo generated-label queue.
/// </summary>
public sealed partial class View_Volvo_GeneratedLabelDataDialog : ContentDialog
{
    public ViewModel_Volvo_GeneratedLabelDataDialog ViewModel { get; }

    public View_Volvo_GeneratedLabelDataDialog(ViewModel_Volvo_GeneratedLabelDataDialog viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
    }

    public async Task InitializeAsync()
    {
        await ViewModel.LoadAsync();
    }
}
