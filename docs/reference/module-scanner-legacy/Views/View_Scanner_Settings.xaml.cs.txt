using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Placeholder scanner settings page.
/// </summary>
public sealed partial class View_Scanner_Settings : Page
{
    private readonly IService_AdaptiveLayout _adaptiveLayout;

    public ViewModel_Scanner_Settings ViewModel { get; }

    public View_Scanner_Settings(
        ViewModel_Scanner_Settings viewModel,
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
        SettingsLayoutGrid.Padding = _adaptiveLayout.GetScannerContentPadding(ActualWidth);

        var state = _adaptiveLayout.ResolveScannerLayoutState(ActualWidth);
        _ = VisualStateManager.GoToState(this, state, false);

        var profilesBoundedHeight = _adaptiveLayout.CalculateBoundedViewportHeight(
            containerHeightEpx: SettingsProfilesPanelGrid.ActualHeight,
            occupiedHeightsEpx:
            [
                ProfilesHeaderTextBlock.ActualHeight,
                ProfilesActionButtonsPanel.ActualHeight,
                ProfilesLoadButton.ActualHeight,
            ]
        );
        if (profilesBoundedHeight > 0)
        {
            ProfilesListView.MaxHeight = profilesBoundedHeight;
        }

        var editorBoundedHeight = _adaptiveLayout.CalculateBoundedViewportHeight(
            containerHeightEpx: SettingsLayoutGrid.ActualHeight
        );
        if (editorBoundedHeight > 0)
        {
            SettingsEditorScrollViewer.MaxHeight = editorBoundedHeight;
        }
    }
}