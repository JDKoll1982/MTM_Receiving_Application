using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Volvo.Settings;

public static class VolvoSettingsDefaults
{
    public static IReadOnlyDictionary<string, string> StringDefaults { get; } =
        new Dictionary<string, string> { [VolvoSettingsKeys.Labels.VolvoLabelPath] = string.Empty };
}
