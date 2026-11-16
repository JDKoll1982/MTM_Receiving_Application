namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// How a batch edit value has to be read before it is written onto a row.
/// </summary>
public enum Enum_EditModeBatchFieldKind
{
    /// <summary>Kept as typed.</summary>
    Text = 0,

    /// <summary>A whole number, such as packages per load.</summary>
    Integer = 1,

    /// <summary>A number that may carry decimals, such as a weight.</summary>
    Decimal = 2,

    /// <summary>One of the package type names the application knows.</summary>
    PackageType = 3,
}

/// <summary>
/// One field a clerk can set across every ticked row in Edit Mode.
/// </summary>
/// <remarks>
/// The list is deliberately short. It offers only the fields that are actually corrected in bulk -
/// a wrong location, a missing heat number, a package count - and leaves the identifiers and the
/// amounts that belong to the receipt itself alone.
/// </remarks>
public sealed class Model_EditModeBatchField
{
    public required string Key { get; init; }

    public required string Label { get; init; }

    public required Enum_EditModeBatchFieldKind Kind { get; init; }

    /// <summary>What to type, shown as the box's placeholder.</summary>
    public string Hint { get; init; } = string.Empty;

    /// <summary>
    /// The label. A combo box otherwise announces the type name, which tells the clerk nothing.
    /// </summary>
    public override string ToString() => Label;
}
