using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.DunnageModule.ViewModels;

namespace MTM_Receiving_Application.DunnageModule.Views;

public sealed partial class View_Dunnage_DetailsEntryView : UserControl
{
    public ViewModel_Dunnage_DetailsEntry ViewModel { get; }

    public View_Dunnage_DetailsEntryView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_DetailsEntry>();
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadSpecsForSelectedPartAsync();
    }
}
