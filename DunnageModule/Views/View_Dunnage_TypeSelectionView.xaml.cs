using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.DunnageModule.Models;
using MTM_Receiving_Application.DunnageModule.ViewModels;

namespace MTM_Receiving_Application.DunnageModule.Views;

/// <summary>
/// UserControl for Dunnage Type Selection view with 3x3 paginated grid
/// </summary>
public sealed partial class View_Dunnage_TypeSelectionView : UserControl
{
    public ViewModel_Dunnage_TypeSelection ViewModel { get; }

    public View_Dunnage_TypeSelectionView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_TypeSelection>();
        InitializeComponent();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Dunnage_TypeSelectionView: OnLoaded called");
        await ViewModel.InitializeAsync();
    }

    private async void TypeCard_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is Model_DunnageType type)
        {
            await ViewModel.SelectTypeCommand.ExecuteAsync(type);
        }
    }
}
