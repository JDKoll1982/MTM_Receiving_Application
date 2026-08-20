using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Scanner.Models;
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
    private bool _isLocationPickerOpen;

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
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
        _ = LoadPaddingSettingsAsync();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyAdaptiveLayout();
        ViewModel.Activate();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
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

    private async void FromLocationTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        await HandleLocationTextBoxLostFocusAsync(
            textBox,
            ViewModel.ValidateFromLocationAsync,
            ViewModel.GetFromLocationSuggestionsAsync,
            value => ViewModel.NewFromLocation = value,
            () => ViewModel.NewFromLocation
        );
    }

    private async void ToLocationTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        await HandleLocationTextBoxLostFocusAsync(
            textBox,
            ViewModel.ValidateToLocationAsync,
            ViewModel.GetToLocationSuggestionsAsync,
            value => ViewModel.NewToLocation = value,
            () => ViewModel.NewToLocation
        );
    }

    private async Task HandleLocationTextBoxLostFocusAsync(
        TextBox textBox,
        Func<Task<Model_ScannerLocationValidationResult>> validateAsync,
        Func<Task<Model_Dao_Result<List<Model_FuzzySearchResult>>>> suggestionAsync,
        Action<string> setLocation,
        Func<string> getLocation
    )
    {
        if (_isLocationPickerOpen)
        {
            return;
        }

        var currentValue = getLocation()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(currentValue))
        {
            return;
        }

        var validation = await validateAsync();
        if (validation.IsValid)
        {
            return;
        }

        var suggestionsResult = await suggestionAsync();
        if (suggestionsResult.IsSuccess && suggestionsResult.Data?.Count > 0)
        {
            _isLocationPickerOpen = true;
            try
            {
                var dialog = new Dialog_FuzzySearchPicker(
                    suggestionsResult.Data,
                    "Select Location",
                    $"No exact match was found for '{currentValue}'. Select a matching location."
                )
                {
                    XamlRoot = textBox.XamlRoot,
                };

                var dialogResult = await dialog.ShowAsync();
                if (
                    dialogResult == ContentDialogResult.Primary
                    && dialog.SelectedResult is not null
                    && string.IsNullOrWhiteSpace(dialog.SelectedResult.Label) is false
                )
                {
                    setLocation(dialog.SelectedResult.Label.Trim());
                    return;
                }
            }
            finally
            {
                _isLocationPickerOpen = false;
            }
        }

        var statusMessage = validation.Message;
        if (!suggestionsResult.IsSuccess && string.IsNullOrWhiteSpace(suggestionsResult.ErrorMessage) is false)
        {
            statusMessage = $"{validation.Message} {suggestionsResult.ErrorMessage}";
        }

        ViewModel.ShowStatus(
            statusMessage,
            Module_Core.Models.Enums.InfoBarSeverity.Warning
        );
    }

    private async void RemoveSelectedItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedSessionItem is null)
        {
            return;
        }

        var xamlRoot = (sender as FrameworkElement)?.XamlRoot ?? XamlRoot;
        if (xamlRoot is null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = xamlRoot,
            Title = "Remove selected item?",
            Content = "This will remove the selected item from the list.",
            PrimaryButtonText = "Remove",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
        };

        Helper_UI_ContentDialogTheme.ApplyTheme(dialog, xamlRoot);

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.RemoveSelectedSessionItemCommand.ExecuteAsync(null);
        }
    }
}