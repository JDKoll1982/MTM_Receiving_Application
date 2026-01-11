using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

/// <summary>
/// UserControl for Dunnage Type Selection view with 3x3 paginated grid
/// </summary>
public sealed partial class View_dunnage_typeselectionView : UserControl
{
    public ViewModel_dunnage_typeselection ViewModel { get; }
    private readonly IService_Focus _focusService;

    public View_dunnage_typeselectionView()
    {
        ViewModel = App.GetService<ViewModel_dunnage_typeselection>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();

        _focusService.AttachFocusOnVisibility(this);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("dunnage_typeselectionView: OnLoaded called");
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
