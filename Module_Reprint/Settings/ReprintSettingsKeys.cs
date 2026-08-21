namespace MTM_Receiving_Application.Module_Reprint.Settings;

/// <summary>
/// Settings keys used by the Reprint Labels pages. The Search By column choice for each module
/// is persisted per user in <c>settings_personal</c> via the settings-core facade.
/// </summary>
public static class ReprintSettingsKeys
{
    public const string Category = "Reprint";

    public static class UserPreferences
    {
        public const string ReceivingSearchBy = "UserPreferences.ReceivingSearchBy";
        public const string DunnageSearchBy = "UserPreferences.DunnageSearchBy";
        public const string VolvoSearchBy = "UserPreferences.VolvoSearchBy";
        public const string ReceivingColumns = "UserPreferences.ReceivingColumns";
        public const string DunnageColumns = "UserPreferences.DunnageColumns";
        public const string VolvoColumns = "UserPreferences.VolvoColumns";
    }
}
