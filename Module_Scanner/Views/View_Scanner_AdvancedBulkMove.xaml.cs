using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Full-page Advanced bulk-move wizard hosted in the scanner module. Step 1 searches a From
/// Location (fuzzy location suggestions + search button) and lists the parts with stock there;
/// Step 2 collects destination locations and quantities before handing rows back to the
/// workbench.
/// </summary>
public sealed partial class View_Scanner_AdvancedBulkMove : Page
{
    private readonly IService_AdaptiveLayout _adaptiveLayout;

    public ViewModel_Scanner_AdvancedBulkMove ViewModel { get; }

    public View_Scanner_AdvancedBulkMove(
        ViewModel_Scanner_AdvancedBulkMove viewModel,
        IService_AdaptiveLayout adaptiveLayout
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(adaptiveLayout);

        ViewModel = viewModel;
        _adaptiveLayout = adaptiveLayout;
        InitializeComponent();
        DataContext = ViewModel;

        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyAdaptiveLayout();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyAdaptiveLayout();
    }

    private void ApplyAdaptiveLayout()
    {
        AdvancedBulkMoveRootGrid.Margin = _adaptiveLayout.GetScannerContentPadding(ActualWidth);
    }

    private async void FromLocationSearchBox_TextChanged(
        AutoSuggestBox sender,
        AutoSuggestBoxTextChangedEventArgs args
    )
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            await ViewModel.UpdateLocationSuggestionsAsync(sender.Text);
        }
    }

    private void FromLocationSearchBox_SuggestionChosen(
        AutoSuggestBox sender,
        AutoSuggestBoxSuggestionChosenEventArgs args
    )
    {
        if (args.SelectedItem is Model_FuzzySearchResult suggestion)
        {
            ViewModel.FromLocation = suggestion.Key;
        }
    }

    private void FromLocationSearchBox_QuerySubmitted(
        AutoSuggestBox sender,
        AutoSuggestBoxQuerySubmittedEventArgs args
    )
    {
        ViewModel.SearchPartsCommand.Execute(null);
    }
}
