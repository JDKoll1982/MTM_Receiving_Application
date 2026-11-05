namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// One card on a cut-out sheet: the part and lot it belongs to, and the marking to draw on it.
/// </summary>
/// <param name="PartId">The part number, shown across the top of the card.</param>
/// <param name="LotNumber">The heat or lot number, shown across the bottom of the card.</param>
/// <param name="ShapeCode">The shape glyph recorded for the lot.</param>
/// <param name="ColorCode">The palette color code recorded for the lot.</param>
public sealed record Model_LabelOverlayCard(
    string PartId,
    string LotNumber,
    string ShapeCode,
    string ColorCode
);
