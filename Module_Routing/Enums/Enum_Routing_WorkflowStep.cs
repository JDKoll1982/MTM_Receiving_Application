namespace MTM_Receiving_Application.Module_Routing.Enums;

/// <summary>
/// Workflow steps for the routing label workflow
/// </summary>
public enum Enum_Routing_WorkflowStep
{
    /// <summary>
    /// Mode selection screen to choose between Wizard, Manual, or History
    /// </summary>
    ModeSelection = -1,

    /// <summary>
    /// Label entry screen with data grid for creating routing labels (Manual Mode)
    /// </summary>
    LabelEntry = 0,

    /// <summary>
    /// Wizard mode for step-by-step label creation
    /// </summary>
    Wizard = 10,

    /// <summary>
    /// Review screen to verify label data before printing
    /// </summary>
    Review = 1,

    /// <summary>
    /// Print/export screen for generating CSV and printing labels
    /// </summary>
    Print = 2,

    /// <summary>
    /// History view showing archived labels grouped by date
    /// </summary>
    History = 3
}
