using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Data;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Settings;

namespace MTM_Receiving_Application.Module_Volvo.Services;

/// <summary>
/// Service for managing user-scoped Volvo label path settings.
/// </summary>
public class Service_VolvoUserLabelSettings : IService_VolvoUserLabelSettings
{
    private readonly Dao_SettingsCoreUser _userSettingsDao;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_LoggingUtility _logger;

    public Service_VolvoUserLabelSettings(
        Dao_SettingsCoreUser userSettingsDao,
        IService_UserSessionManager sessionManager,
        IService_LoggingUtility logger
    )
    {
        _userSettingsDao =
            userSettingsDao ?? throw new ArgumentNullException(nameof(userSettingsDao));
        _sessionManager =
            sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetVolvoLabelPathAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                _logger.LogWarning(
                    "Cannot retrieve user label path - no active user session",
                    nameof(Service_VolvoUserLabelSettings)
                );
                return string.Empty;
            }

            var result = await _userSettingsDao.GetByKeyAsync(
                userId,
                VolvoSettingsKeys.UserLabels.Category,
                VolvoSettingsKeys.UserLabels.VolvoLabelPath
            );

            if (result.IsSuccess && result.Data != null)
            {
                return result.Data.SettingValue ?? string.Empty;
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error retrieving user Volvo label path: {ex.Message}",
                ex,
                nameof(Service_VolvoUserLabelSettings)
            );
            return string.Empty;
        }
    }

    public async Task SaveVolvoLabelPathAsync(string path)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                _logger.LogWarning(
                    "Cannot save user label path - no active user session",
                    nameof(Service_VolvoUserLabelSettings)
                );
                return;
            }

            var username = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "Unknown";

            await _userSettingsDao.UpsertAsync(
                userId,
                VolvoSettingsKeys.UserLabels.Category,
                VolvoSettingsKeys.UserLabels.VolvoLabelPath,
                path ?? string.Empty,
                "string",
                username
            );

            _logger.LogInfo(
                $"Saved user Volvo label path for user {userId}",
                nameof(Service_VolvoUserLabelSettings)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error saving user Volvo label path: {ex.Message}",
                ex,
                nameof(Service_VolvoUserLabelSettings)
            );
        }
    }

    private int GetCurrentUserId()
    {
        return _sessionManager.CurrentSession?.User?.Id ?? 0;
    }
}
