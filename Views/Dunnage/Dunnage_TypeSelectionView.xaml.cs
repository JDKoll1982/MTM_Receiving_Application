using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Views.Dunnage;

/// <summary>
/// UserControl for Dunnage Type Selection view with 3x3 paginated grid
/// </summary>
public sealed partial class Dunnage_TypeSelectionView : UserControl
{
    public Dunnage_TypeSelectionViewModel ViewModel { get; }

    public Dunnage_TypeSelectionView()
    {
        ViewModel = App.GetService<Dunnage_TypeSelectionViewModel>();
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
