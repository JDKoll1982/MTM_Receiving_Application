namespace MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

/// <summary>
/// The column the lot list is ordered by. Each value is a question a handler asks of the list, so the
/// names describe the material rather than the field behind it.
/// </summary>
public enum Enum_LotMarkingSortField
{
    /// <summary>Newest receipt first: the lot that came in most recently.</summary>
    LastReceiptDate,

    /// <summary>The heat or lot number, read as text.</summary>
    HeatNumber,

    /// <summary>How many skids carry the lot, largest first.</summary>
    SkidCount,

    /// <summary>The supplier the lot was received from.</summary>
    Supplier,

    /// <summary>The part numbers the lot is carried by.</summary>
    PartNumber,

    /// <summary>The marking itself, by shape and then color.</summary>
    ShapeAndColor,
}
