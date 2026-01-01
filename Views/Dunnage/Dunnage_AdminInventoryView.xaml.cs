using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Views.Dunnage;

public sealed partial class Dunnage_AdminInventoryView : Page
{
    public Dunnage_AdminInventoryViewModel ViewModel { get; }

    public Dunnage_AdminInventoryView()
    {
        ViewModel = App.GetService<Dunnage_AdminInventoryViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        App.GetService<IService_LoggingUtility>().LogInfo("Admin Inventory View loaded", "AdminInventoryView");
        await ViewModel.InitializeAsync();
    }
}
