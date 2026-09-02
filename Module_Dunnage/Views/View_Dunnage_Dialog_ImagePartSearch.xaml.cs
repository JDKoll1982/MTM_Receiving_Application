using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_Dialog_ImagePartSearch : UserControl
{
    public ViewModel_Dunnage_ImagePartSearchDialog ViewModel { get; }

    public View_Dunnage_Dialog_ImagePartSearch(ViewModel_Dunnage_ImagePartSearchDialog viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        InitializeComponent();
        DataContext = ViewModel;
    }

    public View_Dunnage_Dialog_ImagePartSearch()
        : this(App.GetService<ViewModel_Dunnage_ImagePartSearchDialog>()) { }

    
    private async void PartGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not Model_DunnagePart part)
        {
            return;
        }

        await ViewModel.SelectPartCommand.ExecuteAsync(part);
    }

    private async void OnShowPartsWithoutImagesToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            await ViewModel.HandleShowPartsWithoutImagesChangedAsync(toggleSwitch.IsOn);
        }
    }
}
