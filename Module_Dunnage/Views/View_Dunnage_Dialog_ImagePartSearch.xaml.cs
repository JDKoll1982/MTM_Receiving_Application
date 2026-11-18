using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
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
        if (
            e.ClickedItem is not Model_DunnagePart part
            || IsShiftKeyDown()
        )
        {
            // Shift+click opens the full image instead of picking the part.
            return;
        }

        await ViewModel.SelectPartCommand.ExecuteAsync(part);
    }

    private static bool IsShiftKeyDown()
    {
        return Microsoft
            .UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(
                Windows.System.VirtualKey.Shift
            )
            .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
    }

    private async void OnShowPartsWithoutImagesToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            await ViewModel.HandleShowPartsWithoutImagesChangedAsync(toggleSwitch.IsOn);
        }
    }

    /// <summary>
    /// Shift+click on a card image opens the full-size picture. A plain click still selects the
    /// part, so the card keeps its normal pick behavior.
    /// </summary>
    private void OnPartCardImagePointerPressed(
        object sender,
        Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e
    )
    {
        if (
            e.KeyModifiers.HasFlag(Windows.System.VirtualKeyModifiers.Shift) is false
            || sender is not FrameworkElement { DataContext: Model_DunnagePart part }
        )
        {
            return;
        }

        e.Handled = true;

        Helper_UI_ImageLightbox.Show(
            Helper_DunnageImagePaths.GetDisplayAbsolutePath(
                string.IsNullOrWhiteSpace(part.ImagePath)
                    ? part.DunnageTypeImagePath
                    : part.ImagePath
            )
        );
    }
}
