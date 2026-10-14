namespace MTM_Receiving_Application.Module_ShipRec_Tools.Settings;

/// <summary>
/// Settings keys used by ShipRec Tools features.
/// </summary>
public static class ShipRecToolsSettingsKeys
{
    public const string Category = "ShipRecTools";

    public static class MaterialAvailability
    {
        public const string WorkOrderUiVisibleFieldIds =
            "MaterialAvailability.WorkOrderFields.UiVisibleIds";

        public const string WorkOrderPrintVisibleFieldIds =
            "MaterialAvailability.WorkOrderFields.PrintVisibleIds";

        public const string WorkOrderAllowShowAllChip =
            "MaterialAvailability.WorkOrderFields.AllowShowAllChip";
    }

    public static class POLineSpecSearch
    {
        public const string SearchMode = "POLineSpecSearch.Options.SearchMode";

        public const string PoStatusFilter = "POLineSpecSearch.Options.PoStatusFilter";

        public const string VisibleLines = "POLineSpecSearch.Options.VisibleLines";

        public const string VisibleColumnKeys = "POLineSpecSearch.Options.VisibleColumnKeys";
    }

    /// <summary>
    /// Heat / Lot Management. <see cref="Enabled"/> is the system-wide switch and is the only key
    /// in this category that is gated above a normal user; the rest may be changed by any user.
    /// </summary>
    public static class LotMarking
    {
        public const string Enabled = "LotMarking.Enabled";

        public const string CollisionPolicy = "LotMarking.CollisionPolicy";

        public const string PaletteEnabledShapes = "LotMarking.Palette.EnabledShapes";

        public const string PaletteEnabledColors = "LotMarking.Palette.EnabledColors";

        public const string DisplayShapeFirst = "LotMarking.Display.ShapeFirst";
    }
}
