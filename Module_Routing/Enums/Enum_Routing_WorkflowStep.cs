namespace MTM_Receiving_Application.Module_Routing.Enums;

/// <summary>
/// Workflow steps for the routing label workflow
/// </summary>
public enum Enum_Routing_WorkflowStep
{
    /// <summary>
    /// Label entry screen with data grid for creating routing labels
    /// </summary>
    LabelEntry = 0,

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
