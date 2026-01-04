using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Routing.Views;

public sealed partial class RoutingWizardStep1View : Page
{
    public RoutingWizardStep1ViewModel ViewModel { get; }

    public RoutingWizardStep1View()
    {
        ViewModel = App.GetService<RoutingWizardStep1ViewModel>();
        InitializeComponent();
    }
}
