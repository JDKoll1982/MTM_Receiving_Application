using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;
using MTM_Receiving_Application.Module_Shared.Views.Controls;
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
        private bool _isDialogTransitionActive;
        private readonly Dictionary<Control_Shared_TypedLookupTextBox, string> _lookupStartValues =
            new();

        public View_Receiving_ManualEntry(
            ViewModel_Receiving_ManualEntry viewModel,
            IService_Focus focusService,
            IService_QualityHoldWarning qualityHoldWarning,
            IService_ReceivingSettings receivingSettings
        )
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
            this.Loaded += View_Receiving_ManualEntry_Loaded;

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
                _isPaddingEnabled = await _receivingSettings.GetBoolAsync(
                    ReceivingSettingsKeys.PartNumberPadding.Enabled
                );
                var rulesJson = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.PartNumberPadding.RulesJson
                );

                if (!string.IsNullOrWhiteSpace(rulesJson))
                {
                    try
                    {
                        var rules = JsonSerializer.Deserialize<Model_PartNumberPrefixRule[]>(
                            rulesJson
                        );
                        if (rules?.Length > 0)
                        {
                            _paddingRules = rules.ToList();
                            Debug.WriteLine(
                                $"[ManualEntry] Loaded {_paddingRules.Count} padding rules"
                            );
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        Debug.WriteLine(
                            $"[ManualEntry] Failed to deserialize padding rules JSON: {jsonEx.Message}"
                        );
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

        private void View_Receiving_ManualEntry_Loaded(object sender, RoutedEventArgs e)
        {
            ManualEntryDataGrid.DispatcherQueue.TryEnqueue(() =>
            {
                if (ViewModel.Loads.Count == 0)
                {
                    return;
                }

                ManualEntryDataGrid.SelectedItem = ViewModel.Loads[0];
                ManualEntryDataGrid.ScrollIntoView(
                    ViewModel.Loads[0],
                    GetPreferredEntryColumn(ViewModel.Loads[0])
                );
                SelectPreferredEditableCell(ManualEntryDataGrid);
            });
        }

        private void Loads_CollectionChanged(
            object? sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e
        )
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
                            Debug.WriteLine(
                                $"[ManualEntryView] Loads_CollectionChanged: Selecting new item LoadNumber={newItem.LoadNumber}"
                            );
                            ManualEntryDataGrid.SelectedItem = newItem;
                            ManualEntryDataGrid.ScrollIntoView(
                                newItem,
                                GetPreferredEntryColumn(newItem)
                            );

                            // Use async delay to ensure grid is fully ready before entering edit mode
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(100); // Give grid time to complete selection and render
                                ManualEntryDataGrid.DispatcherQueue.TryEnqueue(() =>
                                {
                                    SelectPreferredEditableCell(ManualEntryDataGrid);
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
                if (_isDialogTransitionActive)
                {
                    Debug.WriteLine(
                        "[ManualEntryView] CurrentCellChanged: BeginEdit suppressed during dialog transition."
                    );
                    return;
                }

                if (grid.CurrentColumn?.IsReadOnly == false)
                {
                    Debug.WriteLine(
                        $"[ManualEntryView] CurrentCellChanged: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}"
                    );
                    TryBeginEdit(grid, "CurrentCellChanged");
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
            if (
                string.IsNullOrWhiteSpace(partID)
                || currentLoad.IsQualityHoldRequired
                || (partID == _lastCheckedPartID && currentLoad.LoadID == _lastCheckedLoadId)
            )
            {
                return;
            }

            // Check if this is a restricted part
            if (_qualityHoldWarning.IsRestrictedPart(partID))
            {
                _lastCheckedPartID = partID; // Remember we checked this
                _lastCheckedLoadId = currentLoad.LoadID;

                // Show warning and get user acknowledgment
                bool acknowledged = await RunWithDialogTransitionSuppressedAsync(() =>
                    _qualityHoldWarning.CheckAndWarnAsync(partID, currentLoad)
                );

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
                            c.Header?.ToString()
                                ?.Contains("Part", StringComparison.OrdinalIgnoreCase) == true
                        );

                        if (partIDColumn != null)
                        {
                            grid.CurrentColumn = partIDColumn;
                            TryBeginEdit(grid, "QualityHoldRefocus");
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
            bool isShiftDown =
                (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

            Debug.WriteLine(
                $"[ManualEntryView] KeyDown: Key={e.Key}, Shift={isShiftDown}, OriginalSource={e.OriginalSource}, CurrentColumn={grid.CurrentColumn?.Header}"
            );
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
                Debug.WriteLine(
                    "[ManualEntryView] Tapped: SelectedItem is null (Header or empty space). Selecting first editable cell."
                );
                // Header clicked or empty space
                if (grid.ItemsSource is IList list && list.Count > 0)
                {
                    grid.SelectedIndex = 0;
                    grid.SelectedItem = list[0];
                    SelectPreferredEditableCell(grid);
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
                    if (_isDialogTransitionActive)
                    {
                        Debug.WriteLine(
                            "[ManualEntryView] Tapped: BeginEdit suppressed during dialog transition."
                        );
                        return;
                    }

                    if (grid.CurrentColumn?.IsReadOnly == false)
                    {
                        Debug.WriteLine(
                            $"[ManualEntryView] Tapped: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}"
                        );
                        TryBeginEdit(grid, "Tapped");
                    }
                });
            }
        }

        private void SelectPreferredEditableCell(DataGrid grid)
        {
            Debug.WriteLine("[ManualEntryView] SelectPreferredEditableCell: Starting");
            if (grid.ItemsSource is IList items && items.Count > 0)
            {
                var currentLoad = grid.SelectedItem as Model_ReceivingLoad;
                var preferredColumn =
                    GetPreferredEntryColumn(currentLoad)
                    ?? grid.Columns.OrderBy(c => c.DisplayIndex).FirstOrDefault(c => !c.IsReadOnly);

                if (preferredColumn != null)
                {
                    Debug.WriteLine(
                        $"[ManualEntryView] SelectPreferredEditableCell: Found editable column {preferredColumn.Header}. Setting CurrentColumn and calling BeginEdit."
                    );
                    grid.CurrentColumn = preferredColumn;
                    grid.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
                    TryBeginEdit(grid, "SelectPreferredEditableCell");
                    Debug.WriteLine(
                        $"[ManualEntryView] SelectPreferredEditableCell: BeginEdit requested for column {preferredColumn.Header}"
                    );
                }
                else
                {
                    Debug.WriteLine(
                        "[ManualEntryView] SelectPreferredEditableCell: No editable column found."
                    );
                }
            }
            else
            {
                var itemCount = (grid.ItemsSource as IList)?.Count ?? 0;
                Debug.WriteLine(
                    $"[ManualEntryView] SelectPreferredEditableCell: Grid has no items (Count={itemCount})"
                );
            }
        }

        private DataGridColumn? GetPreferredEntryColumn(Model_ReceivingLoad? load)
        {
            if (load?.IsNonPOItem == true)
            {
                return !PartIdColumn.IsReadOnly ? PartIdColumn : null;
            }

            return !PoNumberColumn.IsReadOnly ? PoNumberColumn
                : !PartIdColumn.IsReadOnly ? PartIdColumn
                : null;
        }

        /// <summary>
        /// LoadingRow event handler for applying row-level highlighting based on quality holds.
        /// Subscribes to each load's PropertyChanged so the background stays reactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                if (s is not Model_ReceivingLoad changedLoad)
                {
                    return;
                }

                e.Row.DispatcherQueue.TryEnqueue(() =>
                {
                    // Guard against recycled rows that now show a different load
                    if (
                        e.Row.DataContext is Model_ReceivingLoad currentRowLoad
                        && currentRowLoad.LoadID == changedLoad.LoadID
                    )
                    {
                        if (pe.PropertyName == nameof(Model_ReceivingLoad.IsQualityHoldRequired))
                        {
                            ApplyQualityHoldRowBackground(e.Row, changedLoad.IsQualityHoldRequired);
                        }

                        if (
                            pe.PropertyName == nameof(Model_ReceivingLoad.IsNonPOItem)
                            && Equals(ManualEntryDataGrid.SelectedItem, changedLoad)
                        )
                        {
                            SelectPreferredEditableCell(ManualEntryDataGrid);
                        }
                    }
                });
            };

            load.PropertyChanged += handler;

            // Clean up when the row is recycled or unloaded to prevent memory leaks
            e.Row.Unloaded += (_, _) => load.PropertyChanged -= handler;
        }

        private static void ApplyQualityHoldRowBackground(
            DataGridRow row,
            bool isQualityHoldRequired
        )
        {
            row.Background = isQualityHoldRequired
                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 255, 230, 230)
                )
                : null;
        }

        /// <summary>
        /// Auto-formats PO Number when user leaves the textbox.
        /// Applies same formatting logic as guided workflow: PO-NNNNNN (6 digits, zero-padded)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PONumberTextBox_LostFocus(object sender, RoutedEventArgs e)
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

            await RunWithDialogTransitionSuppressedAsync(async () =>
            {
                await ViewModel.ResolveManualEntryRowAsync(load);
                return true;
            });
        }

        // Matches 1-6 digits with an optional B/b suffix (e.g. "064489", "064489B")
        private static readonly System.Text.RegularExpressions.Regex _poNumberPartRegex = new(
            @"^(\d{1,6})([Bb]?)$",
            System.Text.RegularExpressions.RegexOptions.None
        );

        /// <summary>
        /// Formats a PO number to canonical form: PO-NNNNNN (6 digits, zero-padded).
        /// Also handles the B-suffix variant used for blanket POs (e.g. "64489B" → "PO-064489B").
        /// Mirrors Format-PoNumber in Import-ReceivingHistory.ps1.
        /// </summary>
        /// <param name="input"></param>
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
            else if (
                trimmed.StartsWith("po", StringComparison.OrdinalIgnoreCase)
                && trimmed.Length > 2
            )
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

        private void PartLookupControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Control_Shared_TypedLookupTextBox control)
            {
                return;
            }

            control.AutoResolveFuzzyMatches = false;
            control.PrefixPaddingRules = GetSharedPaddingRules();
        }

        private void LocationLookupControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Control_Shared_TypedLookupTextBox control)
            {
                return;
            }

            control.AutoResolveFuzzyMatches = false;
        }

        private void LookupControl_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not Control_Shared_TypedLookupTextBox control)
            {
                return;
            }

            _lookupStartValues[control] = control.InputValue ?? string.Empty;
        }

        private async void PartLookupControl_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not Control_Shared_TypedLookupTextBox control)
            {
                return;
            }

            if (control.IsValidationInProgress)
            {
                return;
            }

            await control.ValidateAsync();
            QueueGridReloadIfLookupValueChanged(control);
        }

        private async void LocationLookupControl_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not Control_Shared_TypedLookupTextBox control)
            {
                return;
            }

            await WaitForLookupValidationToFinishAsync(control);
            QueueGridReloadIfLookupValueChanged(control);
        }

        private async void PartLookupControl_ValidationCompleted(
            object sender,
            Model_SharedLookupValidationCompletedEventArgs e
        )
        {
            if (sender is not Control_Shared_TypedLookupTextBox control)
            {
                return;
            }

            if (control.DataContext is not Model_ReceivingLoad load)
            {
                return;
            }

            if (!e.Result.IsValid)
            {
                ViewModel.ShowStatus(
                    e.Result.Message,
                    Module_Core.Models.Enums.InfoBarSeverity.Warning
                );
                return;
            }

            var canonicalPartValue = string.IsNullOrWhiteSpace(e.Result.ResolvedValue)
                ? e.Result.FormattedValue
                : e.Result.ResolvedValue;

            if (string.IsNullOrWhiteSpace(canonicalPartValue) is false)
            {
                var normalizedPart = canonicalPartValue.Trim();
                control.InputValue = normalizedPart;
                load.PartID = normalizedPart;
            }

            if (e.Result.UsedFuzzyFallback && e.Result.HasExactMatch is false)
            {
                var selectedResult = await ShowFuzzyPickerAsync(
                    "Select Part",
                    $"No exact part match was found for '{e.Result.FormattedValue}'. Select a similar part to continue.",
                    e.Result.FuzzyCandidates
                );

                if (selectedResult is null)
                {
                    control.InputValue = string.Empty;
                    load.PartID = string.Empty;
                    return;
                }

                var selectedValue = (selectedResult.Key ?? selectedResult.Label ?? string.Empty)
                    .Trim();
                if (string.IsNullOrWhiteSpace(selectedValue))
                {
                    control.InputValue = string.Empty;
                    load.PartID = string.Empty;
                    return;
                }

                control.InputValue = selectedValue;
                load.PartID = selectedValue;
                await control.ValidateAsync();
                return;
            }

            await RunWithDialogTransitionSuppressedAsync(async () =>
            {
                await ViewModel.ResolveManualEntryRowAsync(load);
                return true;
            });
        }

        private async void LocationLookupControl_ValidationCompleted(
            object sender,
            Model_SharedLookupValidationCompletedEventArgs e
        )
        {
            if (sender is not Control_Shared_TypedLookupTextBox control)
            {
                return;
            }

            if (control.DataContext is not Model_ReceivingLoad load)
            {
                return;
            }

            if (!e.Result.IsValid)
            {
                ViewModel.ShowStatus(
                    e.Result.Message,
                    Module_Core.Models.Enums.InfoBarSeverity.Warning
                );
                return;
            }

            var canonicalLocationValue = string.IsNullOrWhiteSpace(e.Result.ResolvedValue)
                ? e.Result.FormattedValue
                : e.Result.ResolvedValue;

            if (string.IsNullOrWhiteSpace(canonicalLocationValue) is false)
            {
                var normalizedLocation = canonicalLocationValue.Trim();
                control.InputValue = normalizedLocation;
                load.InitialLocation = normalizedLocation;
            }

            if (e.Result.UsedFuzzyFallback && e.Result.HasExactMatch is false)
            {
                var selectedResult = await ShowFuzzyPickerAsync(
                    "Select Location",
                    $"No exact location match was found for '{e.Result.FormattedValue}'. Select a similar location to continue.",
                    e.Result.FuzzyCandidates
                );

                if (selectedResult is null)
                {
                    control.InputValue = string.Empty;
                    load.InitialLocation = string.Empty;
                    return;
                }

                var selectedValue = (selectedResult.Key ?? selectedResult.Label ?? string.Empty)
                    .Trim();
                if (string.IsNullOrWhiteSpace(selectedValue))
                {
                    control.InputValue = string.Empty;
                    load.InitialLocation = string.Empty;
                    return;
                }

                control.InputValue = selectedValue;
                load.InitialLocation = selectedValue;
                await control.ValidateAsync();
                return;
            }
        }

        private IReadOnlyList<Model_SharedLookupPrefixPaddingRule> GetSharedPaddingRules()
        {
            if (!_paddingSettingsLoaded || !_isPaddingEnabled || _paddingRules.Count == 0)
            {
                return Array.Empty<Model_SharedLookupPrefixPaddingRule>();
            }

            return _paddingRules
                .Where(rule =>
                    rule.IsEnabled
                    && string.IsNullOrWhiteSpace(rule.Prefix) is false
                    && rule.MaxLength > 0
                )
                .Select(rule => new Model_SharedLookupPrefixPaddingRule
                {
                    Prefix = rule.Prefix.Trim(),
                    MaxLength = rule.MaxLength,
                    PadCharacter = rule.PadChar,
                })
                .ToArray();
        }

        private async Task<Model_FuzzySearchResult?> ShowFuzzyPickerAsync(
            string title,
            string subtitle,
            IReadOnlyList<Model_FuzzySearchResult> candidates
        )
        {
            if (candidates.Count == 0)
            {
                return null;
            }

            _isDialogTransitionActive = true;
            try
            {
                var dialog = new Dialog_FuzzySearchPicker(candidates, title, subtitle)
                {
                    XamlRoot = XamlRoot,
                };

                var dialogResult = await dialog.ShowAsync();
                if (dialogResult != ContentDialogResult.Primary)
                {
                    return null;
                }

                return dialog.SelectedResult;
            }
            finally
            {
                _isDialogTransitionActive = false;
            }
        }

        private async void PartIdCell_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (
                sender is not FrameworkElement element
                || element.DataContext is not Model_ReceivingLoad load
            )
            {
                return;
            }

            e.Handled = true;
            await RunWithDialogTransitionSuppressedAsync(async () =>
            {
                await ViewModel.TrySelectPartForPoAsync(load, forceReselection: true);
                return true;
            });
        }

        private async void PackageTypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (
                sender is not FrameworkElement element
                || element.DataContext is not Model_ReceivingLoad load
            )
            {
                return;
            }

            await RunWithDialogTransitionSuppressedAsync(async () =>
            {
                await ShowPackageTypeDialogAsync(load);
                return true;
            });
        }

        private async Task ShowPackageTypeDialogAsync(Model_ReceivingLoad load)
        {
            var xamlRoot = this.XamlRoot;
            if (xamlRoot is null)
            {
                ViewModel.ShowStatus(
                    "Unable to display the package type dialog.",
                    Module_Core.Models.Enums.InfoBarSeverity.Error
                );
                return;
            }

            var selectedPackageType = ViewModel.GetManualEntryPackageTypeSelection(load);
            var customPackageTypeName = ViewModel.GetManualEntryCustomPackageTypeName(load);

            var packageTypeComboBox = new ComboBox
            {
                ItemsSource = ViewModel.ManualEntryPackageTypeOptions,
                SelectedItem = selectedPackageType,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 240,
                Header = "Type",
            };

            var customPackageTypeTextBox = new TextBox
            {
                Text = customPackageTypeName,
                Header = "Custom Name",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 240,
                Visibility =
                    selectedPackageType == "Custom" ? Visibility.Visible : Visibility.Collapsed,
            };

            var changeAllExistingRowsCheckBox = new CheckBox
            {
                Content = "Change all existing Rows",
                IsChecked = false,
            };

            var validationTextBlock = new TextBlock
            {
                Foreground = (Microsoft.UI.Xaml.Media.Brush)
                    Application.Current.Resources["SystemFillColorCriticalBrush"],
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed,
            };

            packageTypeComboBox.SelectionChanged += (_, _) =>
            {
                var isCustom = string.Equals(
                    packageTypeComboBox.SelectedItem?.ToString(),
                    "Custom",
                    StringComparison.OrdinalIgnoreCase
                );
                customPackageTypeTextBox.Visibility = isCustom
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                if (!isCustom)
                {
                    validationTextBlock.Visibility = Visibility.Collapsed;
                    validationTextBlock.Text = string.Empty;
                }
            };

            var dialogContent = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    packageTypeComboBox,
                    customPackageTypeTextBox,
                    changeAllExistingRowsCheckBox,
                    validationTextBlock,
                },
            };

            var dialog = new ContentDialog
            {
                Title = $"Update Package Type for {load.LoadDisplayText}",
                Content = dialogContent,
                PrimaryButtonText = "Apply",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot,
            };

            MTM_Receiving_Application.Module_Core.Helpers.Helper_UI_ContentDialogTheme.ApplyTheme(
                dialog,
                xamlRoot
            );

            dialog.PrimaryButtonClick += (_, args) =>
            {
                var validation = ViewModel.ApplyManualEntryPackageTypeChange(
                    load,
                    packageTypeComboBox.SelectedItem?.ToString() ?? string.Empty,
                    customPackageTypeTextBox.Text,
                    changeAllExistingRowsCheckBox.IsChecked == true
                );

                if (!validation.IsValid)
                {
                    validationTextBlock.Text = validation.Message;
                    validationTextBlock.Visibility = Visibility.Visible;
                    args.Cancel = true;
                }
            };

            await dialog.ShowAsync();
        }

        private async Task<T> RunWithDialogTransitionSuppressedAsync<T>(Func<Task<T>> action)
        {
            _isDialogTransitionActive = true;

            try
            {
                await Task.Yield();
                return await action();
            }
            finally
            {
                _isDialogTransitionActive = false;
            }
        }

        private static void TryBeginEdit(DataGrid grid, string source)
        {
            try
            {
                grid.BeginEdit();
            }
            catch (COMException ex)
            {
                Debug.WriteLine(
                    $"[ManualEntryView] {source}: BeginEdit suppressed after COMException: {ex.Message}"
                );
            }
        }

        private async Task WaitForLookupValidationToFinishAsync(
            Control_Shared_TypedLookupTextBox control
        )
        {
            const int maxAttempts = 25;
            for (var attempt = 0; attempt < maxAttempts && control.IsValidationInProgress; attempt++)
            {
                await Task.Delay(10);
            }
        }

        private void QueueGridReloadIfLookupValueChanged(Control_Shared_TypedLookupTextBox control)
        {
            if (_lookupStartValues.TryGetValue(control, out var startValue) is false)
            {
                return;
            }

            _lookupStartValues.Remove(control);

            var endValue = control.InputValue ?? string.Empty;
            if (string.Equals(startValue, endValue, StringComparison.Ordinal))
            {
                return;
            }

            ManualEntryDataGrid.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    ManualEntryDataGrid.CommitEdit();
                    ManualEntryDataGrid.UpdateLayout();
                    Bindings.Update();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"[ManualEntryView] Grid reload after lookup edit failed: {ex.Message}"
                    );
                }
            });
        }
    }
}
