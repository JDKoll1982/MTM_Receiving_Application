namespace MTM_Receiving_Application.Module_Receiving.Models.Enums;

/// <summary>
/// Defines the available workflow modes in Module_Receiving
/// </summary>
public enum Enum_Receiving_Mode_WorkflowMode
{
    /// <summary>
    /// Guided Mode: 3-step wizard workflow for standard receiving
    /// </summary>
    Wizard = 1,

    /// <summary>
    /// Manual Mode: Grid-based bulk entry for high-volume receiving
    /// </summary>
    Manual = 2,

    /// <summary>
    /// Edit Mode: Search and modify historical receiving transactions
    /// </summary>
    Edit = 3
}
