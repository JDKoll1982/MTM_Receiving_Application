namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// A single labeled value (custom UDC field) rendered inside a dunnage book card.
/// </summary>
public class Model_Tool_DunnageBook_FieldValue
{
    public string Label { get; set; } = string.Empty;

    public string? Value { get; set; }
}
