using System.Globalization;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Module_Reconciliation.Services;

/// <summary>
/// Reads and writes the module's windows through the settings core facade under
/// the module's own <c>Reconciliation.</c> key prefix. A missing value returns
/// the module default rather than failing the day (FR-059).
/// </summary>
public sealed class Service_ReconciliationSettings : IService_ReconciliationSettings
{
    private readonly IService_SettingsCoreFacade _settings;

    /// <summary>Creates the service over the settings core facade.</summary>
    /// <param name="settings">The settings core facade.</param>
    public Service_ReconciliationSettings(IService_SettingsCoreFacade settings)
    {
        _settings = settings;
    }

    /// <inheritdoc />
    public async Task<int> GetDiscoveryWindowDaysAsync()
    {
        int value = await ReadDaysAsync(
            ReconciliationSettingsKeys.MovementDiscoveryWindowDays,
            ReconciliationSettingsDefaults.MovementDiscoveryWindowDays
        );
        return value;
    }

    /// <inheritdoc />
    public async Task<int> GetStrictWindowDaysAsync()
    {
        int value = await ReadDaysAsync(
            ReconciliationSettingsKeys.StrictValidationWindowDays,
            ReconciliationSettingsDefaults.StrictValidationWindowDays
        );
        return value;
    }

    /// <inheritdoc />
    public Task SetDiscoveryWindowDaysAsync(int days) =>
        WriteDaysAsync(ReconciliationSettingsKeys.MovementDiscoveryWindowDays, days);

    /// <inheritdoc />
    public Task SetStrictWindowDaysAsync(int days) =>
        WriteDaysAsync(ReconciliationSettingsKeys.StrictValidationWindowDays, days);

    private async Task<int> ReadDaysAsync(string key, int defaultValue)
    {
        var result = await _settings.GetSettingAsync(
            ReconciliationSettingsKeys.Category,
            key
        );

        if (result.IsSuccess is false || result.Data is null)
        {
            return defaultValue;
        }

        if (
            int.TryParse(
                result.Data.Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int parsed
            )
            && parsed >= 0
        )
        {
            return parsed;
        }

        return defaultValue;
    }

    private async Task WriteDaysAsync(string key, int days)
    {
        int clamped = days < 0 ? 0 : days;

        await _settings.SetSettingAsync(
            ReconciliationSettingsKeys.Category,
            key,
            clamped.ToString(CultureInfo.InvariantCulture)
        );
    }
}
