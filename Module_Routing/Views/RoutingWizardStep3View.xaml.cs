using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Routing.Views;

public sealed partial class RoutingWizardStep3View : Page
{
    public RoutingWizardStep3ViewModel ViewModel { get; }

    public RoutingWizardStep3View()
    {
        ViewModel = App.GetService<RoutingWizardStep3ViewModel>();
        InitializeComponent();
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        // Load review data when page loads
        ViewModel.LoadReviewData();
    }
}
