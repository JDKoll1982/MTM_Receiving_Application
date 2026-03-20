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
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using Windows.System;
using Windows.UI.Core;

namespace MTM_Receiving_Application.Module_Dunnage.Views
{
    public sealed partial class View_Dunnage_EditModeView : UserControl
    {
        public ViewModel_Dunnage_EditMode ViewModel { get; }
        private readonly IService_Focus _focusService;

        public View_Dunnage_EditModeView()
        {
            ViewModel = App.GetService<ViewModel_Dunnage_EditMode>();
            _focusService = App.GetService<IService_Focus>();
            this.DataContext = ViewModel;
            this.InitializeComponent();

            _focusService.AttachFocusOnVisibility(this);
        }

        private void EditModeDataGrid_CurrentCellChanged(object? sender, EventArgs e)
        {
            var grid = sender as DataGrid;
            // Wait for the move to complete then activate edit mode
            grid?.DispatcherQueue.TryEnqueue(() =>
            {
                if (grid.CurrentColumn?.IsReadOnly == false)
                {
                    Debug.WriteLine(
                        $"[Dunnage_EditModeView] CurrentCellChanged: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}"
                    );
                    grid.BeginEdit();
                }
            });
        }

        // ------------------------------------------------------------------ column chooser
        private async void ColumnsButton_Click(object sender, RoutedEventArgs e)
        {
            // Build checkboxes from DataGrid columns, skipping the checkbox column (index 0)
            var panel = new StackPanel { Spacing = 6 };
            var checkBoxes = new List<(CheckBox Box, int Index)>();

            var quickRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Margin = new Thickness(0, 0, 0, 8),
            };
            var selectAllBtn = new Button { Content = "Select All" };
            var clearAllBtn = new Button { Content = "Clear All" };
            quickRow.Children.Add(selectAllBtn);
            quickRow.Children.Add(clearAllBtn);
            panel.Children.Add(quickRow);

            for (int i = 1; i < EditModeDataGrid.Columns.Count; i++)
            {
                var col = EditModeDataGrid.Columns[i];
                var header = col.Header?.ToString() ?? $"Column {i}";
                var cb = new CheckBox
                {
                    Content = header,
                    IsChecked = col.Visibility == Visibility.Visible,
                    Tag = i,
                };
                panel.Children.Add(cb);
                checkBoxes.Add((cb, i));
            }

            selectAllBtn.Click += (_, _) =>
            {
                foreach (var (cb, _) in checkBoxes)
                    cb.IsChecked = true;
            };
            clearAllBtn.Click += (_, _) =>
            {
                foreach (var (cb, _) in checkBoxes)
                    cb.IsChecked = false;
            };

            var dialog = new ContentDialog
            {
                Title = "Choose visible columns",
                Content = new ScrollViewer
                {
                    Content = panel,
                    MaxHeight = 400,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                },
                PrimaryButtonText = "Apply",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot,
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            foreach (var (cb, index) in checkBoxes)
            {
                EditModeDataGrid.Columns[index].Visibility =
                    cb.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
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
                $"[Dunnage_EditModeView] KeyDown: Key={e.Key}, Shift={isShiftDown}, OriginalSource={e.OriginalSource}, CurrentColumn={grid.CurrentColumn?.Header}"
            );
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
                Debug.WriteLine(
                    "[Dunnage_EditModeView] Tapped: SelectedItem is null (Header or empty space). Selecting first editable cell."
                );
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
                Debug.WriteLine(
                    "[Dunnage_EditModeView] Tapped: Cell clicked. Enqueuing BeginEdit."
                );
                grid.DispatcherQueue.TryEnqueue(() =>
                {
                    if (grid.CurrentColumn?.IsReadOnly == false)
                    {
                        Debug.WriteLine(
                            $"[Dunnage_EditModeView] Tapped: BeginEdit for Row={grid.SelectedIndex}, Col={grid.CurrentColumn.Header}"
                        );
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
                var firstEditable = grid
                    .Columns.OrderBy(c => c.DisplayIndex)
                    .FirstOrDefault(c => !c.IsReadOnly);
                if (firstEditable != null)
                {
                    Debug.WriteLine(
                        $"[Dunnage_EditModeView] SelectFirstEditableCell: Found editable column {firstEditable.Header}. Setting CurrentColumn and calling BeginEdit."
                    );
                    grid.CurrentColumn = firstEditable;
                    grid.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
                    grid.BeginEdit();
                    Debug.WriteLine(
                        $"[Dunnage_EditModeView] SelectFirstEditableCell: BeginEdit called for column {firstEditable.Header}"
                    );
                }
                else
                {
                    Debug.WriteLine(
                        "[Dunnage_EditModeView] SelectFirstEditableCell: No editable column found."
                    );
                }
            }
            else
            {
                var itemCount = (grid.ItemsSource as IList)?.Count ?? 0;
                Debug.WriteLine(
                    $"[Dunnage_EditModeView] SelectFirstEditableCell: Grid has no items (Count={itemCount})"
                );
            }
        }
    }
}
