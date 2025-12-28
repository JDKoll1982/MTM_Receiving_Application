using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Views.Dunnage;

public sealed partial class Dunnage_DetailsEntryView : UserControl
{
    public Dunnage_DetailsEntryViewModel ViewModel { get; }

    public Dunnage_DetailsEntryView()
    {
        ViewModel = App.GetService<Dunnage_DetailsEntryViewModel>();
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadSpecsForSelectedPartAsync();
    }
}
