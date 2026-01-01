using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Views.Dunnage;

public sealed partial class Dunnage_AdminTypesView : Page
{
    public Dunnage_AdminTypesViewModel ViewModel { get; }

    public Dunnage_AdminTypesView()
    {
        ViewModel = App.GetService<Dunnage_AdminTypesViewModel>();
        InitializeComponent();
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        App.GetService<IService_LoggingUtility>().LogInfo("Admin Types View loaded", "AdminTypesView");
        await ViewModel.LoadTypesCommand.ExecuteAsync(null);
    }
}
