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
}
