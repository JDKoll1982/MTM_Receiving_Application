using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_PartSelectionView : UserControl
{
    public ViewModel_Dunnage_PartSelection ViewModel { get; }

    /// <summary>
    /// Raised when the operator double-taps a part card that can be picked. The workflow page owns
    /// the "go to the next step" logic, so this view only reports the intent.
    /// </summary>
    public event EventHandler? PartCardActivated;

    private readonly IService_Focus _focusService;

    public View_Dunnage_PartSelectionView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_PartSelection>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();

        _focusService.AttachFocusOnVisibility(this, PartNumberComboBox);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Dunnage_PartSelectionView: OnLoaded called");
        await ViewModel.InitializeAsync();
    }

    private async void OnDisplayFormatPreferenceToggled(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch toggleSwitch)
        {
            await ViewModel.HandleDisplayFormatChangedAsync(toggleSwitch.IsOn);
        }
    }

    /// <summary>
    /// A double-tap on a card means "this is the one, carry on". It only advances when the card
    /// actually holds a part, because the Next button is gated on the same selection.
    /// </summary>
    private void OnPartCardDoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (
            e.OriginalSource is not DependencyObject source
            || FindPartFromElement(source) is not { } part
        )
        {
            return;
        }

        ViewModel.SelectedPart = part;
        if (!ViewModel.IsPartSelected)
        {
            return;
        }

        e.Handled = true;
        PartCardActivated?.Invoke(this, EventArgs.Empty);
    }

    private static Model_DunnagePart? FindPartFromElement(DependencyObject element)
    {
        var current = element;
        while (current is not null)
        {
            if (current is GridViewItem { Content: Model_DunnagePart part })
            {
                return part;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
