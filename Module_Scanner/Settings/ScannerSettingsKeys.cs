namespace MTM_Receiving_Application.Module_Scanner.Settings;

/// <summary>
/// Scanner settings key container. Scanner access is intentionally system-scoped so one
/// allow-list covers the whole plant instead of being configured per workstation or per user.
/// </summary>
public static class ScannerSettingsKeys
{
    /// <summary>Settings category that owns Scanner-scoped definitions.</summary>
    public const string Category = "Scanner";

    public static class Access
    {
        /// <summary>
        /// System-scoped JSON array of employee numbers allowed to use the Scanner module.
        /// An empty array means the allow-list has not been configured yet, in which case the
        /// legacy developer-only rollout rule applies.
        /// </summary>
        public const string AllowedEmployeeNumbers = "Scanner.Access.AllowedEmployeeNumbers";

        /// <summary>Default value written when the allow-list has never been saved.</summary>
        public const string DefaultAllowedEmployeeNumbers = "[]";
    }

    public static class Automation
    {
        /// <summary>
        /// User-scoped flag for Auto Mode: press Alt+S as the automation's last step and chain
        /// straight to the next line when the operator answers Yes to the save prompt.
        /// </summary>
        public const string AutoModeEnabled = "Scanner.Automation.AutoModeEnabled";

        /// <summary>Auto Mode starts off, so it is only ever on because an operator turned it on.</summary>
        public const string DefaultAutoModeEnabled = "false";
    }
}
