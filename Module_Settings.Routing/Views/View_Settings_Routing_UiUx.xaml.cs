using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Routing.Views;

public sealed partial class View_Settings_Routing_UiUx : Page
{
    public ViewModel_Settings_Routing_UiUx ViewModel { get; }

    public View_Settings_Routing_UiUx(ViewModel_Settings_Routing_UiUx viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}
