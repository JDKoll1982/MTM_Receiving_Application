using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_SettingsOverview : Page
{
    public ViewModel_Settings_Receiving_SettingsOverview ViewModel { get; }

    public View_Settings_Receiving_SettingsOverview(ViewModel_Settings_Receiving_SettingsOverview viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}
