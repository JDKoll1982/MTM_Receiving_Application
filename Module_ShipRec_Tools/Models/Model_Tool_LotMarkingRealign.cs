using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One distinct group of receiving rows: a heat and supplier pair, the marking those rows hold, and
/// how many rows there are. Read from both the queue and history, and returned with the heat exactly
/// as captured, because the matching key is produced by the application.
/// </summary>
/// <param name="Source">Where the rows live, <c>queue</c> or <c>history</c>.</param>
/// <param name="Heat">The heat value exactly as captured, empty when the row carries no lot number.</param>
/// <param name="SupplierName">The supplier value exactly as captured, empty when the receipt named none.</param>
/// <param name="PartId">The part number those rows carry, empty when they carry none. One lot appears once per part that carries it, so the parts a lot belongs to are read from these rows rather than from the marking.</param>
/// <param name="ShapeCode">The shape those rows show, empty when they carry no marking.</param>
/// <param name="ColorCode">The color those rows show, empty when they carry no marking.</param>
/// <param name="RowCount">How many rows this group covers.</param>
/// <param name="LastLineDate">The newest receipt date in the group, or the time the newest line was written when it has no receipt date.</param>
public sealed record Model_Tool_ReceivingHeatLotKey(
    string Source,
    string Heat,
    string SupplierName,
    string PartId,
    string ShapeCode,
    string ColorCode,
    int RowCount,
    DateTime? LastLineDate
);

/// <summary>
/// One raw spelling of a lot, exactly as the receiving rows carry it. The matching key collapses
/// spellings, so one lot can need writing under several of them.
/// </summary>
/// <param name="Heat">The heat value exactly as captured.</param>
/// <param name="SupplierName">The supplier value exactly as captured, empty when the receipt named none.</param>
/// <param name="RowCount">How many rows carry this spelling.</param>
public sealed record Model_Tool_LotMarkingRealignSpelling(
    string Heat,
    string SupplierName,
    int RowCount
);

/// <summary>
/// One lot the run will change, and what it will do about it. Heat and supplier are the first spelling
/// the lot was seen under, held exactly as captured because those are what the report shows.
/// </summary>
/// <param name="Action">Whether the rows take a recorded marking, take a new lot's marking, or lose theirs.</param>
/// <param name="Heat">The heat value as first captured, or empty when the number was cleared.</param>
/// <param name="SupplierName">The supplier value as first captured, empty when the receipt named none.</param>
/// <param name="Spellings">Every raw spelling of this lot that has to be written.</param>
/// <param name="ShapeCode">The shape the rows will show. Empty when the marking is being removed.</param>
/// <param name="ColorCode">The color the rows will show. Empty when the marking is being removed.</param>
/// <param name="PreviousShapeCode">The shape the rows show now, for the report.</param>
/// <param name="PreviousColorCode">The color the rows show now, for the report.</param>
public sealed record Model_Tool_LotMarkingRealignTarget(
    Enum_LotMarkingRealignAction Action,
    string Heat,
    string SupplierName,
    IReadOnlyList<Model_Tool_LotMarkingRealignSpelling> Spellings,
    string ShapeCode,
    string ColorCode,
    string PreviousShapeCode,
    string PreviousColorCode
)
{
    /// <summary>How many rows this entry covers, across every spelling of the lot.</summary>
    public int RowCount => Spellings.Sum(spelling => spelling.RowCount);
}

/// <summary>
/// One lot the run would remove, because no receiving row carries its number any more. Removing it
/// frees its shape and color for a later lot.
/// </summary>
/// <param name="Id">The lot's id.</param>
/// <param name="HeatNumber">The lot number as it was first captured.</param>
/// <param name="SupplierId">The supplier group the lot belongs to.</param>
/// <param name="ShapeCode">The shape being freed.</param>
/// <param name="ColorCode">The color being freed.</param>
public sealed record Model_Tool_LotMarkingRealignRemoval(
    int Id,
    string HeatNumber,
    string SupplierId,
    string ShapeCode,
    string ColorCode
);

/// <summary>
/// A lot the run will not touch, with the reason, so a person can deal with it by hand.
/// </summary>
/// <param name="HeatNumber">The lot number as it was captured.</param>
/// <param name="SupplierId">The supplier group the lot belongs to.</param>
/// <param name="Reason">Why the lot was left alone.</param>
public sealed record Model_Tool_LotMarkingRealignSkip(
    string HeatNumber,
    string SupplierId,
    string Reason
);

/// <summary>
/// Everything Update All Lots would do, worked out and shown before it does any of it.
/// </summary>
/// <param name="Targets">The row groups that need changing.</param>
/// <param name="Removals">The lots whose number no longer appears on any row.</param>
/// <param name="Skips">The lots left alone, and why.</param>
/// <param name="LotsAlreadyCorrect">How many lots were already in agreement, so an empty result is not mistaken for a failure.</param>
public sealed record Model_Tool_LotMarkingRealignPreview(
    IReadOnlyList<Model_Tool_LotMarkingRealignTarget> Targets,
    IReadOnlyList<Model_Tool_LotMarkingRealignRemoval> Removals,
    IReadOnlyList<Model_Tool_LotMarkingRealignSkip> Skips,
    int LotsAlreadyCorrect
)
{
    /// <summary>How many rows would be written.</summary>
    public int RowsToChange => Targets.Sum(target => target.RowCount);

    /// <summary>How many rows would lose their marking.</summary>
    public int RowsToClear =>
        Targets
            .Where(target => target.Action == Enum_LotMarkingRealignAction.ClearMarking)
            .Sum(target => target.RowCount);

    /// <summary>How many lot numbers would become lots.</summary>
    public int LotsToCreate =>
        Targets.Count(target => target.Action == Enum_LotMarkingRealignAction.CreateLot);

    /// <summary>How many lots would be removed from the list.</summary>
    public int LotsToRemove => Removals.Count;

    /// <summary><c>true</c> when the run would change anything at all.</summary>
    public bool HasWork => Targets.Count > 0 || Removals.Count > 0;
}

/// <summary>
/// What Update All Lots actually did.
/// </summary>
/// <param name="RowsChanged">How many rows were given a different marking.</param>
/// <param name="RowsCleared">How many rows lost their marking.</param>
/// <param name="LotsCreated">How many lot numbers became lots.</param>
/// <param name="LotsRemoved">How many lots were removed from the list.</param>
/// <param name="LotsSkipped">How many lots were left alone.</param>
public sealed record Model_Tool_LotMarkingRealignResult(
    int RowsChanged,
    int RowsCleared,
    int LotsCreated,
    int LotsRemoved,
    int LotsSkipped
);
