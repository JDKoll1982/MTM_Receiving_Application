namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// One label on the receipt whose part number is on the list of parts that get the lot shape treatment,
/// so the printed tag still needs the colored symbol added over it.
/// </summary>
/// <param name="PartId">The part the label was printed for.</param>
/// <param name="HeatLotNumber">The heat or lot number on the label, empty when the receipt carried none.</param>
/// <param name="LabelNumber">The label's number within its load.</param>
/// <param name="MarkingText">
/// The shape and color already recorded for the lot, in words; empty while the marking has not been
/// resolved yet, which is the case until the receipt is saved.
/// </param>
public sealed record Model_ReceivingLotMarkingNeed(
    string PartId,
    string HeatLotNumber,
    int LabelNumber,
    string MarkingText
);
