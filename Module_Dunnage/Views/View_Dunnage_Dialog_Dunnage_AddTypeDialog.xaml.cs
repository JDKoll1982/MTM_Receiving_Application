using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Views.Shared;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_Dialog_Dunnage_AddTypeDialog : ContentDialog
{
    public ViewModel_Dunnage_AddTypeDialog ViewModel { get; }

    public View_Dunnage_Dialog_Dunnage_AddTypeDialog()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_AddTypeDialog>();
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
