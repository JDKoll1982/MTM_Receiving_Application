using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Volvo.ViewModels;

namespace MTM_Receiving_Application.Module_Volvo.Views;

/// <summary>
/// Page for viewing and managing Volvo shipment history
/// </summary>
public sealed partial class View_Volvo_History : Page
{
    public ViewModel_Volvo_History ViewModel { get; }

    public View_Volvo_History()
    {
        ViewModel = App.GetService<ViewModel_Volvo_History>();
        InitializeComponent();
    }

    private async void OnPageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Auto-filter on page load (last 30 days)
        await ViewModel.FilterCommand.ExecuteAsync(null);
    }
}
