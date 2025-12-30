using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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
        await ViewModel.InitializeAsync();
    }
}
