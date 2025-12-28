using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Views.Dunnage;

public sealed partial class Dunnage_PartSelectionView : UserControl
{
    public Dunnage_PartSelectionViewModel ViewModel { get; }

    public Dunnage_PartSelectionView()
    {
        ViewModel = App.GetService<Dunnage_PartSelectionViewModel>();
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }
}
