using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
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
        private string? _lastCheckedPartID;

        public View_Receiving_ManualEntry(
            ViewModel_Receiving_ManualEntry viewModel,
            IService_Focus focusService,
            IService_QualityHoldWarning qualityHoldWarning)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(focusService);
            ArgumentNullException.ThrowIfNull(qualityHoldWarning);

            ViewModel = viewModel;
            _focusService = focusService;
            _qualityHoldWarning = qualityHoldWarning;
            this.DataContext = ViewModel;
            this.InitializeComponent();

            // Listen for collection changes to handle "Add Row" focus
            ViewModel.Loads.CollectionChanged += Loads_CollectionChanged;

            _focusService.AttachFocusOnVisibility(this, AddRowButton);
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

            // Skip if empty or if we already checked this exact value
            if (string.IsNullOrWhiteSpace(partID) || partID == _lastCheckedPartID)
            {
                return;
            }

            // Check if this is a restricted part
            if (_qualityHoldWarning.IsRestrictedPart(partID))
            {
                _lastCheckedPartID = partID; // Remember we checked this

                // Show warning and get user acknowledgment
                bool acknowledged = await _qualityHoldWarning.CheckAndWarnAsync(partID, currentLoad);

                if (!acknowledged)
                {
                    // User cancelled - clear the part ID
                    currentLoad.PartID = string.Empty;
                    _lastCheckedPartID = null;

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
        /// Fallback method when binding converters are insufficient.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManualEntryDataGrid_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            if (e.Row.DataContext is Model_ReceivingLoad load && load.IsQualityHoldRequired)
            {
                // Apply light red background to highlight quality hold rows
                e.Row.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 255, 230, 230)  // #FFE6E6
                );
            }
        }
    }
}

