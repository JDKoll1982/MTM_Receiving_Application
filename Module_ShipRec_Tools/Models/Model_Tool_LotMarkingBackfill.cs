namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One row of the unmarked-lot preview: a heat value as it was captured, the supplier it came
/// from, and how many unmarked receiving rows that pair covers.
/// </summary>
/// <param name="Source">Where the rows live, <c>queue</c> or <c>history</c>.</param>
/// <param name="Heat">The heat value exactly as it was captured. Normalization belongs to the service.</param>
/// <param name="SupplierName">The supplier the receipt named, or empty when it named none.</param>
/// <param name="RowCount">How many unmarked rows this pair covers.</param>
public sealed record Model_Tool_UnmarkedLotRow(
    string Source,
    string Heat,
    string SupplierName,
    int RowCount
);

/// <summary>
/// What filling in the unmarked lots of one part would cover, reported before anything is written.
/// </summary>
/// <param name="DistinctLots">
/// How many lots would be marked. Two spellings of one heat number count once, and a row with no
/// heat number is not a lot at all.
/// </param>
/// <param name="TotalRows">How many receiving rows those lots cover.</param>
public sealed record Model_Tool_LotMarkingBackfillPreview(int DistinctLots, int TotalRows);
