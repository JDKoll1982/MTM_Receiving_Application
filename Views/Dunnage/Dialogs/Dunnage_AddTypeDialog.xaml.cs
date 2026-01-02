using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Dunnage;
using MTM_Receiving_Application.Views.Shared;

namespace MTM_Receiving_Application.Views.Dunnage.Dialogs;

public sealed partial class Dunnage_AddTypeDialog : ContentDialog
{
    public Dunnage_AddTypeDialogViewModel ViewModel { get; }

    public Dunnage_AddTypeDialog()
    {
        ViewModel = App.GetService<Dunnage_AddTypeDialogViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    private async void OnSelectIconClick(object sender, RoutedEventArgs e)
    {
        var iconSelector = new Shared_IconSelectorWindow();

        // Set initial selection to current icon
        iconSelector.SetInitialSelection(ViewModel.SelectedIcon);

        iconSelector.Activate();

        // Wait for the window to close and get the result
        var selectedIcon = await iconSelector.WaitForSelectionAsync();

        if (selectedIcon.HasValue)
        {
            ViewModel.SelectedIcon = selectedIcon.Value;
        }
    }
}
