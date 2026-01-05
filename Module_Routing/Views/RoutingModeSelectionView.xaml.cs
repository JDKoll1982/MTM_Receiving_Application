using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Routing.Views;

public sealed partial class RoutingModeSelectionView : Page
{
    public RoutingModeSelectionViewModel ViewModel { get; }

    public RoutingModeSelectionView()
    {
        ViewModel = App.GetService<RoutingModeSelectionViewModel>();
        InitializeComponent();
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }
}
