using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Dialogs;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using Windows.System;
using Windows.UI.Core;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_EditMode : UserControl
    {
        public ViewModel_Receiving_EditMode ViewModel { get; }
        private readonly IService_QualityHoldWarning _qualityHoldWarning;
        private string? _lastCheckedPartID;

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
    }
}
