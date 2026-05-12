using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Data;

namespace MTM_Receiving_Application.Module_Dunnage.Services;

/// <summary>
/// Service for managing user-scoped Dunnage label path settings.
/// </summary>
public class Service_DunnageUserLabelSettings : IService_DunnageUserLabelSettings
{
    private readonly Dao_SettingsCoreUser _userSettingsDao;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_LoggingUtility _logger;

    public Service_DunnageUserLabelSettings(
        Dao_SettingsCoreUser userSettingsDao,
        IService_UserSessionManager sessionManager,
        IService_LoggingUtility _logger
    )
    {
        _userSettingsDao =
            userSettingsDao ?? throw new ArgumentNullException(nameof(userSettingsDao));
        _sessionManager =
            sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        this._logger = _logger ?? throw new ArgumentNullException(nameof(_logger));
    }

    public async Task<string> GetDunnageLabelPathAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                _logger.LogWarning(
                    "Cannot retrieve user label path - no active user session",
                    nameof(Service_DunnageUserLabelSettings)
                );
                return string.Empty;
            }

            var result = await _userSettingsDao.GetByKeyAsync(
                userId,
                DunnageSettingsKeys.UserLabels.Category,
                DunnageSettingsKeys.UserLabels.DunnageLabelPath
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
                $"Error retrieving user Dunnage label path: {ex.Message}",
                ex,
                nameof(Service_DunnageUserLabelSettings)
            );
            return string.Empty;
        }
    }

    public async Task SaveDunnageLabelPathAsync(string path)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                _logger.LogWarning(
                    "Cannot save user label path - no active user session",
                    nameof(Service_DunnageUserLabelSettings)
                );
                return;
            }

            var username = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "Unknown";

            await _userSettingsDao.UpsertAsync(
                userId,
                DunnageSettingsKeys.UserLabels.Category,
                DunnageSettingsKeys.UserLabels.DunnageLabelPath,
                path ?? string.Empty,
                "string",
                username
            );

            _logger.LogInfo(
                $"Saved user Dunnage label path for user {userId}",
                nameof(Service_DunnageUserLabelSettings)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error saving user Dunnage label path: {ex.Message}",
                ex,
                nameof(Service_DunnageUserLabelSettings)
            );
        }
    }

    private int GetCurrentUserId()
    {
        return _sessionManager.CurrentSession?.User?.EmployeeNumber ?? 0;
    }
}
