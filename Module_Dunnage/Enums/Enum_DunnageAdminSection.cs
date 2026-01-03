namespace MTM_Receiving_Application.Module_Dunnage.Enums;

/// <summary>
/// Enum for Dunnage Admin UI sections
/// </summary>
public enum Enum_DunnageAdminSection
{
    /// <summary>
    /// Main navigation hub (4-card view)
    /// </summary>
    Hub = 0,

    /// <summary>
    /// Dunnage Types management (CRUD on dunnage_types table)
    /// </summary>
    Types = 1,

    /// <summary>
    /// Part Master management (CRUD on dunnage_part_numbers table)
    /// </summary>
    Parts = 2,

    /// <summary>
    /// Spec Definitions management (CRUD on dunnage_specs table)
    /// </summary>
    Specs = 3,

    /// <summary>
    /// Inventoried Dunnage List (CRUD on inventoried_dunnage_list table)
    /// </summary>
    InventoriedList = 4
}
