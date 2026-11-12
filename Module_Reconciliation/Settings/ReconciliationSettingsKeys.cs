namespace MTM_Receiving_Application.Module_Reconciliation.Settings;

/// <summary>
/// Settings keys owned by the reconciliation module. Every key carries the
/// module's own <c>Reconciliation.</c> prefix and no <c>Receiving.</c> key is
/// read or written (FR-059, FR-060).
/// </summary>
public static class ReconciliationSettingsKeys
{
    /// <summary>Days either side of the receipt date used to discover movements.</summary>
    public const string MovementDiscoveryWindowDays =
        "Reconciliation.BusinessRules.MovementDiscoveryWindowDays";

    /// <summary>Days after the receipt date used for strict validation.</summary>
    public const string StrictValidationWindowDays =
        "Reconciliation.BusinessRules.StrictValidationWindowDays";

    /// <summary>The settings category the module stores under.</summary>
    public const string Category = "Reconciliation";
}
