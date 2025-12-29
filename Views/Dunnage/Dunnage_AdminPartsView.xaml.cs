using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Views.Dunnage;

public sealed partial class Dunnage_AdminPartsView : Page
{
    public Dunnage_AdminPartsViewModel ViewModel { get; }

    public Dunnage_AdminPartsView()
    {
        ViewModel = App.GetService<Dunnage_AdminPartsViewModel>();
        InitializeComponent();
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadPartsCommand.ExecuteAsync(null);
    }

    private async void OnSearchKeyboardAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        await ViewModel.SearchPartsCommand.ExecuteAsync(null);
        args.Handled = true;
    }
}
