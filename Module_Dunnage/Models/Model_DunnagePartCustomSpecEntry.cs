namespace MTM_Receiving_Application.Module_Dunnage.Models;

/// <summary>
/// Editable key/value pair used for part-specific specs in the add and edit part dialogs.
/// </summary>
public class Model_DunnagePartCustomSpecEntry
{
    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
