using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.DunnageModule.ViewModels;

namespace MTM_Receiving_Application.DunnageModule.Views;

public sealed partial class View_Dunnage_AdminPartsView : Page
{
    public ViewModel_Dunnage_AdminParts ViewModel { get; }

    public View_Dunnage_AdminPartsView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_AdminParts>();
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
