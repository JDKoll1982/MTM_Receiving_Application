namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// Mock-mode selectable location row used by the Customer Pull n' Pack report workflow.
/// </summary>
public class Model_InforVisualCustomerPullPackLocationRow
{
    public string LocationKey { get; set; } = string.Empty;

    public string SourceLineKey { get; set; } = string.Empty;

    public string ParentPartId { get; set; } = string.Empty;

    public string LocationId { get; set; } = string.Empty;

    public string DisplayLabel { get; set; } = string.Empty;

    public decimal OnHandQuantity { get; set; }

    public string SourceType { get; set; } = string.Empty;

    public bool InitiallySelected { get; set; }
}
