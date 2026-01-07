namespace MTM_Receiving_Application.Module_Routing.Constants;

/// <summary>
/// Constants for Routing Module
/// Fixes Issue #10: Magic string "OTHER" scattered across codebase
/// </summary>
public static class Constant_Routing
{
    /// <summary>
    /// Special PO number value indicating non-PO package
    /// </summary>
    public const string OtherPoNumber = "OTHER";

    /// <summary>
    /// Line number used for OTHER packages
    /// </summary>
    public const string OtherLineNumber = "0";
}
