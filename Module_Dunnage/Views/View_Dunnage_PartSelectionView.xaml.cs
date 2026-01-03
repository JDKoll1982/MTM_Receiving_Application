using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_PartSelectionView : UserControl
{
    public ViewModel_Dunnage_PartSelection ViewModel { get; }

    public View_Dunnage_PartSelectionView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_PartSelection>();
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Dunnage_PartSelectionView: OnLoaded called");
        await ViewModel.InitializeAsync();
    }
}
