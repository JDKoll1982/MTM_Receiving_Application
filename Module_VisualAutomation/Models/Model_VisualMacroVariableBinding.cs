using MTM_Receiving_Application.Module_VisualAutomation.Enums;

namespace MTM_Receiving_Application.Module_VisualAutomation.Models;

/// <summary>
/// What a macro variable really is: which table and column backs it, and how safe it is to write.
/// </summary>
/// <remarks>
/// Derived from the live VMRCVENT process memory, read-only queries against MTMFG, and the VISUAL API
/// Toolkit documentation. See docs/development/InforVisual-Macro-Variable-Reference.md.
/// </remarks>
public sealed class Model_VisualMacroVariableBinding
{
    /// <summary>Gets the macro variable name.</summary>
    public required string Key { get; init; }

    /// <summary>Gets the plain-language meaning of the variable.</summary>
    public required string Meaning { get; init; }

    /// <summary>Gets the backing MTMFG table, or an empty string when nothing is persisted.</summary>
    public string Table { get; init; } = string.Empty;

    /// <summary>Gets the backing MTMFG column, or an empty string when nothing is persisted.</summary>
    public string Column { get; init; } = string.Empty;

    /// <summary>Gets the MTMFG data type, or an empty string when nothing is persisted.</summary>
    public string DataType { get; init; } = string.Empty;

    /// <summary>Gets the safety class of this variable.</summary>
    public required Enum_VisualVariableSafety Safety { get; init; }

    /// <summary>Gets the fully qualified backing column, for display.</summary>
    public string BackingColumn =>
        Table.Length == 0 ? "(not persisted)" : $"{Table}.{Column}";
}
