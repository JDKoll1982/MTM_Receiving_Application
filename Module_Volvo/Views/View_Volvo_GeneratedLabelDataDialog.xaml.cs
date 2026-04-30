using System.Threading.Tasks;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Volvo.ViewModels;
using Windows.System;

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

    private void OnClearLabelDataClick(object sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
        bool clearAllRows =
            (shiftState & Windows.UI.Core.CoreVirtualKeyStates.Down)
            == Windows.UI.Core.CoreVirtualKeyStates.Down;

        if (ViewModel.ClearLabelDataCommand.CanExecute(clearAllRows))
        {
            ViewModel.ClearLabelDataCommand.Execute(clearAllRows);
        }
    }
}
