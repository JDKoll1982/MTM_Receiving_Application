using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Routing.Views;

public sealed partial class RoutingWizardStep2View : Page
{
    public RoutingWizardStep2ViewModel ViewModel { get; }

    public RoutingWizardStep2View()
    {
        ViewModel = App.GetService<RoutingWizardStep2ViewModel>();
        InitializeComponent();
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        // Load recipients when page loads
        await ViewModel.LoadRecipientsCommand.ExecuteAsync(null);
    }
}
