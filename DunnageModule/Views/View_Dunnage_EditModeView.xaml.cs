using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.DunnageModule.ViewModels;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Input;
using Windows.System;
using Windows.UI.Core;
using CommunityToolkit.WinUI.UI.Controls;
using System.Linq;
using System.Collections;
using System;
using System.Diagnostics;
using MTM_Receiving_Application.DunnageModule.Models;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.DunnageModule.Views
{
    public sealed partial class View_Dunnage_EditModeView : UserControl
    {
        public ViewModel_Dunnage_EditMode ViewModel { get; }

        public View_Dunnage_EditModeView()
        {
            ViewModel = App.GetService<ViewModel_Dunnage_EditMode>();
            this.DataContext = ViewModel;
            this.InitializeComponent();
        }

        private void EditModeDataGrid_CurrentCellChanged(object? sender, EventArgs e)
        {
            var grid = sender as DataGrid;
            // Wait for the move to complete then activate edit mode
            grid?.DispatcherQueue.TryEnqueue(() =>
            {
                if (grid.CurrentColumn?.IsReadOnly == false)
                {
                    Debug.WriteLine($"[Dunnage_EditModeView] CurrentCellChanged: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
                    grid.BeginEdit();
                }
            });
        }

        private void EditModeDataGrid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (sender is not DataGrid grid)
            {
                return;
            }

            var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
            bool isShiftDown = (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

            Debug.WriteLine($"[Dunnage_EditModeView] KeyDown: Key={e.Key}, Shift={isShiftDown}, OriginalSource={e.OriginalSource}, CurrentColumn={grid.CurrentColumn?.Header}");
        }

        private void EditModeDataGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is not DataGrid grid)
            {
                return;
            }

            Debug.WriteLine($"[Dunnage_EditModeView] Tapped: OriginalSource={e.OriginalSource}");

            // Handle empty grid or header clicks
            if (grid.ItemsSource is IList items && items.Count == 0)
            {
                Debug.WriteLine("[Dunnage_EditModeView] Tapped: Grid empty");
                return;
            }
            else if (grid.SelectedItem == null)
            {
                Debug.WriteLine("[Dunnage_EditModeView] Tapped: SelectedItem is null (Header or empty space). Selecting first editable cell.");
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
                Debug.WriteLine("[Dunnage_EditModeView] Tapped: Cell clicked. Enqueuing BeginEdit.");
                grid.DispatcherQueue.TryEnqueue(() =>
                {
                    if (grid.CurrentColumn?.IsReadOnly == false)
                    {
                        Debug.WriteLine($"[Dunnage_EditModeView] Tapped: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}");
                        grid.BeginEdit();
                    }
                });
            }
        }

        private void SelectFirstEditableCell(DataGrid grid)
        {
            Debug.WriteLine("[Dunnage_EditModeView] SelectFirstEditableCell: Starting");
            if (grid.ItemsSource is IList items && items.Count > 0)
            {
                // Find first non-readonly column
                var firstEditable = grid.Columns.OrderBy(c => c.DisplayIndex).FirstOrDefault(c => !c.IsReadOnly);
                if (firstEditable != null)
                {
                    Debug.WriteLine($"[Dunnage_EditModeView] SelectFirstEditableCell: Found editable column {firstEditable.Header}. Setting CurrentColumn and calling BeginEdit.");
                    grid.CurrentColumn = firstEditable;
                    grid.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
                    grid.BeginEdit();
                    Debug.WriteLine($"[Dunnage_EditModeView] SelectFirstEditableCell: BeginEdit called for column {firstEditable.Header}");
                }
                else
                {
                    Debug.WriteLine("[Dunnage_EditModeView] SelectFirstEditableCell: No editable column found.");
                }
            }
            else
            {
                var itemCount = (grid.ItemsSource as IList)?.Count ?? 0;
                Debug.WriteLine($"[Dunnage_EditModeView] SelectFirstEditableCell: Grid has no items (Count={itemCount})");
            }
        }
    }
}
