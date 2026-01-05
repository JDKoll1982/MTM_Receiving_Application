namespace MTM_Receiving_Application.Module_Routing.Enums;

/// <summary>
/// Enumeration of routing module operating modes
/// </summary>
public enum Enum_RoutingMode
{
    /// <summary>
    /// Wizard mode: Step-by-step guided workflow
    /// </summary>
    WIZARD,

    /// <summary>
    /// Manual entry mode: Grid-based batch entry
    /// </summary>
    MANUAL,

    /// <summary>
    /// Edit mode: Search and edit existing labels
    /// </summary>
    EDIT
}
