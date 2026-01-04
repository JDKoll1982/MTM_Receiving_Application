using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_AdminPartsView : Page
{
    public ViewModel_Dunnage_AdminParts ViewModel { get; }
    private readonly IService_Focus _focusService;

    public View_Dunnage_AdminPartsView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_AdminParts>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();

        _focusService.AttachFocusOnVisibility(this);
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
