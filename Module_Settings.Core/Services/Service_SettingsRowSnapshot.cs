using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Data;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Reads the settings rows for a handful of categories once, and serves them by key from memory.
/// </summary>
/// <remarks>
/// The table already has a stored procedure that returns a whole category, so priming is one read per
/// category rather than one read per setting. That is the difference between roughly a dozen round trips
/// and nearly three hundred, which is what a terminal was waiting through on every launch.
/// </remarks>
public class Service_SettingsRowSnapshot : ISettingsRowSnapshot
{
    private readonly Dao_SettingsCoreSystem _systemDao;
    private readonly Dao_SettingsCoreUser _userDao;
    private readonly IService_LoggingUtility _logger;

    private readonly ConcurrentDictionary<string, Model_CoreSetting> _systemRows = new(
        StringComparer.OrdinalIgnoreCase
    );

    private readonly ConcurrentDictionary<string, Model_UserSetting> _userRows = new(
        StringComparer.OrdinalIgnoreCase
    );

    private volatile bool _isPrimed;

    private int? _primedUserId;

    public Service_SettingsRowSnapshot(
        Dao_SettingsCoreSystem systemDao,
        Dao_SettingsCoreUser userDao,
        IService_LoggingUtility logger
    )
    {
        _systemDao = systemDao ?? throw new ArgumentNullException(nameof(systemDao));
        _userDao = userDao ?? throw new ArgumentNullException(nameof(userDao));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool IsPrimed => _isPrimed;

    public async Task PrimeAsync(
        int? userId,
        IReadOnlyCollection<string> categories,
        CancellationToken cancellationToken = default
    )
    {
        if (categories is null || categories.Count == 0)
        {
            return;
        }

        _systemRows.Clear();
        _userRows.Clear();
        _primedUserId = userId;
        _isPrimed = false;

        foreach (var category in categories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var systemResult = await _systemDao.GetByCategoryAsync(category);

            if (systemResult.Success && systemResult.Data is not null)
            {
                foreach (var row in systemResult.Data)
                {
                    _systemRows[BuildKey(row.Category, row.SettingKey)] = row;
                }
            }
            else if (!systemResult.Success)
            {
                _logger.LogWarning(
                    $"Settings snapshot could not read the system rows for '{category}': {systemResult.ErrorMessage}",
                    nameof(Service_SettingsRowSnapshot)
                );
            }

            if (userId is null)
            {
                continue;
            }

            var userResult = await _userDao.GetByCategoryAsync(userId.Value, category);

            if (userResult.Success && userResult.Data is not null)
            {
                foreach (var row in userResult.Data)
                {
                    _userRows[BuildKey(row.Category, row.SettingKey)] = row;
                }
            }
            else if (!userResult.Success)
            {
                _logger.LogWarning(
                    $"Settings snapshot could not read the user rows for '{category}': {userResult.ErrorMessage}",
                    nameof(Service_SettingsRowSnapshot)
                );
            }
        }

        // Primed only once the reads are done, so a half-filled snapshot is never treated as authoritative.
        _isPrimed = true;

        _logger.LogInfo(
            $"Settings snapshot primed from {categories.Count} categor{(categories.Count == 1 ? "y" : "ies")}: "
                + $"{_systemRows.Count} system row(s), {_userRows.Count} user row(s).",
            nameof(Service_SettingsRowSnapshot)
        );
    }

    public Model_CoreSetting? FindSystem(string category, string key) =>
        _systemRows.TryGetValue(BuildKey(category, key), out var row) ? row : null;

    public Model_UserSetting? FindUser(int userId, string category, string key)
    {
        // A snapshot taken for somebody else's session must not answer for this user.
        if (_primedUserId != userId)
        {
            return null;
        }

        return _userRows.TryGetValue(BuildKey(category, key), out var row) ? row : null;
    }

    private static string BuildKey(string category, string key) => $"{category}:{key}";
}
