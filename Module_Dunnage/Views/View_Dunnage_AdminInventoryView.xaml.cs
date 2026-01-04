using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_AdminInventoryView : Page
{
    public ViewModel_Dunnage_AdminInventory ViewModel { get; }

    public View_Dunnage_AdminInventoryView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_AdminInventory>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        App.GetService<IService_LoggingUtility>().LogInfo("Admin Inventory View loaded", "AdminInventoryView");
        await ViewModel.InitializeAsync();
    }
}

