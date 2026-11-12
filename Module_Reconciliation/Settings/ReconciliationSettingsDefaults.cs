using System.Collections.Generic;
using System.Globalization;

namespace MTM_Receiving_Application.Module_Reconciliation.Settings;

/// <summary>
/// The module's setting defaults. A missing value falls back here rather than
/// failing the day (FR-059).
/// </summary>
public static class ReconciliationSettingsDefaults
{
    /// <summary>The movement discovery window used when no value is stored.</summary>
    public const int MovementDiscoveryWindowDays = 2;

    /// <summary>The strict validation window used when no value is stored.</summary>
    public const int StrictValidationWindowDays = 1;

    /// <summary>Every setting default, keyed by its full settings key.</summary>
    public static IReadOnlyDictionary<string, string> StringDefaults { get; } =
        new Dictionary<string, string>
        {
            [ReconciliationSettingsKeys.MovementDiscoveryWindowDays] =
                MovementDiscoveryWindowDays.ToString(CultureInfo.InvariantCulture),
            [ReconciliationSettingsKeys.StrictValidationWindowDays] =
                StrictValidationWindowDays.ToString(CultureInfo.InvariantCulture),
        };
}
