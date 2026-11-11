namespace MTM_Receiving_Application.Module_VisualAutomation.Enums;

/// <summary>
/// How safe it is for automation to write a macro variable on the Purchase Receipt Entry window.
/// </summary>
public enum Enum_VisualVariableSafety
{
    /// <summary>Changes receipt identity or the vendor snapshot. Read, do not write.</summary>
    Identity = 0,

    /// <summary>System supplied at runtime. Read only.</summary>
    Runtime = 1,

    /// <summary>Free text on the receipt with no database consequence. Safest surface for automation.</summary>
    ReceiptOnly = 2,

    /// <summary>Updates the purchase order or the PO line when the receipt is posted.</summary>
    Consequential = 3,

    /// <summary>Form-level scratch slot that this module never persists.</summary>
    Scratch = 4,

    /// <summary>Written by a macro to report its own result back to the caller.</summary>
    MacroOutput = 5,
}
