namespace MTM_Receiving_Application.Module_Receiving.Models.Enums;

/// <summary>
/// Defines the workflow steps for the Guided Mode (3-step wizard)
/// </summary>
public enum Enum_Receiving_State_WorkflowStep
{
    /// <summary>
    /// Step 1: Order &amp; Part Selection (PO Number, Part Number, Load Count)
    /// </summary>
    OrderAndPartSelection = 1,

    /// <summary>
    /// Step 2: Load Details Entry (Weight, Heat Lot, Package Type, etc.)
    /// </summary>
    LoadDetailsEntry = 2,

    /// <summary>
    /// Step 3: Review &amp; Save (Review summary, Save, Complete)
    /// </summary>
    ReviewAndSave = 3
}
