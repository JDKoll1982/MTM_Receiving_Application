using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Placeholder scanner workbench page.
/// </summary>
public sealed partial class View_Scanner_Workbench : Page
{
    private readonly IService_AdaptiveLayout _adaptiveLayout;
    private readonly IService_ReceivingSettings _receivingSettings;
    private List<Model_PartNumberPrefixRule> _paddingRules = [];
    private bool _isPaddingEnabled;
    private bool _paddingSettingsLoaded;

    public ViewModel_Scanner_Workbench ViewModel { get; }

    public View_Scanner_Workbench(
        ViewModel_Scanner_Workbench viewModel,
        IService_AdaptiveLayout adaptiveLayout,
        IService_ReceivingSettings receivingSettings
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(adaptiveLayout);
        ArgumentNullException.ThrowIfNull(receivingSettings);
        ViewModel = viewModel;
        _adaptiveLayout = adaptiveLayout;
        _receivingSettings = receivingSettings;
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
        _ = LoadPaddingSettingsAsync();
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
        WorkbenchContentGrid.Padding = _adaptiveLayout.GetScannerContentPadding(ActualWidth);

        var state = _adaptiveLayout.ResolveScannerLayoutState(ActualWidth);
        _ = VisualStateManager.GoToState(this, state, false);

        var boundedHeight = _adaptiveLayout.CalculateBoundedViewportHeight(
            containerHeightEpx: ItemsCardContentGrid.ActualHeight,
            occupiedHeightsEpx:
            [
                ItemsHeaderGrid.ActualHeight,
                ItemEntryGrid.ActualHeight,
            ]
        );

        if (boundedHeight > 0)
        {
            SessionItemsListView.MaxHeight = boundedHeight;
        }
    }

    private async Task LoadPaddingSettingsAsync()
    {
        try
        {
            _isPaddingEnabled = await _receivingSettings.GetBoolAsync(
                ReceivingSettingsKeys.PartNumberPadding.Enabled
            );
            var rulesJson = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.PartNumberPadding.RulesJson
            );

            if (!string.IsNullOrWhiteSpace(rulesJson))
            {
                var rules = JsonSerializer.Deserialize<Model_PartNumberPrefixRule[]>(rulesJson);
                _paddingRules = rules?.Where(static rule => rule is not null).ToList() ?? [];
            }

            _paddingSettingsLoaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ScannerWorkbench] Failed to load part padding settings: {ex.Message}");
            _isPaddingEnabled = false;
            _paddingRules = [];
            _paddingSettingsLoaded = true;
        }
    }

    private async void PartTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        if (!_paddingSettingsLoaded)
        {
            await LoadPaddingSettingsAsync();
        }

        var value = textBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            ViewModel.NewPartId = string.Empty;
            return;
        }

        var formattedPartId = ApplyPartNumberPadding(value);
        if (string.Equals(formattedPartId, value, StringComparison.Ordinal))
        {
            return;
        }

        ViewModel.NewPartId = formattedPartId;
        textBox.Text = formattedPartId;
    }

    private string ApplyPartNumberPadding(string input)
    {
        if (!_paddingSettingsLoaded || !_isPaddingEnabled || _paddingRules.Count == 0)
        {
            return input;
        }

        return Model_PartNumberPrefixRule.ApplyBestMatchingRule(_paddingRules.ToArray(), input);
    }
}