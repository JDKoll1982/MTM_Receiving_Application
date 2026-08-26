using System.Collections.Generic;
using System.ComponentModel;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Contracts;

/// <summary>
/// Coordinates which placeholder scanner page is active inside the module host.
/// </summary>
public interface IService_ScannerNavigation : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the currently selected scanner page.
    /// </summary>
    Enum_ScannerPage CurrentPage { get; }

    /// <summary>
    /// Shows the workbench page.
    /// </summary>
    void ShowWorkbench();

    /// <summary>
    /// Shows the history page.
    /// </summary>
    void ShowHistory();

    /// <summary>
    /// Shows the settings page.
    /// </summary>
    void ShowSettings();

    /// <summary>
    /// Shows the Advanced Bulk Move page.
    /// </summary>
    void ShowAdvancedBulkMove();

    /// <summary>
    /// Gets or sets destination rows produced by the Advanced Bulk Move page. The workbench
    /// consumes this payload after it returns to the workbench page; cleared after consumption.
    /// </summary>
    IReadOnlyList<Model_ScannerBulkMoveDestination>? PendingBulkMoveDestinations { get; set; }
}