using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MTM_Receiving_Application.Module_Reconciliation.Models;

/// <summary>
/// One row of the Needs Attention window: the unresolved line, its evidence, and the
/// location field the operator types into (FR-047).
/// </summary>
public sealed partial class Model_ReconciliationAttentionItem : ObservableObject
{
    /// <summary>The location the operator is setting for this line.</summary>
    [ObservableProperty]
    private string _locationInput = string.Empty;

    /// <summary>The unresolved finding.</summary>
    public Model_ReconciliationFinding Finding { get; init; } = null!;

    /// <summary>The command that writes this row's location.</summary>
    public IAsyncRelayCommand SubmitCommand { get; set; } = null!;

    /// <summary>Why the line could not be answered.</summary>
    public string Reason => Finding.Reason;

    /// <summary>The movement evidence behind the line.</summary>
    public string Evidence => Finding.Bucket?.EvidenceLocations ?? string.Empty;

    /// <summary>The raw difference where one applies.</summary>
    public string DifferenceText => Finding.DifferenceText;

    /// <summary>The line's part and purchase order, as the row's heading shows them.</summary>
    public string LineHeader =>
        $"{Finding.Row.PartId} · {Finding.Row.PurchaseOrderNumber} · {Finding.Row.Quantity}";
}
