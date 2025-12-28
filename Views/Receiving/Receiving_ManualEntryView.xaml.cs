using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;
using Windows.System;
using Windows.UI.Core;
using CommunityToolkit.WinUI.UI.Controls;
using System.Linq;
using System.Collections;
using System;
using System.Diagnostics;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class Receiving_ManualEntryView : UserControl
    {
        public Receiving_ManualEntryViewModel ViewModel { get; }

        public Receiving_ManualEntryView()
        {
            ViewModel = App.GetService<Receiving_ManualEntryViewModel>();
            this.DataContext = ViewModel;
            this.InitializeComponent();

            // Listen for collection changes to handle "Add Row" focus
            ViewModel.Loads.CollectionChanged += Loads_CollectionChanged;
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
                        var newItem = e.NewItems?[0] as Model_ReceivingLoad;
                        if (newItem != null)
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
            if (grid != null)
            {
                // Wait for the move to complete then activate edit mode
                grid.DispatcherQueue.TryEnqueue(() =>
                {
                    if (grid.CurrentColumn != null && !grid.CurrentColumn.IsReadOnly)
                    {
                        Debug.WriteLine($"[ManualEntryView] CurrentCellChanged: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
                        grid.BeginEdit();
                    }
                });
            }
        }

        private void ManualEntryDataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null) return;

            var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
            bool isShiftDown = (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

            Debug.WriteLine($"[ManualEntryView] KeyDown: Key={e.Key}, Shift={isShiftDown}, OriginalSource={e.OriginalSource}, CurrentColumn={grid.CurrentColumn?.Header}");
        }

        private void ManualEntryDataGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid == null) return;

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
                    if (grid.CurrentColumn != null && !grid.CurrentColumn.IsReadOnly)
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
    }
}
