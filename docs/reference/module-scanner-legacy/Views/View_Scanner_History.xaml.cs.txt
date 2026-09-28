using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Placeholder scanner history page.
/// </summary>
public sealed partial class View_Scanner_History : Page
{
    private readonly IService_AdaptiveLayout _adaptiveLayout;

    public ViewModel_Scanner_History ViewModel { get; }

    public View_Scanner_History(
        ViewModel_Scanner_History viewModel,
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
        HistoryContentGrid.Padding = _adaptiveLayout.GetScannerContentPadding(ActualWidth);

        var state = _adaptiveLayout.ResolveScannerLayoutState(ActualWidth);
        _ = VisualStateManager.GoToState(this, state, false);

        var runsBoundedHeight = _adaptiveLayout.CalculateBoundedViewportHeight(
            containerHeightEpx: HistoryTopSplitGrid.ActualHeight,
            occupiedHeightsEpx:
            [
                HistoryRunsHeaderTextBlock.ActualHeight,
            ]
        );
        if (runsBoundedHeight > 0)
        {
            RunHistoryListView.MaxHeight = runsBoundedHeight;
        }

        var resultsBoundedHeight = _adaptiveLayout.CalculateBoundedViewportHeight(
            containerHeightEpx: ItemResultsPanelGrid.ActualHeight,
            occupiedHeightsEpx:
            [
                ItemResultsHeaderTextBlock.ActualHeight,
            ]
        );
        if (resultsBoundedHeight > 0)
        {
            ItemResultsListView.MaxHeight = resultsBoundedHeight;
        }
    }
}