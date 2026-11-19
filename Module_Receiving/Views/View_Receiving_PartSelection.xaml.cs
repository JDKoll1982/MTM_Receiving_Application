using System;
using System.ComponentModel;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views;

/// <summary>
/// The guided "Select part" step. Everything it shows comes from
/// <see cref="ViewModel_Receiving_PartSelection"/>; the code here only moves focus and mirrors the
/// view model's sort order onto the grid's header arrows.
/// </summary>
public sealed partial class View_Receiving_PartSelection : UserControl, IReceivingWorkflowFocusable
{
    private readonly IService_Focus _focusService;

    public View_Receiving_PartSelection(
        ViewModel_Receiving_PartSelection viewModel,
        IService_Focus focusService
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(focusService);

        ViewModel = viewModel;
        _focusService = focusService;
        InitializeComponent();

        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        SyncSortIndicator();
    }

    public ViewModel_Receiving_PartSelection ViewModel { get; }

    /// <summary>
    /// Puts the cursor in the search box, so a scan gun's next trigger fills it and does not have to
    /// be aimed at the right field first.
    /// </summary>
    public bool FocusForAccess() => _focusService.TrySetFocus(PartSearchBox);

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (
            e.PropertyName
            is nameof(ViewModel_Receiving_PartSelection.SortColumn)
                or nameof(ViewModel_Receiving_PartSelection.SortAscending)
        )
        {
            SyncSortIndicator();
        }
    }

    private void PartsGrid_Sorting(object sender, DataGridColumnEventArgs e)
    {
        if (e.Column.Tag is not string columnKey || string.IsNullOrWhiteSpace(columnKey))
        {
            return;
        }

        // A column with no arrow yet sorts ascending on the first click; clicking it again flips it.
        var ascending = e.Column.SortDirection is null or DataGridSortDirection.Descending;

        ViewModel.SortBy(columnKey, ascending);
        SyncSortIndicator();
    }

    private void SyncSortIndicator()
    {
        foreach (var column in PartsGrid.Columns)
        {
            column.SortDirection =
                column.Tag as string == ViewModel.SortColumn
                    ? (
                        ViewModel.SortAscending
                            ? DataGridSortDirection.Ascending
                            : DataGridSortDirection.Descending
                    )
                    : null;
        }
    }

    /// <summary>
    /// Treats a double-click on a part as "this one, next page". The first click of the pair has
    /// already chosen the row, so the command only has to carry the walk forward.
    /// </summary>
    private void PartsGrid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (!IsInsideARow(e.OriginalSource))
        {
            return;
        }

        e.Handled = true;

        if (ViewModel.SelectPartAndAdvanceCommand.CanExecute(null))
        {
            ViewModel.SelectPartAndAdvanceCommand.Execute(null);
        }
    }

    private static bool IsInsideARow(object? originalSource)
    {
        for (
            var current = originalSource as DependencyObject;
            current is not null;
            current = VisualTreeHelper.GetParent(current)
        )
        {
            if (current is DataGridRow)
            {
                return true;
            }

            if (current is DataGrid)
            {
                return false;
            }
        }

        return false;
    }
}
