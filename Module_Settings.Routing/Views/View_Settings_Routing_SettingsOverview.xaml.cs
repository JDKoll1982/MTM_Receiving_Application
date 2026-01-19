using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Routing.Views;

public sealed partial class View_Settings_Routing_SettingsOverview : Page
{
    public ViewModel_Settings_Routing_SettingsOverview ViewModel { get; }

    public View_Settings_Routing_SettingsOverview(ViewModel_Settings_Routing_SettingsOverview viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}
