namespace MTM_Receiving_Application.Module_Dunnage.Models;

/// <summary>
/// Counts of rows that a dunnage-type delete will cascade-remove.
/// Returned by sp_Dunnage_Types_GetDeleteImpact and used to build the
/// delete-impact warning shown before a type is deleted.
/// </summary>
public sealed class Model_DunnageTypeDeleteImpact
{
    /// <summary>Parts that belong to the type (each cascades its own rows).</summary>
    public int PartsCount { get; set; }

    /// <summary>Active label-data queue rows referencing the type or its parts.</summary>
    public int LabelDataCount { get; set; }

    /// <summary>Archived history rows referencing the type or its parts.</summary>
    public int HistoryCount { get; set; }

    public bool HasImpact => PartsCount > 0 || LabelDataCount > 0 || HistoryCount > 0;
}
