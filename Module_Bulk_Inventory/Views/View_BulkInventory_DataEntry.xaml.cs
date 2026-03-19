using System;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Bulk_Inventory.Models;
using MTM_Receiving_Application.Module_Bulk_Inventory.ViewModels;
using Windows.System;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.Views;

/// <summary>
/// Grid-based data entry screen for bulk inventory transactions.
/// Users add rows and then submit them all to Infor Visual via UI Automation.
/// </summary>
public sealed partial class View_BulkInventory_DataEntry : UserControl
{
    public ViewModel_BulkInventory_DataEntry ViewModel { get; }

    public View_BulkInventory_DataEntry(ViewModel_BulkInventory_DataEntry viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    /// <summary>
    /// Parameterless constructor for XAML instantiation.
    /// Uses Service Locator temporarily — constructor injection is preferred when navigating programmatically.
    /// </summary>
    public View_BulkInventory_DataEntry()
    {
        var viewModel = App.GetService<ViewModel_BulkInventory_DataEntry>();
        if (viewModel is null)
        {
            throw new InvalidOperationException(
                "ViewModel_BulkInventory_DataEntry could not be resolved from the DI container. " +
                "Ensure the ViewModel and all its dependencies are registered in ModuleServicesExtensions.cs");
        }

        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.XamlRoot = XamlRoot;
    }

    private void DeleteRowButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Model_BulkInventoryTransaction row)
        {
            ViewModel.DeleteRowCommand.Execute(row);
        }
    }

    /// <summary>
    /// Fires after the user commits an edit in any cell.
    /// <list type="bullet">
    ///   <item>Triggers per-cell validation (exact-match + fuzzy fallback) for Part ID and Location columns.</item>
    ///   <item>Auto-saves the row to MySQL: inserts new rows, updates existing ones, removes cleared rows.</item>
    /// </list>
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Cell edit event arguments.</param>
    private async void BulkInventoryDataGrid_CellEditEnded(object sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit)
            return;

        if (e.Row.DataContext is not Model_BulkInventoryTransaction row)
            return;

        var tag = e.Column.Tag as string;

        // Validate Part ID and Location fields first so the row is corrected before saving.
        if (tag is "PartId" or "FromLocation" or "ToLocation")
            await ViewModel.ValidateFieldAsync(row, tag);

        // Persist the row (insert / update / delete) based on current data state.
        await ViewModel.SaveOrRemoveRowAsync(row);
    }

    /// <summary>
    /// Keyboard shortcuts for the Data Entry view:
    /// F5 = Push Batch (if enabled), F6 = Skip Row (stub), Escape = Clear All (with confirm).
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Key event arguments.</param>
    private void UserControl_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.F5:
                if (ViewModel.PushBatchCommand.CanExecute(null))
                {
                    ViewModel.PushBatchCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case VirtualKey.F6:
                ViewModel.SkipCurrentRowCommand.Execute(null);
                e.Handled = true;
                break;

            case VirtualKey.Escape:
                if (ViewModel.ClearAllCommand.CanExecute(null))
                {
                    ViewModel.ClearAllCommand.Execute(null);
                    e.Handled = true;
                }
                break;
        }
    }
}
