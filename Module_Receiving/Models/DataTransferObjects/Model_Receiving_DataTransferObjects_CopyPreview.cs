namespace MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;

/// <summary>
/// Data Transfer Object for bulk copy preview information.
/// Shows before/after values for fields being copied.
/// </summary>
public class Model_Receiving_DataTransferObjects_CopyPreview
{
    /// <summary>
    /// Load number being affected by the copy operation.
    /// </summary>
    public int LoadNumber { get; set; }

    /// <summary>
    /// Name of the field being copied.
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Current value before copy (may be empty).
    /// </summary>
    public string CurrentValue { get; set; } = string.Empty;

    /// <summary>
    /// New value after copy.
    /// </summary>
    public string NewValue { get; set; } = string.Empty;
}
