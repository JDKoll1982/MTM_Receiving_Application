using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Bulk_Inventory.Models;
using MTM_Receiving_Application.Module_Bulk_Inventory.ViewModels;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.Views;

/// <summary>
/// Top-level host page for the Bulk Inventory module.
/// Manages navigation between the DataEntry, Push (overlay), and Summary child pages
/// by replacing the content of the inner <see cref="ContentFrame"/>.
/// </summary>
public sealed partial class View_BulkInventory_Host : Page
{
    private readonly IServiceProvider _services;

    public View_BulkInventory_Host(IServiceProvider services)
    {
        _services = services;
        InitializeComponent();
        Loaded += (_, _) => ShowDataEntry();
    }

    // ── Navigation helpers ────────────────────────────────────────────────────

    private void ShowDataEntry()
    {
        var vm = _services.GetRequiredService<ViewModel_BulkInventory_DataEntry>();
        var page = new View_BulkInventory_DataEntry(vm);
        vm.XamlRoot = XamlRoot;

        vm.RequestNavigateToPush = rows => ShowPush(rows);

        // Load existing rows and trigger crash-recovery banner if needed (T030).
        _ = vm.LoadPendingRowsAsync();

        ContentFrame.Content = page;
    }

    private void ShowPush(ObservableCollection<Model_BulkInventoryTransaction> rows)
    {
        var vm = _services.GetRequiredService<ViewModel_BulkInventory_Push>();
        var page = new View_BulkInventory_Push { ViewModel = vm };

        vm.RequestNavigateToSummary = completedRows => ShowSummary(completedRows);

        ContentFrame.Content = page;

        // Kick off the automation loop immediately.
        _ = vm.StartPushCommand.ExecuteAsync(rows);
    }

    private void ShowSummary(IReadOnlyList<Model_BulkInventoryTransaction> completedRows)
    {
        var vm = _services.GetRequiredService<ViewModel_BulkInventory_Summary>();
        var page = new View_BulkInventory_Summary { ViewModel = vm };

        vm.LoadResults(completedRows);
        vm.RequestNavigateToDataEntry = ShowDataEntry;

        ContentFrame.Content = page;
    }
}
