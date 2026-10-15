namespace MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

/// <summary>
/// What Update All Lots does about one group of marked receiving rows that share a heat number and a
/// supplier.
/// </summary>
public enum Enum_LotMarkingRealignAction
{
    /// <summary>
    /// The lot number the rows now carry already has a marking, so the rows are changed to show it.
    /// </summary>
    ApplyRecordedMarking,

    /// <summary>
    /// The lot number the rows now carry has no marking, so it becomes a lot and the rows are changed
    /// to show the shape and color it is given.
    /// </summary>
    CreateLot,

    /// <summary>
    /// The lot number was cleared, so the rows lose their marking. A line with no lot number is not a
    /// lot and must not carry a lot's marking.
    /// </summary>
    ClearMarking,
}
