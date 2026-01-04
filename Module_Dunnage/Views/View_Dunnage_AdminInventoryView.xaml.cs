using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_AdminInventoryView : Page
{
    public ViewModel_Dunnage_AdminInventory ViewModel { get; }
    private readonly IService_Focus _focusService;

    public View_Dunnage_AdminInventoryView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_AdminInventory>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();

        _focusService.AttachFocusOnVisibility(this);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        App.GetService<IService_LoggingUtility>().LogInfo("Admin Inventory View loaded", "AdminInventoryView");
        await ViewModel.InitializeAsync();
    }
}

