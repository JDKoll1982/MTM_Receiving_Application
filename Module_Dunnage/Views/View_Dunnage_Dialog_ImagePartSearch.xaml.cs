using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_Dialog_ImagePartSearch : ContentDialog
{
    public ViewModel_Dunnage_ImagePartSearchDialog ViewModel { get; }

    public View_Dunnage_Dialog_ImagePartSearch(ViewModel_Dunnage_ImagePartSearchDialog viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        InitializeComponent();
        Title = ViewModel.Heading;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadPartsCommand.ExecuteAsync(null);
    }

    private async void PartGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not Model_DunnagePart part)
        {
            return;
        }

        var dialog = App.GetService<View_Dunnage_Dialog_PartInfoModal>();
        if (dialog is null)
        {
            return;
        }

        await dialog.ViewModel.InitializeAsync(part);
        dialog.XamlRoot = XamlRoot;
        await dialog.ShowAsync();
    }
}
