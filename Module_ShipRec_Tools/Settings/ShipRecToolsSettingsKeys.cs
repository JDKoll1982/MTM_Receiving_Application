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
}
