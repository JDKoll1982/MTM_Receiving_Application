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
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Dialogs;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;
using MTM_Receiving_Application.Module_Shared.Views.Controls;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using Windows.System;
using Windows.UI.Core;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_EditMode : UserControl
    {
        private static readonly JsonSerializerOptions PartPaddingJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public ViewModel_Receiving_EditMode ViewModel { get; }
        private readonly IService_QualityHoldWarning _qualityHoldWarning;
        private string? _lastCheckedPartID;
        private IReadOnlyList<Model_SharedLookupPrefixPaddingRule> _partPaddingRules =
            Array.Empty<Model_SharedLookupPrefixPaddingRule>();
        private readonly Dictionary<Control_Shared_TypedLookupTextBox, string> _lookupStartValues =
            new();

        public View_Receiving_EditMode(
            ViewModel_Receiving_EditMode viewModel,
            IService_QualityHoldWarning qualityHoldWarning
        )
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(qualityHoldWarning);

            ViewModel = viewModel;
            _qualityHoldWarning = qualityHoldWarning;
            this.DataContext = ViewModel;
            this.InitializeComponent();

            // Wire up the column-chooser event from the ViewModel
            ViewModel.ShowColumnChooserRequested += OnShowColumnChooserRequested;

            // Apply saved column visibility once the control is loaded
            this.Loaded += async (_, _) => await ApplyColumnVisibilityAsync();
            this.Unloaded += OnUnloaded;

            _ = LoadPartPaddingRulesAsync();
        }

        private async Task LoadPartPaddingRulesAsync()
        {
            var receivingSettings = ResolveReceivingSettings();
            if (receivingSettings is null)
            {
                return;
            }

            try
            {
                var isEnabled = await receivingSettings.GetBoolAsync(
                    ReceivingSettingsKeys.PartNumberPadding.Enabled
                );
                if (!isEnabled)
                {
                    _partPaddingRules = Array.Empty<Model_SharedLookupPrefixPaddingRule>();
                    return;
                }

                var rulesJson = await receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.PartNumberPadding.RulesJson
                );
                if (string.IsNullOrWhiteSpace(rulesJson))
                {
                    _partPaddingRules = Array.Empty<Model_SharedLookupPrefixPaddingRule>();
                    return;
                }

                var configuredRules = JsonSerializer.Deserialize<Model_PartNumberPrefixRule[]>(
                    rulesJson,
                    PartPaddingJsonOptions
                );

                _partPaddingRules = (configuredRules ?? Array.Empty<Model_PartNumberPrefixRule>())
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
            catch
            {
                _partPaddingRules = Array.Empty<Model_SharedLookupPrefixPaddingRule>();
            }
        }

        private void PartLookupControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Control_Shared_TypedLookupTextBox control)
            {
                return;
            }

            control.AutoResolveFuzzyMatches = false;
            control.PrefixPaddingRules = _partPaddingRules;
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

        private async void LookupControl_LostFocus(object sender, RoutedEventArgs e)
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
                var normalized = canonicalPartValue.Trim();
                load.PartID = normalized;
                control.InputValue = normalized;
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
                    load.PartID = string.Empty;
                    control.InputValue = string.Empty;
                    return;
                }

                var selectedValue = (selectedResult.Key ?? selectedResult.Label ?? string.Empty)
                    .Trim();

                if (string.IsNullOrWhiteSpace(selectedValue))
                {
                    load.PartID = string.Empty;
                    control.InputValue = string.Empty;
                    return;
                }

                load.PartID = selectedValue;
                control.InputValue = selectedValue;
                await control.ValidateAsync();
            }
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
                var normalized = canonicalLocationValue.Trim();
                load.InitialLocation = normalized;
                control.InputValue = normalized;
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
                    load.InitialLocation = string.Empty;
                    control.InputValue = string.Empty;
                    return;
                }

                var selectedValue = (selectedResult.Key ?? selectedResult.Label ?? string.Empty)
                    .Trim();

                if (string.IsNullOrWhiteSpace(selectedValue))
                {
                    load.InitialLocation = string.Empty;
                    control.InputValue = string.Empty;
                    return;
                }

                load.InitialLocation = selectedValue;
                control.InputValue = selectedValue;
                await control.ValidateAsync();
            }
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

            var dialog = new Dialog_FuzzySearchPicker(candidates, title, subtitle)
            {
                XamlRoot = XamlRoot,
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return null;
            }

            return dialog.SelectedResult;
        }

        private static IService_ReceivingSettings? ResolveReceivingSettings()
        {
#pragma warning disable CS0618
            try
            {
                return App.GetService<IService_ReceivingSettings>();
            }
            catch
            {
                return null;
            }
#pragma warning restore CS0618
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.ResetToDefaults();
        }

        // ------------------------------------------------------------------ column visibility
        private async Task ApplyColumnVisibilityAsync()
        {
            // ViewModel already loaded ColumnSettings; just mirror to the DataGrid columns
            await Task.CompletedTask;
            SyncColumnVisibilityToGrid();
        }

        private void SyncColumnVisibilityToGrid()
        {
            if (ViewModel.ColumnSettings.Count == 0)
            {
                return;
            }

            var visibilityMap = ViewModel.ColumnSettings.ToDictionary(c => c.Key, c => c.IsVisible);

            foreach (var col in EditModeDataGrid.Columns)
            {
                if (col.Tag is string tag && visibilityMap.TryGetValue(tag, out bool visible))
                {
                    col.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        // ------------------------------------------------------------------ column chooser dialog
        private void ColumnsButton_Click(object sender, RoutedEventArgs e)
        {
            // Let the ViewModel fire the event (which routes back here)
            ViewModel.ShowColumnChooserCommand.Execute(null);
        }

        // ------------------------------------------------------------------ date range flyout
        /// <summary>Executes the command attached to a flyout button's Tag, then closes the flyout.</summary>
        private void DateFilterActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (
                sender is FrameworkElement { Tag: System.Windows.Input.ICommand command }
                && command.CanExecute(null)
            )
            {
                command.Execute(null);
            }

            DateRangeFlyout?.Hide();
        }

        private async void OnShowColumnChooserRequested(object? sender, EventArgs e)
        {
            await ShowColumnChooserDialogAsync();
        }

        private async Task ShowColumnChooserDialogAsync()
        {
            var dialog = new Dialog_Receiving_EditModeColumnChooser(ViewModel.ColumnSettings)
            {
                XamlRoot = this.XamlRoot,
            };
            dialog.PrepareDialogSize();
            dialog.HorizontalAlignment = HorizontalAlignment.Center;
            dialog.VerticalAlignment = VerticalAlignment.Center;
            await dialog.ShowAsync();
            if (!dialog.WasAccepted)
            {
                return;
            }

            // Persist to settings
            await ViewModel.SaveColumnVisibilityAsync();

            // Mirror to the DataGrid
            SyncColumnVisibilityToGrid();
        }

        // ------------------------------------------------------------------ grid interaction
        private void EditModeDataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            var tag = e.Column.Tag as string;
            if (string.IsNullOrEmpty(tag))
            {
                return;
            }

            bool ascending;
            if (
                e.Column.SortDirection == null
                || e.Column.SortDirection == DataGridSortDirection.Descending
            )
            {
                e.Column.SortDirection = DataGridSortDirection.Ascending;
                ascending = true;
            }
            else
            {
                e.Column.SortDirection = DataGridSortDirection.Descending;
                ascending = false;
            }

            // Clear sort indicator from all other columns
            if (sender is DataGrid grid)
            {
                foreach (var col in grid.Columns)
                {
                    if (col != e.Column)
                    {
                        col.SortDirection = null;
                    }
                }
            }

            ViewModel.SortBy(tag, ascending);
        }

        private void GoToPageTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                ViewModel.GoToPageCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void EditModeDataGrid_CurrentCellChanged(object? sender, EventArgs e)
        {
            var grid = sender as DataGrid;
            _ = CheckQualityHoldOnCellChangeAsync(grid);

            grid?.DispatcherQueue.TryEnqueue(() =>
            {
                if (grid.CurrentColumn?.IsReadOnly == false)
                {
                    grid.BeginEdit();
                }
            });
        }

        private async Task CheckQualityHoldOnCellChangeAsync(DataGrid? grid)
        {
            if (grid?.SelectedItem is not Model_ReceivingLoad currentLoad)
            {
                return;
            }

            var partID = currentLoad.PartID;

            if (string.IsNullOrWhiteSpace(partID) || partID == _lastCheckedPartID)
            {
                return;
            }

            if (_qualityHoldWarning.IsRestrictedPart(partID))
            {
                _lastCheckedPartID = partID;

                bool acknowledged = await _qualityHoldWarning.CheckAndWarnAsync(
                    partID,
                    currentLoad
                );

                if (!acknowledged)
                {
                    currentLoad.PartID = string.Empty;
                    _lastCheckedPartID = null;

                    grid.DispatcherQueue.TryEnqueue(() =>
                    {
                        var partIDColumn = grid.Columns.FirstOrDefault(c =>
                            c.Header?.ToString()
                                ?.Contains("Part", StringComparison.OrdinalIgnoreCase) == true
                        );

                        if (partIDColumn != null)
                        {
                            grid.CurrentColumn = partIDColumn;
                            grid.BeginEdit();
                        }
                    });
                }
            }
        }

        private void EditModeDataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (sender is not DataGrid grid)
            {
                return;
            }

            var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
            bool isShiftDown =
                (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

            Debug.WriteLine(
                $"[EditModeView] KeyDown: Key={e.Key}, Shift={isShiftDown}, Col={grid.CurrentColumn?.Header}"
            );
        }

        private void EditModeDataGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is not DataGrid grid)
            {
                return;
            }

            if (grid.SelectedItem == null)
            {
                if (grid.ItemsSource is IList list && list.Count > 0)
                {
                    grid.SelectedIndex = 0;
                    SelectFirstEditableCell(grid);
                }
            }
            else
            {
                grid.DispatcherQueue.TryEnqueue(() =>
                {
                    if (grid.CurrentColumn?.IsReadOnly == false)
                    {
                        grid.BeginEdit();
                    }
                });
            }
        }

        private static void SelectFirstEditableCell(DataGrid grid)
        {
            if (grid.ItemsSource is IList items && items.Count > 0)
            {
                var firstEditable = grid
                    .Columns.Where(c => c.Visibility == Visibility.Visible)
                    .OrderBy(c => c.DisplayIndex)
                    .FirstOrDefault(c => !c.IsReadOnly);

                if (firstEditable != null)
                {
                    grid.CurrentColumn = firstEditable;
                    grid.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
                    grid.BeginEdit();
                }
            }
        }

        private void EditModeDataGrid_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            // Reprint row highlighting moved to the dedicated Reprint Labels page.
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

            EditModeDataGrid.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    EditModeDataGrid.CommitEdit();
                    EditModeDataGrid.UpdateLayout();
                    Bindings.Update();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[EditModeView] Grid reload after lookup edit failed: {ex.Message}");
                }
            });
        }
    }
}
