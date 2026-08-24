using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using MTM_Receiving_Application.Module_Shared.Models.Lookup;
using MTM_Receiving_Application.Module_Shared.Views.Controls;

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
        ViewModel.FromLocationInventoryPickerRequested += OnFromLocationInventoryPickerRequestedAsync;
        ViewModel.PartIdFocusRequested += FocusPartIdTextBox;
        ViewModel.Activate();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.FromLocationInventoryPickerRequested -= OnFromLocationInventoryPickerRequestedAsync;
        ViewModel.PartIdFocusRequested -= FocusPartIdTextBox;
        ViewModel.Deactivate();
    }

    private void FocusPartIdTextBox()
    {
        PartIdLookupControl?.FocusInput();
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

            ApplyPartPaddingRulesToLookupControl();
            _paddingSettingsLoaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ScannerWorkbench] Failed to load part padding settings: {ex.Message}");
            _isPaddingEnabled = false;
            _paddingRules = [];
            ApplyPartPaddingRulesToLookupControl();
            _paddingSettingsLoaded = true;
        }
    }

    /// <summary>
    /// Feeds the loaded part-number padding rules into the shared lookup control so it applies
    /// the same prefix padding before running exact-match/fuzzy validation.
    /// </summary>
    private void ApplyPartPaddingRulesToLookupControl()
    {
        if (PartIdLookupControl is null)
        {
            return;
        }

        if (!_isPaddingEnabled || _paddingRules.Count == 0)
        {
            PartIdLookupControl.PrefixPaddingRules = Array.Empty<Model_SharedLookupPrefixPaddingRule>();
            return;
        }

        PartIdLookupControl.PrefixPaddingRules = _paddingRules
            .Where(rule => rule.IsEnabled)
            .Select(rule => new Model_SharedLookupPrefixPaddingRule
            {
                Prefix = rule.Prefix,
                MaxLength = rule.MaxLength,
                PadCharacter = rule.PadChar,
                IsEnabled = rule.IsEnabled,
            })
            .ToArray();
    }

    /// <summary>
    /// Runs after the shared lookup control validates the typed part. When the part has no exact
    /// match but fuzzy candidates exist, prompts the operator with the fuzzy-search picker and
    /// commits the chosen part into the NewPartId field (mirrors Receiving part entry).
    /// </summary>
    private async void PartIdLookupControl_ValidationCompleted(
        object sender,
        Model_SharedLookupValidationCompletedEventArgs e
    )
    {
        if (!_paddingSettingsLoaded)
        {
            await LoadPaddingSettingsAsync();
            await PartIdLookupControl.ValidateAsync();
            return;
        }

        if (!e.Result.IsValid)
        {
            ViewModel.ShowStatus(
                e.Result.Message,
                MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity.Warning
            );
            return;
        }

        if (e.Result.UsedFuzzyFallback && e.Result.HasExactMatch is false)
        {
            var selectedResult = await ShowPartFuzzyPickerAsync(
                e.Result.FormattedValue,
                e.Result.FuzzyCandidates
            );

            if (selectedResult is null)
            {
                PartIdLookupControl.InputValue = string.Empty;
                ViewModel.NewPartId = string.Empty;
                return;
            }

            var selectedValue = (selectedResult.Key ?? selectedResult.Label ?? string.Empty)
                .Trim();
            if (string.IsNullOrWhiteSpace(selectedValue))
            {
                PartIdLookupControl.InputValue = string.Empty;
                ViewModel.NewPartId = string.Empty;
                return;
            }

            PartIdLookupControl.InputValue = selectedValue;
            ViewModel.NewPartId = selectedValue;
        }
    }

    private async Task<Model_FuzzySearchResult?> ShowPartFuzzyPickerAsync(
        string searchTerm,
        IReadOnlyList<Model_FuzzySearchResult> items
    )
    {
        if (items.Count == 0)
        {
            return null;
        }

        var xamlRoot = XamlRoot;
        if (xamlRoot is null)
        {
            return null;
        }

        var dialog = new Dialog_FuzzySearchPicker(
            items,
            "Select Part",
            $"No exact part match was found for '{searchTerm}'. Select a similar part to continue."
        )
        {
            XamlRoot = xamlRoot,
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return null;
        }

        return dialog.SelectedResult;
    }

    private async void FromLocationTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        ViewModel.NewFromLocation = textBox.Text;
        await HandleFromLocationTextBoxLostFocusAsync(textBox);
    }

    private async Task HandleFromLocationTextBoxLostFocusAsync(TextBox textBox)
    {
        if (_isLocationPickerOpen)
        {
            return;
        }

        var rawValue = ViewModel.NewFromLocation?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            ViewModel.ClearFromQuantityLimit();
            return;
        }

        // Any new From validation resets the quantity limit until a valid source is resolved.
        ViewModel.ClearFromQuantityLimit();

        // 1) Autocomplete first: apply the shared dash-formatting rule (e.g. "VA101" ->
        // "V-A1-01") and write the canonical value back into the field.
        var formatted = ViewModel.FormatLocation(rawValue);
        if (string.Equals(formatted, rawValue, StringComparison.OrdinalIgnoreCase) is false)
        {
            ViewModel.NewFromLocation = formatted;
            textBox.Text = formatted;
        }

        var currentValue = ViewModel.NewFromLocation?.Trim() ?? string.Empty;

        // 2) Ensure the location exists in the system.
        var validation = await ViewModel.ValidateFromLocationAsync();
        if (validation.IsValid)
        {
            // 3) The location resolved cleanly — validate the quantity against the canonical
            // location the validation service returned (e.g. "VA101" for the display-formatted
            // "V-A1-01"). Matching against the display form fails because the stock rows use
            // the canonical DB location id.
            var canonicalFromLocation = ViewModel.NewFromLocation?.Trim() ?? string.Empty;
            await ValidateFromQuantityAsync(
                textBox,
                string.IsNullOrWhiteSpace(canonicalFromLocation)
                    ? currentValue
                    : canonicalFromLocation
            );
            return;
        }

        // The location is not in the system. Without a part there is nothing to resolve
        // against stock, so fall back to the generic fuzzy-location behavior.
        if (string.IsNullOrWhiteSpace(ViewModel.NewPartId))
        {
            await HandleLocationTextBoxLostFocusAsync(
                textBox,
                ViewModel.ValidateFromLocationAsync,
                ViewModel.GetFromLocationSuggestionsAsync,
                value => ViewModel.NewFromLocation = value,
                () => ViewModel.NewFromLocation
            );
            return;
        }

        // Fuzzy-search for a matching location, then validate the quantity on the selection.
        var fuzzyResult = await ViewModel.GetFromLocationSuggestionsAsync();
        if (fuzzyResult.IsSuccess && fuzzyResult.Data?.Count > 0)
        {
            _isLocationPickerOpen = true;
            try
            {
                var dialog = new Dialog_FuzzySearchPicker(
                    fuzzyResult.Data,
                    "Select Location",
                    $"'{currentValue}' was not found in the system. Select a matching location:"
                )
                {
                    XamlRoot = XamlRoot,
                };

                var dialogResult = await dialog.ShowAsync();
                if (
                    dialogResult == ContentDialogResult.Primary
                    && dialog.SelectedResult is not null
                    && string.IsNullOrWhiteSpace(dialog.SelectedResult.Label) is false
                )
                {
                    var picked = dialog.SelectedResult.Label.Trim();
                    ViewModel.NewFromLocation = picked;
                    textBox.Text = picked;

                    await ValidateFromQuantityAsync(textBox, picked);
                    return;
                }
            }
            finally
            {
                _isLocationPickerOpen = false;
            }
        }

        ViewModel.ShowStatus(
            validation.Message,
            Module_Core.Models.Enums.InfoBarSeverity.Warning
        );
    }

    /// <summary>
    /// Validates that the resolved From location can fulfill the requested quantity. When it
    /// cannot, opens the inventory picker (locations holding stock) so the operator can choose
    /// a source that has enough on hand.
    /// </summary>
    private async Task ValidateFromQuantityAsync(TextBox textBox, string fromLocation)
    {
        var stockResult = await ViewModel.GetFromInventoryLocationsAsync();
        if (!stockResult.Success || stockResult.Data is null)
        {
            return; // Stock check is unavailable; do not block.
        }

        if (stockResult.Data.Count == 0)
        {
            ViewModel.ClearFromQuantityLimit();
            ViewModel.ShowStatus(
                $"No stock was found for {ViewModel.NewPartId} in warehouse {ViewModel.NewFromWarehouse}.",
                Module_Core.Models.Enums.InfoBarSeverity.Warning
            );
            return;
        }

        var source = stockResult.Data.FirstOrDefault(row =>
            string.Equals(row.LocationId, fromLocation, StringComparison.OrdinalIgnoreCase)
        );
        if (source is null)
        {
            ViewModel.ClearFromQuantityLimit();
            ViewModel.ShowStatus(
                $"No stock was found at {fromLocation} for {ViewModel.NewPartId}.",
                Module_Core.Models.Enums.InfoBarSeverity.Warning
            );
            return;
        }

        // The From location is valid — enable the Qty field and cap it at the on-hand qty.
        ViewModel.SetFromQuantityLimit(source.Quantity);

        if (!TryGetRequestedQuantity(out var requested))
        {
            return; // No quantity entered yet; the limit is known for when the operator types one.
        }

        if (requested <= source.Quantity)
        {
            return; // The source can fulfill the entered quantity.
        }

        // The entered quantity exceeds the source's stock — offer locations that hold stock.
        var picked = await ShowFromLocationInventoryPickerAsync(stockResult.Data, fromLocation);
        if (string.IsNullOrWhiteSpace(picked) is false)
        {
            ViewModel.NewFromLocation = picked!;
            textBox.Text = picked!;
            var newSource = stockResult.Data.FirstOrDefault(row =>
                string.Equals(row.LocationId, picked, StringComparison.OrdinalIgnoreCase)
            );
            if (newSource is not null)
            {
                ViewModel.SetFromQuantityLimit(newSource.Quantity);
            }
            return;
        }

        ViewModel.ShowStatus(
            $"Requested {ViewModel.NewQuantity} exceeds available stock at {fromLocation}.",
            Module_Core.Models.Enums.InfoBarSeverity.Warning
        );
    }

    /// <summary>
    /// Parses the entered quantity as a positive decimal. Returns false when none is set.
    /// </summary>
    private bool TryGetRequestedQuantity(out decimal quantity)
    {
        return decimal.TryParse(
                ViewModel.NewQuantity,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out quantity
            )
            && quantity > 0;
    }

    /// <summary>
    /// Shows the source-location inventory picker built from the given in-stock rows and
    /// returns the selected location, or null when the operator cancels.
    /// </summary>
    private async Task<string?> ShowFromLocationInventoryPickerAsync(
        IReadOnlyList<Model_InforVisualMaterialLocationRow> stockRows,
        string currentValue
    )
    {
        _isLocationPickerOpen = true;
        try
        {
            var matches = stockRows
                .Select(row => new Model_FuzzySearchResult
                {
                    Key = row.LocationId,
                    Label = row.LocationId,
                    Detail = $"On hand: {row.Quantity.ToString(CultureInfo.InvariantCulture)}",
                })
                .ToList();

            var dialog = new Dialog_FuzzySearchPicker(
                matches,
                "Select Source Location",
                $"'{currentValue}' cannot supply the requested quantity for {ViewModel.NewPartId}. Select the location holding stock:"
            )
            {
                XamlRoot = XamlRoot,
            };

            var dialogResult = await dialog.ShowAsync();
            if (
                dialogResult == ContentDialogResult.Primary
                && dialog.SelectedResult is not null
                && string.IsNullOrWhiteSpace(dialog.SelectedResult.Label) is false
            )
            {
                return dialog.SelectedResult.Label.Trim();
            }
        }
        finally
        {
            _isLocationPickerOpen = false;
        }

        return null;
    }

    /// <summary>
    /// View-side handler for <see cref="ViewModel_Scanner_Workbench.FromLocationInventoryPickerRequested"/>:
    /// queries the in-stock locations for the part and shows the picker.
    /// </summary>
    private async Task<string?> OnFromLocationInventoryPickerRequestedAsync(
        string partId,
        string fromWarehouse,
        string currentValue
    )
    {
        var stockResult = await ViewModel.GetFromInventoryLocationsAsync();
        if (!stockResult.Success || stockResult.Data is null)
        {
            return null;
        }

        if (stockResult.Data.Count == 0)
        {
            ViewModel.ShowStatus(
                $"No stock was found for {partId} in warehouse {fromWarehouse}.",
                Module_Core.Models.Enums.InfoBarSeverity.Warning
            );
            return null;
        }

        return await ShowFromLocationInventoryPickerAsync(stockResult.Data, currentValue);
    }

    private async void ToLocationTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        ViewModel.NewToLocation = textBox.Text;
        await HandleLocationTextBoxLostFocusAsync(
            textBox,
            ViewModel.ValidateToLocationAsync,
            ViewModel.GetToLocationSuggestionsAsync,
            value => ViewModel.NewToLocation = value,
            () => ViewModel.NewToLocation
        );
    }

    /// <summary>
    /// Rejects quantity input that exceeds the available stock at the validated From location.
    /// </summary>
    private void QuantityTextBox_BeforeTextChanging(
        TextBox sender,
        TextBoxBeforeTextChangingEventArgs args
    )
    {
        if (!ViewModel.IsQuantityEnabled || ViewModel.MaxQuantity is null)
        {
            return;
        }

        if (
            decimal.TryParse(
                args.NewText,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var entered
            )
            && entered > ViewModel.MaxQuantity.Value
        )
        {
            args.Cancel = true;
        }
    }

    private async Task HandleLocationTextBoxLostFocusAsync(
        TextBox textBox,
        Func<Task<Model_ScannerLocationValidationResult>> validateAsync,
        Func<Task<Model_Dao_Result<List<Model_FuzzySearchResult>>>> suggestionAsync,
        Action<string> setLocation,
        Func<string?> getLocation
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

        // Autocomplete first: apply the shared dash-formatting rule (e.g. "VA101" ->
        // "V-A1-01") and write the canonical value back into the field.
        var formatted = ViewModel.FormatLocation(currentValue);
        if (string.Equals(formatted, currentValue, StringComparison.OrdinalIgnoreCase) is false)
        {
            setLocation(formatted);
            textBox.Text = formatted;
            currentValue = formatted;
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