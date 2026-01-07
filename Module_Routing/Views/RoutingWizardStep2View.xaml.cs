using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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

    private void ListView_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter && ViewModel.SelectedRecipient != null)
        {
            e.Handled = true;
            ViewModel.ProceedToStep3Command.Execute(null);
        }
    }
}
