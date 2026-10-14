namespace MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

/// <summary>
/// How a heat/lot marking came to exist.
/// </summary>
public enum Enum_LotMarkingSource
{
    /// <summary>
    /// Chosen by the resolver the first time the lot was seen. The normal case.
    /// </summary>
    Auto,

    /// <summary>
    /// Replaced by a user, which always records a reason and the acting user.
    /// </summary>
    Manual,

    /// <summary>
    /// Brought in from an import rather than resolved in the application.
    /// </summary>
    Import,
}
