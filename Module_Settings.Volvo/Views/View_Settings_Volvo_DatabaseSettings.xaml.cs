using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Volvo.Views;

public sealed partial class View_Settings_Volvo_DatabaseSettings : Page
{
    public ViewModel_Settings_Volvo_DatabaseSettings ViewModel { get; }

    public View_Settings_Volvo_DatabaseSettings(ViewModel_Settings_Volvo_DatabaseSettings viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}
