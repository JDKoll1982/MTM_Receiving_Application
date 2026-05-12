using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Data;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Service for managing user-scoped Receiving label path settings.
/// </summary>
public class Service_ReceivingUserLabelSettings : IService_ReceivingUserLabelSettings
{
    private readonly Dao_SettingsCoreUser _userSettingsDao;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_LoggingUtility _logger;

    public Service_ReceivingUserLabelSettings(
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

    public async Task<string> GetReceivingLabelPathAsync()
    {
        return await GetUserLabelPathAsync(ReceivingSettingsKeys.UserLabels.ReceivingLabelPath);
    }

    public async Task<string> GetMiniReceivingLabelPathAsync()
    {
        return await GetUserLabelPathAsync(
            ReceivingSettingsKeys.UserLabels.MiniReceivingLabelPath
        );
    }

    public async Task SaveReceivingLabelPathAsync(string path)
    {
        await SaveUserLabelPathAsync(
            ReceivingSettingsKeys.UserLabels.ReceivingLabelPath,
            path
        );
    }

    public async Task SaveMiniReceivingLabelPathAsync(string path)
    {
        await SaveUserLabelPathAsync(
            ReceivingSettingsKeys.UserLabels.MiniReceivingLabelPath,
            path
        );
    }

    private async Task<string> GetUserLabelPathAsync(string settingKey)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                _logger.LogWarning(
                    "Cannot retrieve user label path - no active user session",
                    nameof(Service_ReceivingUserLabelSettings)
                );
                return string.Empty;
            }

            var result = await _userSettingsDao.GetByKeyAsync(
                userId,
                ReceivingSettingsKeys.UserLabels.Category,
                settingKey
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
                $"Error retrieving user label path for key '{settingKey}': {ex.Message}",
                ex,
                nameof(Service_ReceivingUserLabelSettings)
            );
            return string.Empty;
        }
    }

    private async Task SaveUserLabelPathAsync(string settingKey, string path)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                _logger.LogWarning(
                    "Cannot save user label path - no active user session",
                    nameof(Service_ReceivingUserLabelSettings)
                );
                return;
            }

            var username = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "Unknown";

            await _userSettingsDao.UpsertAsync(
                userId,
                ReceivingSettingsKeys.UserLabels.Category,
                settingKey,
                path ?? string.Empty,
                "string",
                username
            );

            _logger.LogInfo(
                $"Saved user label path for key '{settingKey}' for user {userId}",
                nameof(Service_ReceivingUserLabelSettings)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error saving user label path for key '{settingKey}': {ex.Message}",
                ex,
                nameof(Service_ReceivingUserLabelSettings)
            );
        }
    }

    private int GetCurrentUserId()
    {
        return _sessionManager.CurrentSession?.User?.Id ?? 0;
    }
}
