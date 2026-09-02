using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.ViewModels;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Workbench view code-behind. Keeps business logic in the ViewModel; this file handles
/// focus/selection, the Step 6b-a1-a stock-location modal, and loading the part-number
/// padding rules into the shared part lookup control (Part Padding feature).
/// </summary>
public sealed partial class View_Scanner_Workbench : Page
{
    private readonly IService_ReceivingSettings _receivingSettings;

    private bool _isPickerOpen;

    private bool _isPaddingEnabled;
    private List<Model_PartNumberPrefixRule> _paddingRules = [];

    public ViewModel_Scanner_Workbench ViewModel { get; }

    public View_Scanner_Workbench(
        ViewModel_Scanner_Workbench viewModel,
        IService_ReceivingSettings receivingSettings
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(receivingSettings);
        ViewModel = viewModel;
        _receivingSettings = receivingSettings;
        InitializeComponent();
        DataContext = ViewModel;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        // Load the Part Padding rules up front so prefix padding is the first step applied
        // to the part field on focus-loss (before exact-match/fuzzy validation).
        _ = LoadPaddingSettingsAsync();
    }

    /// <summary>
    /// Reads the Part Padding settings (Enabled + RulesJson) and feeds them into the shared
    /// part lookup control so prefix padding is applied before exact-match/fuzzy validation.
    /// </summary>
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
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ScannerWorkbench] Failed to load part padding settings: {ex.Message}");
            _isPaddingEnabled = false;
            _paddingRules = [];
            ApplyPartPaddingRulesToLookupControl();
        }
    }

    /// <summary>
    /// Maps the loaded padding rules onto the shared lookup control. Empty when padding is
    /// disabled or no valid rules apply, so the control runs with its default behavior.
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
            .Where(rule => rule.IsEnabled && !string.IsNullOrWhiteSpace(rule.Prefix) && rule.MaxLength > 0)
            .Select(rule => new Model_SharedLookupPrefixPaddingRule
            {
                Prefix = rule.Prefix.Trim(),
                MaxLength = rule.MaxLength,
                PadCharacter = rule.PadChar,
                IsEnabled = rule.IsEnabled,
            })
            .ToArray();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.FromLocationInventoryPickerRequested += OnFromLocationInventoryPickerRequestedAsync;
        ViewModel.LocationPartsPickerRequested += OnLocationPartsPickerRequestedAsync;
        ViewModel.PartIdFocusRequested += OnPartIdFocusRequested;
        ViewModel.PartIdClearRequested += OnPartIdClearRequested;
        ViewModel.FirstRowToCellFocusRequested += OnFirstRowToCellFocusRequested;
        ViewModel.DuplicateAddConfirmationRequested += OnDuplicateAddConfirmationRequestedAsync;

        ViewModel.Activate();
        await ViewModel.EnsureCurrentSessionAsync();

        // Edge-case fix: rows resumed as "valid" after an app restart have no in-memory
        // on-hand guard (MaxQuantity is not persisted). Revalidate them once against current
        // stock in the background to restore the guard.
        if (ViewModel.SessionItems.Count > 0)
        {
            _ = ViewModel.RevalidateResumedValidItemsAsync();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.FromLocationInventoryPickerRequested -= OnFromLocationInventoryPickerRequestedAsync;
        ViewModel.LocationPartsPickerRequested -= OnLocationPartsPickerRequestedAsync;
        ViewModel.PartIdFocusRequested -= OnPartIdFocusRequested;
        ViewModel.PartIdClearRequested -= OnPartIdClearRequested;
        ViewModel.FirstRowToCellFocusRequested -= OnFirstRowToCellFocusRequested;
        ViewModel.DuplicateAddConfirmationRequested -= OnDuplicateAddConfirmationRequestedAsync;
        ViewModel.Deactivate();
    }

    private async Task<bool> OnDuplicateAddConfirmationRequestedAsync(string message)
    {
        var xamlRoot = XamlRoot ?? this.XamlRoot;
        if (xamlRoot is null)
        {
            return true;
        }

        var dialog = new ContentDialog
        {
            Title = "Add duplicate lines?",
            Content = message,
            PrimaryButtonText = "Add Anyway",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = xamlRoot,
        };
        Helper_UI_ContentDialogTheme.ApplyTheme(dialog, xamlRoot);
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    // ── Step 6b-a1-a: stock-location modal ───────────────────────────────────────

    private async Task<IReadOnlyList<Model_ScannerStockPick>> OnFromLocationInventoryPickerRequestedAsync(
        string partId,
        string warehouseCode,
        string currentLocation
    )
    {
        if (_isPickerOpen)
        {
            return [];
        }

        _isPickerOpen = true;
        try
        {
            var rowsResult = await ViewModel.GetFromInventoryLocationsAsync();
            if (!rowsResult.Success || rowsResult.Data is null || rowsResult.Data.Count == 0)
            {
                return [];
            }

            return await ShowStockPickerDialogAsync(partId, warehouseCode, rowsResult.Data);
        }
        finally
        {
            _isPickerOpen = false;
        }
    }

    private async Task<IReadOnlyList<Model_ScannerStockPick>> OnLocationPartsPickerRequestedAsync(
        string location,
        string warehouseCode
    )
    {
        if (_isPickerOpen)
        {
            return [];
        }

        _isPickerOpen = true;
        try
        {
            var partsResult = await ViewModel.GetPartsAtLocationForPickerAsync(location, warehouseCode);
            if (!partsResult.Success || partsResult.Data is null || partsResult.Data.Count == 0)
            {
                return [];
            }

            return await ShowLocationPartsPickerDialogAsync(location, warehouseCode, partsResult.Data);
        }
        finally
        {
            _isPickerOpen = false;
        }
    }

    private async Task<IReadOnlyList<Model_ScannerStockPick>> ShowStockPickerDialogAsync(
        string partId,
        string warehouseCode,
        IReadOnlyList<Model_InforVisualMaterialLocationRow> rows
    )
    {
        var xamlRoot = XamlRoot ?? this.XamlRoot;
        if (xamlRoot is null)
        {
            return [];
        }

        var listView = new ListView
        {
            SelectionMode = ListViewSelectionMode.None,
            MaxHeight = 360,
        };

        var pickRows = rows.Select(row => new StockPickRow
        {
            LocationId = row.LocationId,
            WarehouseCode = row.WarehouseCode,
            OnHand = row.Quantity,
            OnHandDisplay = FormatDecimal(row.Quantity),
            Quantity = FormatDecimal(row.Quantity),
            TransactionCount = 1,
        }).ToList();

        listView.ItemsSource = pickRows;
        listView.ItemTemplate = BuildStockPickTemplate();

        var (panel, selectAllButton, pickerErrorText) = BuildPickerContent(listView, "Location");
        WireSelectAll(selectAllButton, pickRows);

        var dialog = new ContentDialog
        {
            Title = $"Stock locations for {partId}",
            Content = panel,
            PrimaryButtonText = "Use Selected",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
        };
        ApplyPickerDialogSizing(dialog);
        dialog.PrimaryButtonClick += (_, clickArgs) =>
        {
            var message = ValidatePickRowsForSubmit(pickRows);
            if (message is not null)
            {
                clickArgs.Cancel = true;
                pickerErrorText.Text = message;
                pickerErrorText.Visibility = Visibility.Visible;
            }
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return [];
        }

        return pickRows
            .Where(row => row.IsChecked)
            .Select(row => new Model_ScannerStockPick
            {
                Location = row.LocationId,
                OnHand = row.OnHand,
                Quantity = ClampQuantity(row.Quantity, row.OnHand),
                TransactionCount = Math.Max(1, row.TransactionCount),
            })
            .ToList();
    }

    private async Task<IReadOnlyList<Model_ScannerStockPick>> ShowLocationPartsPickerDialogAsync(
        string location,
        string warehouseCode,
        IReadOnlyList<Model_InforVisualMaterialLocationRow> rows
    )
    {
        var xamlRoot = XamlRoot ?? this.XamlRoot;
        if (xamlRoot is null)
        {
            return [];
        }

        var listView = new ListView
        {
            SelectionMode = ListViewSelectionMode.None,
            MaxHeight = 360,
        };

        var pickRows = rows.Select(row => new StockPickRow
        {
            PartId = row.PartId,
            WarehouseCode = row.WarehouseCode,
            OnHand = row.Quantity,
            OnHandDisplay = FormatDecimal(row.Quantity),
            Quantity = FormatDecimal(row.Quantity),
            TransactionCount = 1,
        }).ToList();

        listView.ItemsSource = pickRows;
        listView.ItemTemplate = BuildStockPickTemplate();

        var (panel, selectAllButton, pickerErrorText) = BuildPickerContent(listView, "Part");
        WireSelectAll(selectAllButton, pickRows);

        var dialog = new ContentDialog
        {
            Title = $"Parts at {location}",
            Content = panel,
            PrimaryButtonText = "Use Selected",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
        };
        ApplyPickerDialogSizing(dialog);
        dialog.PrimaryButtonClick += (_, clickArgs) =>
        {
            var message = ValidatePickRowsForSubmit(pickRows);
            if (message is not null)
            {
                clickArgs.Cancel = true;
                pickerErrorText.Text = message;
                pickerErrorText.Visibility = Visibility.Visible;
            }
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return [];
        }

        return pickRows
            .Where(row => row.IsChecked)
            .Select(row => new Model_ScannerStockPick
            {
                PartId = row.PartId,
                OnHand = row.OnHand,
                Quantity = ClampQuantity(row.Quantity, row.OnHand),
                TransactionCount = Math.Max(1, row.TransactionCount),
            })
            .ToList();
    }

    private static void WireSelectAll(Button selectAllButton, IReadOnlyList<StockPickRow> pickRows)
    {
        var allSelected = false;
        selectAllButton.Click += (_, _) =>
        {
            allSelected = !allSelected;
            foreach (var row in pickRows)
            {
                row.IsChecked = allSelected;
            }

            selectAllButton.Content = allSelected ? "Select None" : "Select All";
        };
    }

    /// <summary>Formats a decimal without trailing zeros (16452.0000000 -> 16452).</summary>
    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.########", CultureInfo.InvariantCulture);
    }

    private static string ClampQuantity(string? raw, decimal onHand)
    {
        if (!decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity))
        {
            quantity = 0m;
        }

        return FormatDecimal(Math.Clamp(quantity, 0m, onHand));
    }

    /// <summary>
    /// Returns an error message when the submitted picker rows have no selection or a checked
    /// row has a non-numeric, non-positive, or over-on-hand quantity; otherwise returns null.
    /// The dialog is kept open (PrimaryButtonClick is cancelled) until the rows are valid.
    /// </summary>
    private static string? ValidatePickRowsForSubmit(IReadOnlyList<StockPickRow> pickRows)
    {
        var checkedRows = pickRows.Where(row => row.IsChecked).ToList();
        if (checkedRows.Count == 0)
        {
            return "Select at least one row to continue.";
        }

        foreach (var row in checkedRows)
        {
            if (!TryParseDecimalOnly(row.Quantity, out var quantity))
            {
                return $"Quantity for {row.DisplayValue} must be a number (decimals only, e.g. 10 or 2.5).";
            }

            if (quantity <= 0)
            {
                return $"Quantity for {row.DisplayValue} must be greater than zero.";
            }

            if (quantity > row.OnHand)
            {
                return $"Quantity for {row.DisplayValue} cannot exceed the on-hand amount of {FormatDecimal(row.OnHand)}.";
            }
        }

        return null;
    }

    /// <summary>
    /// Parses a quantity as a plain decimal using the period as the decimal point. Thousands
    /// separators (commas) and exponent notation are not accepted, so a comma-decimal entry
    /// like "1,5" is rejected rather than silently read as 15.
    /// </summary>
    private static bool TryParseDecimalOnly(string? raw, out decimal value)
    {
        return decimal.TryParse(
            raw?.Trim(),
            NumberStyles.AllowLeadingSign
                | NumberStyles.AllowDecimalPoint
                | NumberStyles.AllowLeadingWhite
                | NumberStyles.AllowTrailingWhite,
            CultureInfo.InvariantCulture,
            out value
        );
    }

    /// <summary>
    /// Applies the repo-standard ContentDialog sizing used by the larger list/detail
    /// dialogs (e.g. Module_Receiving EditModeColumnChooser, Module_Dunnage dialogs).
    /// Without this, ContentDialog clamps to its default 548px max width.
    /// </summary>
    private static void ApplyPickerDialogSizing(ContentDialog dialog)
    {
        dialog.Resources["ContentDialogMinWidth"] = 900d;
        dialog.Resources["ContentDialogMaxWidth"] = 1600d;
        dialog.Resources["ContentDialogMaxHeight"] = 760d;
    }

    /// <summary>
    /// Builds the modal body: a Select All/None toggle, a column-header row, the list, and an
    /// inline validation error line shown when submitted quantities are not valid.
    /// </summary>
    private static (StackPanel Panel, Button SelectAllButton, TextBlock ErrorText) BuildPickerContent(
        ListView listView,
        string firstColumnHeader
    )
    {
        var header = new Grid
        {
            ColumnSpacing = 12,
            Margin = new Thickness(4, 0, 4, 6),
        };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });

        AddHeader(header, 1, firstColumnHeader);
        AddHeader(header, 2, "Total Qty");
        AddHeader(header, 3, "Qty");
        AddHeader(header, 4, "# Trans");

        var selectAllButton = new Button
        {
            Content = "Select All",
            HorizontalAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(12, 4, 12, 4),
        };

        var errorText = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Visibility = Visibility.Collapsed,
        };
        if (
            Application.Current.Resources.TryGetValue(
                "SystemFillColorCriticalBrush",
                out var criticalBrush
            ) && criticalBrush is Brush criticalBrushBrush
        )
        {
            errorText.Foreground = criticalBrushBrush;
        }

        // Content floor matching ContentDialogMinWidth so rows never collapse below a
        // usable Part column (mirrors the reference dialogs' content MinWidth).
        var panel = new StackPanel { Spacing = 6, MinWidth = 900 };
        panel.Children.Add(selectAllButton);
        panel.Children.Add(header);
        panel.Children.Add(listView);
        panel.Children.Add(errorText);
        return (panel, selectAllButton, errorText);
    }

    private static void AddHeader(Grid header, int column, string text)
    {
        var label = new TextBlock
        {
            Text = text,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(label, column);
        header.Children.Add(label);
    }

    private DataTemplate BuildStockPickTemplate()
    {
        return (DataTemplate)XamlReader.Load(
            """
            <DataTemplate
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:controls="using:Microsoft.UI.Xaml.Controls">
                <Grid Padding="4,4" ColumnSpacing="12">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="40" />
                        <ColumnDefinition Width="1.5*" />
                        <ColumnDefinition Width="90" />
                        <ColumnDefinition Width="120" />
                        <ColumnDefinition Width="110" />
                    </Grid.ColumnDefinitions>
                    <CheckBox Grid.Column="0" IsChecked="{Binding IsChecked, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" VerticalAlignment="Center" />
                    <TextBlock Grid.Column="1" Text="{Binding DisplayValue}" FontWeight="SemiBold" VerticalAlignment="Center" />
                    <TextBlock Grid.Column="2" Text="{Binding OnHandDisplay}" VerticalAlignment="Center" />
                    <TextBox Grid.Column="3" Text="{Binding Quantity, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" VerticalAlignment="Center" />
                    <controls:NumberBox Grid.Column="4" Minimum="1" SmallChange="1" SpinButtonPlacementMode="Compact" Value="{Binding TransactionCount, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" VerticalAlignment="Center" />
                </Grid>
            </DataTemplate>
            """
        );
    }

    // ── Focus/selection wiring ───────────────────────────────────────────────────

    private void OnPartIdFocusRequested()
    {
        PartIdLookupControl?.FocusInput();
    }

    private void OnPartIdClearRequested()
    {
        PartIdLookupControl.InputValue = string.Empty;
    }

    private void OnFirstRowToCellFocusRequested()
    {
        // Focus the first row's destination cell after the modal populates the list.
        DispatcherQueue.TryEnqueue(() => MoveFocusToCell(0, "ToCell"));
    }

    // ── Table cell navigation (Tab: To -> Qty -> next row's To; Enter: next row) ──

    private void SessionItemTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (sender is not TextBox box || box.DataContext is not Model_ScannerBatchItem item)
        {
            return;
        }

        var isToCell = string.Equals(box.Tag?.ToString(), "ToCell", StringComparison.Ordinal);
        var rowIndex = ViewModel.SessionItems.IndexOf(item);
        if (rowIndex < 0)
        {
            return;
        }

        if (e.Key == VirtualKey.Tab)
        {
            e.Handled = true;
            // To -> Qty (same row); Qty -> next row's To (wrap to Part input on the last row).
            MoveFocusToCell(isToCell ? rowIndex : rowIndex + 1, isToCell ? "QtyCell" : "ToCell");
            return;
        }

        if (e.Key == VirtualKey.Enter)
        {
            e.Handled = true;
            // Enter goes down to the next line: To -> next row's Qty; Qty -> next row's To.
            MoveFocusToCell(rowIndex + 1, isToCell ? "QtyCell" : "ToCell");
        }
    }

    private void MoveFocusToCell(int rowIndex, string cellTag)
    {
        if (rowIndex < 0 || rowIndex >= ViewModel.SessionItems.Count)
        {
            // Past the last row -> return to the part lookup to start the next part.
            PartIdLookupControl?.FocusInput();
            return;
        }

        var item = ViewModel.SessionItems[rowIndex];
        var container = SessionItemsListView.ContainerFromIndex(rowIndex) as ListViewItem;
        var target = FindCellTextBox(container, cellTag);

        if (target is null)
        {
            // Container not realized yet (virtualization) -> bring it into view and retry.
            SessionItemsListView.ScrollIntoView(item);
            container = SessionItemsListView.ContainerFromIndex(rowIndex) as ListViewItem;
            target = FindCellTextBox(container, cellTag);
        }

        if (target is not null)
        {
            target.Focus(FocusState.Programmatic);
            target.SelectAll();
        }
    }

    private static TextBox? FindCellTextBox(DependencyObject? root, string cellTag)
    {
        if (root is null)
        {
            return null;
        }

        if (root is TextBox textBox && string.Equals(textBox.Tag?.ToString(), cellTag, StringComparison.Ordinal))
        {
            return textBox;
        }

        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var found = FindCellTextBox(VisualTreeHelper.GetChild(root, index), cellTag);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    // ── Event handlers ───────────────────────────────────────────────────────────

    private async void PartIdLookupControl_ValidationCompleted(
        object sender,
        Model_SharedLookupValidationCompletedEventArgs args
    )
    {
        if (args?.Result is null)
        {
            return;
        }

        var result = args.Result;

        // Edge-case fix: when no exact part/location matched but close matches exist, present
        // them for the operator to choose instead of silently passing the (wrong) typed value
        // down to the "part not found" path.
        if (
            result.UsedFuzzyFallback
            && result.HasExactMatch is false
            && result.FuzzyCandidates.Count > 0
        )
        {
            var searchTerm = result.FormattedValue ?? result.RawInput ?? string.Empty;
            var selectedResult = await ShowFuzzyPickerAsync(searchTerm, result.FuzzyCandidates);

            if (selectedResult is null)
            {
                ClearLookupInput();
                return;
            }

            var selectedValue = (selectedResult.Key ?? selectedResult.Label ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(selectedValue))
            {
                ClearLookupInput();
                return;
            }

            PartIdLookupControl.InputValue = selectedValue;
            ViewModel.NewPartId = selectedValue;

            if (ViewModel.SearchMode == Enum_ScannerSearchMode.Location)
            {
                await ViewModel.LocationValidationCompletedAsync(selectedValue);
            }
            else
            {
                await ViewModel.PartValidationCompletedAsync(selectedValue);
            }

            return;
        }

        var resolved = result.ResolvedValue
            ?? result.FormattedValue
            ?? result.RawInput
            ?? ViewModel.NewPartId;

        if (ViewModel.SearchMode == Enum_ScannerSearchMode.Location)
        {
            await ViewModel.LocationValidationCompletedAsync(resolved);
        }
        else
        {
            await ViewModel.PartValidationCompletedAsync(resolved);
        }
    }

    private void ClearLookupInput()
    {
        PartIdLookupControl.InputValue = string.Empty;
        ViewModel.NewPartId = string.Empty;
    }

    private async Task<Model_FuzzySearchResult?> ShowFuzzyPickerAsync(
        string searchTerm,
        IReadOnlyList<Model_FuzzySearchResult> candidates
    )
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        var xamlRoot = XamlRoot ?? this.XamlRoot;
        if (xamlRoot is null)
        {
            return null;
        }

        var isLocationMode = ViewModel.SearchMode == Enum_ScannerSearchMode.Location;
        var dialog = new Dialog_FuzzySearchPicker(
            candidates,
            isLocationMode ? "Select Location" : "Select Part",
            isLocationMode
                ? $"No exact location match was found for '{searchTerm}'. Select a similar location to continue."
                : $"No exact part match was found for '{searchTerm}'. Select a similar part to continue."
        )
        {
            XamlRoot = xamlRoot,
        };

        var outcome = await dialog.ShowAsync();
        if (outcome != ContentDialogResult.Primary)
        {
            return null;
        }

        return dialog.SelectedResult;
    }

    /// <summary>
    /// Table-row destination edit (Step 6b-a1-c). Commits the typed value to the row, then
    /// sanitizes + validates it so the Status column and Send enablement update on focus loss.
    /// </summary>
    private async void SessionItemToTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox toBox || toBox.DataContext is not Model_ScannerBatchItem item)
        {
            return;
        }

        item.PayloadToLocation = toBox.Text;
        await ViewModel.ValidateSessionItemAsync(item);
    }

    /// <summary>Blocks non-numeric or negative quantities while typing in the table row.</summary>
    private void SessionItemQtyTextBox_BeforeTextChanging(
        TextBox sender,
        TextBoxBeforeTextChangingEventArgs args
    )
    {
        if (args is null || args.NewText.Length == 0)
        {
            return;
        }

        // Decimals only: reject commas (thousands separators) and exponent forms so entries
        // like "1,5" cannot be misread, and reject negatives while typing.
        if (!TryParseDecimalOnly(args.NewText, out var value) || value < 0)
        {
            args.Cancel = true;
        }
    }

    /// <summary>Table-row quantity edit: commits the typed value and re-validates the row.</summary>
    private async void SessionItemQtyTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox qtyBox || qtyBox.DataContext is not Model_ScannerBatchItem item)
        {
            return;
        }

        item.PayloadQuantity = qtyBox.Text;
        await ViewModel.ValidateSessionItemAsync(item);
    }

    /// <summary>Local row type for the stock/part picker modal.</summary>
    private sealed partial class StockPickRow : ObservableObject
    {
        /// <summary>Part id (location-search mode).</summary>
        public string PartId { get; init; } = string.Empty;

        /// <summary>Location id (part-number mode).</summary>
        public string LocationId { get; init; } = string.Empty;

        public string WarehouseCode { get; init; } = string.Empty;

        public decimal OnHand { get; init; }

        /// <summary>On-hand formatted without trailing zeros (e.g. 16452, 12554.5).</summary>
        public string OnHandDisplay { get; init; } = string.Empty;

        public string Quantity { get; set; } = string.Empty;

        public int TransactionCount { get; set; } = 1;

        [ObservableProperty]
        private bool _isChecked;

        /// <summary>Value shown in the first column: the part in location mode, else the location.</summary>
        public string DisplayValue => string.IsNullOrEmpty(PartId) ? LocationId : PartId;
    }
}
