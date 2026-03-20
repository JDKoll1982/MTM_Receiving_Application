using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Bulk_Inventory.Enums;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.Models;

/// <summary>
/// Represents one row in the bulk_inventory_transactions MySQL table.
/// Maps 1-to-1 with the schema created by T004.
/// Implements <see cref="ObservableObject"/> so that x:Bind DataTemplate bindings
/// (Mode=TwoWay / OneWay) propagate correctly when fields are updated programmatically
/// by the validation or push-automation layers.
/// </summary>
public partial class Model_BulkInventoryTransaction : ObservableObject
{
    public int Id { get; set; }

    /// <summary>MTM application username of the person who created this batch row.</summary>
    public string CreatedByUser { get; set; } = string.Empty;

    /// <summary>Infor Visual username used to drive the automation session.</summary>
    public string VisualUsername { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TransactionTypeIndex))]
    private Enum_BulkInventoryTransactionType _transactionType;

    /// <summary>
    /// Zero-based index for ComboBox binding: 0 = Transfer, 1 = NewTransaction.
    /// Avoids enum-to-string type mismatch in TwoWay x:Bind on SelectedItem.
    /// </summary>
    public int TransactionTypeIndex
    {
        get => TransactionType == Enum_BulkInventoryTransactionType.Transfer ? 0 : 1;
        set =>
            TransactionType =
                value == 0
                    ? Enum_BulkInventoryTransactionType.Transfer
                    : Enum_BulkInventoryTransactionType.NewTransaction;
    }

    [ObservableProperty]
    private string _partId = string.Empty;

    /// <summary>Warehouse code for the source location (Transfer mode only).</summary>
    [ObservableProperty]
    private string? _fromWarehouse;

    /// <summary>Location code for the source (Transfer mode only).</summary>
    [ObservableProperty]
    private string? _fromLocation;

    /// <summary>Warehouse code for the destination.</summary>
    [ObservableProperty]
    private string _toWarehouse = string.Empty;

    /// <summary>Location code for the destination.</summary>
    [ObservableProperty]
    private string _toLocation = string.Empty;

    [ObservableProperty]
    private decimal _quantity;

    /// <summary>Work order number in Visual format — e.g. WO-123456 (NewTransaction mode only).</summary>
    public string? WorkOrder { get; set; }

    /// <summary>Lot number (NewTransaction mode only; defaults to "1").</summary>
    public string? LotNo { get; set; }

    [ObservableProperty]
    private Enum_BulkInventoryStatus _status = Enum_BulkInventoryStatus.Pending;

    /// <summary>Set when <see cref="Status"/> is <see cref="Enum_BulkInventoryStatus.Failed"/>.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Optional inline warning surfaced by ValidateAllCommand (does not block UI entry).
    /// Not persisted to the database.
    /// </summary>
    [ObservableProperty]
    private string? _validationMessage;

    /// <summary>
    /// UI-only flag: the user has checked this failed row for re-push in the Summary view.
    /// Not persisted to the database.
    /// </summary>
    public bool IsSelectedForRepush { get; set; }
}
