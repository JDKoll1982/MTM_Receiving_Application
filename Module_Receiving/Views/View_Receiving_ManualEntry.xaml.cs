using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using Windows.System;
using Windows.UI.Core;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_ManualEntry : UserControl
    {
        public ViewModel_Receiving_ManualEntry ViewModel { get; }
        private readonly IService_Focus _focusService;
        private readonly IService_QualityHoldWarning _qualityHoldWarning;
        private readonly IService_ReceivingSettings _receivingSettings;
        private string? _lastCheckedPartID;
        private Guid? _lastCheckedLoadId;
        private List<Model_PartNumberPrefixRule> _paddingRules = new();
        private bool _isPaddingEnabled;
        private bool _paddingSettingsLoaded;

        public View_Receiving_ManualEntry(
            ViewModel_Receiving_ManualEntry viewModel,
            IService_Focus focusService,
            IService_QualityHoldWarning qualityHoldWarning,
            IService_ReceivingSettings receivingSettings)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(focusService);
            ArgumentNullException.ThrowIfNull(qualityHoldWarning);
            ArgumentNullException.ThrowIfNull(receivingSettings);

            ViewModel = viewModel;
            _focusService = focusService;
            _qualityHoldWarning = qualityHoldWarning;
            _receivingSettings = receivingSettings;
            this.DataContext = ViewModel;
            this.InitializeComponent();

            // Listen for collection changes to handle "Add Row" focus
            ViewModel.Loads.CollectionChanged += Loads_CollectionChanged;

            _focusService.AttachFocusOnVisibility(this, AddRowButton);

            // Load padding settings
            _ = LoadPaddingSettingsAsync();
        }

        private async Task LoadPaddingSettingsAsync()
        {
            try
            {
                _isPaddingEnabled = await _receivingSettings.GetBoolAsync(ReceivingSettingsKeys.PartNumberPadding.Enabled);
                var rulesJson = await _receivingSettings.GetStringAsync(ReceivingSettingsKeys.PartNumberPadding.RulesJson);

                if (!string.IsNullOrWhiteSpace(rulesJson))
                {
                    try
                    {
                        var rules = JsonSerializer.Deserialize<Model_PartNumberPrefixRule[]>(rulesJson);
                        if (rules?.Length > 0)
                        {
                            _paddingRules = rules.ToList();
                            Debug.WriteLine($"[ManualEntry] Loaded {_paddingRules.Count} padding rules");
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        Debug.WriteLine($"[ManualEntry] Failed to deserialize padding rules JSON: {jsonEx.Message}");
                        // Fall back to empty list
                        _paddingRules = new List<Model_PartNumberPrefixRule>();
                    }
                }
                _paddingSettingsLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ManualEntry] Failed to load padding settings: {ex.Message}");
                // Ensure defaults
                _isPaddingEnabled = false;
                _paddingRules = new List<Model_PartNumberPrefixRule>();
            }
        }

        private void Loads_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                Debug.WriteLine("[ManualEntryView] Loads_CollectionChanged: New row added");
                // When a new row is added, ensure we focus and edit the first cell
                ManualEntryDataGrid.DispatcherQueue.TryEnqueue(() =>
                {
                    if (ViewModel.Loads.Count > 0)
                    {
                        // Select the last added item (assuming add to bottom)
                        if (e.NewItems?[0] is Model_ReceivingLoad newItem)
                        {
                            Debug.WriteLine($"[ManualEntryView] Loads_CollectionChanged: Selecting new item LoadNumber={newItem.LoadNumber}");
                            ManualEntryDataGrid.SelectedItem = newItem;
                            ManualEntryDataGrid.ScrollIntoView(newItem, ManualEntryDataGrid.Columns.FirstOrDefault());

                            // Use async delay to ensure grid is fully ready before entering edit mode
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(100); // Give grid time to complete selection and render
                                ManualEntryDataGrid.DispatcherQueue.TryEnqueue(() =>
                                {
                                    SelectFirstEditableCell(ManualEntryDataGrid);
                                });
                            });
                        }
                    }
                });
            }
        }

        private void ManualEntryDataGrid_CurrentCellChanged(object? sender, EventArgs e)
        {
            var grid = sender as DataGrid;

            // Check for quality hold warning when leaving PartID cell
            _ = CheckQualityHoldOnCellChangeAsync(grid);

            // Wait for the move to complete then activate edit mode
            grid?.DispatcherQueue.TryEnqueue(() =>
            {
                if (grid.CurrentColumn?.IsReadOnly == false)
                {
                    Debug.WriteLine($"[ManualEntryView] CurrentCellChanged: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
                    grid.BeginEdit();
                }
            });
        }

        /// <summary>
        /// Checks for quality hold requirements when user moves away from a cell.
        /// Displays warning if restricted part (MMFSR/MMCSR) is detected.
        /// </summary>
        /// <param name="grid"></param>
        private async Task CheckQualityHoldOnCellChangeAsync(DataGrid? grid)
        {
            if (grid?.SelectedItem is not Model_ReceivingLoad currentLoad)
            {
                return;
            }

            var partID = currentLoad.PartID;

            // Skip if empty or if we already checked this exact value on this exact row
            if (string.IsNullOrWhiteSpace(partID) || (partID == _lastCheckedPartID && currentLoad.LoadID == _lastCheckedLoadId))
            {
                return;
            }

            // Check if this is a restricted part
            if (_qualityHoldWarning.IsRestrictedPart(partID))
            {
                _lastCheckedPartID = partID; // Remember we checked this
                _lastCheckedLoadId = currentLoad.LoadID;

                // Show warning and get user acknowledgment
                bool acknowledged = await _qualityHoldWarning.CheckAndWarnAsync(partID, currentLoad);

                if (!acknowledged)
                {
                    // User cancelled - clear the part ID
                    currentLoad.PartID = string.Empty;
                    _lastCheckedPartID = null;
                    _lastCheckedLoadId = null;

                    // Re-focus the PartID cell for correction
                    grid.DispatcherQueue.TryEnqueue(() =>
                    {
                        var partIDColumn = grid.Columns.FirstOrDefault(c =>
                            c.Header?.ToString()?.Contains("Part", StringComparison.OrdinalIgnoreCase) == true);

                        if (partIDColumn != null)
                        {
                            grid.CurrentColumn = partIDColumn;
                            grid.BeginEdit();
                        }
                    });
                }
            }
        }

        private void ManualEntryDataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (!(sender is DataGrid grid))
            {
                return;
            }

            var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
            bool isShiftDown = (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

            Debug.WriteLine($"[ManualEntryView] KeyDown: Key={e.Key}, Shift={isShiftDown}, OriginalSource={e.OriginalSource}, CurrentColumn={grid.CurrentColumn?.Header}");
        }

        private void ManualEntryDataGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!(sender is DataGrid grid))
            {
                return;
            }

            Debug.WriteLine($"[ManualEntryView] Tapped: OriginalSource={e.OriginalSource}");

            // Handle empty grid or header clicks
            if (grid.ItemsSource is IList items && items.Count == 0)
            {
                Debug.WriteLine("[ManualEntryView] Tapped: Grid empty, triggering AddRow command.");
                if (ViewModel.AddRowCommand.CanExecute(null))
                {
                    ViewModel.AddRowCommand.Execute(null);
                }
                // Focus handled by Loads_CollectionChanged
            }
            else if (grid.SelectedItem == null)
            {
                Debug.WriteLine("[ManualEntryView] Tapped: SelectedItem is null (Header or empty space). Selecting first editable cell.");
                // Header clicked or empty space
                if (grid.ItemsSource is IList list && list.Count > 0)
                {
                    grid.SelectedIndex = 0;
                    SelectFirstEditableCell(grid);
                }
            }
            else
            {
                // Cell clicked - CurrentCellChanged handles the edit mode if cell changes.
                // But if we tap the SAME cell, CurrentCellChanged might not fire.
                // So we ensure edit mode here too.
                Debug.WriteLine("[ManualEntryView] Tapped: Cell clicked. Enqueuing BeginEdit.");
                grid.DispatcherQueue.TryEnqueue(() =>
                {
                    if (grid.CurrentColumn?.IsReadOnly == false)
                    {
                        Debug.WriteLine($"[ManualEntryView] Tapped: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
                        grid.BeginEdit();
                    }
                });
            }
        }

        private void SelectFirstEditableCell(DataGrid grid)
        {
            Debug.WriteLine("[ManualEntryView] SelectFirstEditableCell: Starting");
            if (grid.ItemsSource is IList items && items.Count > 0)
            {
                // Find first non-readonly column
                var firstEditable = grid.Columns.OrderBy(c => c.DisplayIndex).FirstOrDefault(c => !c.IsReadOnly);
                if (firstEditable != null)
                {
                    Debug.WriteLine($"[ManualEntryView] SelectFirstEditableCell: Found editable column {firstEditable.Header}. Setting CurrentColumn and calling BeginEdit.");
                    grid.CurrentColumn = firstEditable;
                    grid.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
                    grid.BeginEdit();
                    Debug.WriteLine($"[ManualEntryView] SelectFirstEditableCell: BeginEdit called for column {firstEditable.Header}");
                }
                else
                {
                    Debug.WriteLine("[ManualEntryView] SelectFirstEditableCell: No editable column found.");
                }
            }
            else
            {
                var itemCount = (grid.ItemsSource as IList)?.Count ?? 0;
                Debug.WriteLine($"[ManualEntryView] SelectFirstEditableCell: Grid has no items (Count={itemCount})");
            }
        }

        /// <summary>
        /// LoadingRow event handler for applying row-level highlighting based on quality holds.
        /// Subscribes to each load's PropertyChanged so the background stays reactive.
        /// </summary>
        private void ManualEntryDataGrid_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            if (e.Row.DataContext is not Model_ReceivingLoad load)
            {
                return;
            }

            ApplyQualityHoldRowBackground(e.Row, load.IsQualityHoldRequired);

            // Keep the background in sync when IsQualityHoldRequired changes after initial render
            System.ComponentModel.PropertyChangedEventHandler handler = (s, pe) =>
            {
                if (s is not Model_ReceivingLoad changedLoad
                    || pe.PropertyName != nameof(Model_ReceivingLoad.IsQualityHoldRequired))
                {
                    return;
                }

                ManualEntryDataGrid.DispatcherQueue.TryEnqueue(() =>
                {
                    // Guard against recycled rows that now show a different load
                    if (e.Row.DataContext is Model_ReceivingLoad currentRowLoad
                        && currentRowLoad.LoadID == changedLoad.LoadID)
                    {
                        ApplyQualityHoldRowBackground(e.Row, changedLoad.IsQualityHoldRequired);
                    }
                });
            };

            load.PropertyChanged += handler;

            // Clean up when the row is recycled or unloaded to prevent memory leaks
            e.Row.Unloaded += (_, _) => load.PropertyChanged -= handler;
        }

        private static void ApplyQualityHoldRowBackground(DataGridRow row, bool isQualityHoldRequired)
        {
            row.Background = isQualityHoldRequired
                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 230, 230))
                : null;
        }

        /// <summary>
        /// Auto-formats PO Number when user leaves the textbox.
        /// Applies same formatting logic as guided workflow: PO-NNNNNN (6 digits, zero-padded)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PONumberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            if (textBox.DataContext is not Model_ReceivingLoad load)
            {
                return;
            }

            var value = textBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(value))
            {
                load.PoNumber = null;
                return;
            }

            // Auto-format PO number
            string formattedPO = FormatPONumber(value);
            
            if (formattedPO != value)
            {
                load.PoNumber = formattedPO;
                textBox.Text = formattedPO;
            }
        }

        // Matches 1-6 digits with an optional B/b suffix (e.g. "064489", "064489B")
        private static readonly System.Text.RegularExpressions.Regex _poNumberPartRegex =
            new(@"^(\d{1,6})([Bb]?)$", System.Text.RegularExpressions.RegexOptions.None);

        /// <summary>
        /// Formats a PO number to canonical form: PO-NNNNNN (6 digits, zero-padded).
        /// Also handles the B-suffix variant used for blanket POs (e.g. "64489B" → "PO-064489B").
        /// Mirrors Format-PoNumber in Import-ReceivingHistory.ps1.
        /// </summary>
        private static string FormatPONumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            string trimmed = input.Trim();
            string numberPart;

            // Strip recognised prefixes ("PO-" or bare "PO", case-insensitive)
            if (trimmed.StartsWith("po-", StringComparison.OrdinalIgnoreCase))
            {
                numberPart = trimmed.Substring(3);
            }
            else if (trimmed.StartsWith("po", StringComparison.OrdinalIgnoreCase) && trimmed.Length > 2)
            {
                numberPart = trimmed.Substring(2);
            }
            else
            {
                numberPart = trimmed;
            }

            // Accept digits (1-6) with an optional single B/b suffix
            var match = _poNumberPartRegex.Match(numberPart);
            if (match.Success)
            {
                string digits = match.Groups[1].Value;
                string suffix = match.Groups[2].Value.ToUpperInvariant(); // "B" or ""
                return $"PO-{digits.PadLeft(6, '0')}{suffix}";
            }

            // Unrecognised format — return as-is
            return trimmed;
        }

        /// <summary>
        /// Auto-formats Part ID when user leaves the textbox using configured padding rules.
        /// Example: "MMC1000" → "MMC0001000" (if rule configured for MMC prefix with MaxLength 10)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PartIDTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            if (textBox.DataContext is not Model_ReceivingLoad load)
            {
                return;
            }

            var value = textBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(value))
            {
                load.PartID = string.Empty;
                return;
            }

            // Auto-format Part ID using configured padding rules
            string formattedPartID = ApplyPartNumberPadding(value);

            if (formattedPartID != value)
            {
                load.PartID = formattedPartID;
                textBox.Text = formattedPartID;
                Debug.WriteLine($"[ManualEntry] Formatted PartID: '{value}' → '{formattedPartID}'");
            }
        }

        /// <summary>
        /// Applies part number padding rules to the input string.
        /// </summary>
        /// <param name="input"></param>
        private string ApplyPartNumberPadding(string input)
        {
            // Guard: if settings haven't finished loading yet, skip formatting to avoid applying
            // stale defaults. The user can re-commit the cell or Auto-Fill once settings are ready.
            if (!_paddingSettingsLoaded || !_isPaddingEnabled || _paddingRules.Count == 0)
            {
                return input;
            }

            var result = input;

            foreach (var rule in _paddingRules.Where(r => r.IsEnabled))
            {
                result = rule.FormatPartNumber(result);
            }

            return result;
        }
    }
}


