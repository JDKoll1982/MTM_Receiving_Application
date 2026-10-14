namespace MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

/// <summary>
/// What the resolver does when a computed marking is already held by another live lot.
/// </summary>
public enum Enum_LotMarkingCollisionPolicy
{
    /// <summary>
    /// Accept the computed marking even if another lot already holds it.
    /// </summary>
    Allow,

    /// <summary>
    /// Advance deterministically to the first marking no other live lot holds. The default.
    /// </summary>
    Probe,

    /// <summary>
    /// Accept the computed marking but let the tool surface the collision for a user to fix.
    /// </summary>
    Warn,
}
