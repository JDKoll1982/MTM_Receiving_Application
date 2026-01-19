using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.Views;

public sealed partial class View_Settings_Dunnage_UserPreferences : Page
{
    public ViewModel_Settings_Dunnage_UserPreferences ViewModel { get; }

    public View_Settings_Dunnage_UserPreferences(ViewModel_Settings_Dunnage_UserPreferences viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}
