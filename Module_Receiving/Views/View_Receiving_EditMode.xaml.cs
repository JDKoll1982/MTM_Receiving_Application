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
            IService_QualityHoldWarning qualityHoldWarning)
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

            var visibilityMap = ViewModel.ColumnSettings
                .ToDictionary(c => c.Key, c => c.IsVisible);

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
            // Build a CheckBox list from ColumnSettings, skipping always-visible columns
            var items = ViewModel.ColumnSettings
                .Where(c => !c.IsAlwaysVisible)
                .ToList();

            var panel = new StackPanel { Spacing = 6 };
            var checkBoxes = new List<(CheckBox Box, string Key)>();

            // Select All / Clear All quick buttons
            var quickRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Thickness(0, 0, 0, 8) };
            var selectAllBtn = new Button { Content = "Select All" };
            var clearAllBtn = new Button { Content = "Clear All" };
            quickRow.Children.Add(selectAllBtn);
            quickRow.Children.Add(clearAllBtn);
            panel.Children.Add(quickRow);

            foreach (var col in items)
            {
                var cb = new CheckBox
                {
                    Content = col.Header,
                    IsChecked = col.IsVisible,
                    Tag = col.Key,
                };
                panel.Children.Add(cb);
                checkBoxes.Add((cb, col.Key));
            }

            selectAllBtn.Click += (_, _) => { foreach (var (cb, _) in checkBoxes) cb.IsChecked = true; };
            clearAllBtn.Click += (_, _) => { foreach (var (cb, _) in checkBoxes) cb.IsChecked = false; };

            var scrollViewer = new ScrollViewer
            {
                Content = panel,
                MaxHeight = 400,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };

            var dialog = new ContentDialog
            {
                Title = "Choose visible columns",
                Content = scrollViewer,
                PrimaryButtonText = "Apply",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot,
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            // Apply user choices back to ColumnSettings
            foreach (var (box, key) in checkBoxes)
            {
                var colSetting = ViewModel.ColumnSettings.FirstOrDefault(c => c.Key == key);
                if (colSetting != null)
                {
                    colSetting.IsVisible = box.IsChecked == true;
                }
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
            if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
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

                bool acknowledged = await _qualityHoldWarning.CheckAndWarnAsync(partID, currentLoad);

                if (!acknowledged)
                {
                    currentLoad.PartID = string.Empty;
                    _lastCheckedPartID = null;

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

        private void EditModeDataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (sender is not DataGrid grid)
            {
                return;
            }

            var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
            bool isShiftDown = (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

            Debug.WriteLine($"[EditModeView] KeyDown: Key={e.Key}, Shift={isShiftDown}, Col={grid.CurrentColumn?.Header}");
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
                var firstEditable = grid.Columns
                    .Where(c => c.Visibility == Visibility.Visible)
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
