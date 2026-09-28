using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Helpers;
using MTM_Receiving_Application.Module_Scanner.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Module_Scanner.Services;

/// <summary>
/// Resolves Scanner module access from the plant-wide allow-list stored in core settings.
/// </summary>
public class Service_ScannerAccessPolicy : IService_ScannerAccessPolicy
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromSeconds(30);

    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_LoggingUtility _logger;

    private IReadOnlyList<int>? _cachedEmployeeNumbers;
    private DateTime _cachedAtUtc = DateTime.MinValue;

    public Service_ScannerAccessPolicy(
        IService_SettingsCoreFacade settingsCore,
        IService_LoggingUtility logger
    )
    {
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Model_Dao_Result<IReadOnlyList<int>>> GetAllowedEmployeeNumbersAsync()
    {
        if (TryGetCached(out var cached))
        {
            return Model_Dao_Result_Factory.Success(cached);
        }

        var result = await _settingsCore.GetSettingAsync(
            ScannerSettingsKeys.Category,
            ScannerSettingsKeys.Access.AllowedEmployeeNumbers
        );

        if (!result.Success || result.Data is null)
        {
            return Model_Dao_Result_Factory.Failure<IReadOnlyList<int>>(
                result.ErrorMessage ?? "Failed to read the Scanner access allow-list.",
                result.Exception
            );
        }

        var employeeNumbers = ParseEmployeeNumbers(result.Data.Value);
        Cache(employeeNumbers);
        return Model_Dao_Result_Factory.Success(employeeNumbers);
    }

    /// <inheritdoc/>
    public async Task<Model_Dao_Result> SaveAllowedEmployeeNumbersAsync(
        IReadOnlyCollection<int> employeeNumbers
    )
    {
        ArgumentNullException.ThrowIfNull(employeeNumbers);

        var normalized = employeeNumbers
            .Where(employeeNumber => employeeNumber > 0)
            .Distinct()
            .Order()
            .ToArray();

        var result = await _settingsCore.SetSettingAsync(
            ScannerSettingsKeys.Category,
            ScannerSettingsKeys.Access.AllowedEmployeeNumbers,
            JsonSerializer.Serialize(normalized)
        );

        if (result.Success)
        {
            InvalidateCache();
            _logger.LogInfo(
                $"Scanner access allow-list saved with {normalized.Length} user(s).",
                nameof(Service_ScannerAccessPolicy)
            );
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> IsUserAllowedAsync(
        Model_User? user,
        string? fallbackWindowsUserName = null
    )
    {
        var allowedResult = await GetAllowedEmployeeNumbersAsync();

        if (!allowedResult.Success || allowedResult.Data is null)
        {
            // Never widen access when the policy cannot be read: keep the rollout rule.
            _logger.LogWarning(
                $"Scanner access allow-list unavailable ({allowedResult.ErrorMessage}). Using the developer-only rule.",
                nameof(Service_ScannerAccessPolicy)
            );
            return Helper_ScannerAccess.IsDeveloperUser(user, fallbackWindowsUserName);
        }

        if (allowedResult.Data.Count == 0)
        {
            return Helper_ScannerAccess.IsDeveloperUser(user, fallbackWindowsUserName);
        }

        // A configured allow-list is absolute: only listed employees may use Scanner, and a
        // missing session user cannot be matched so it is denied.
        return user is not null && allowedResult.Data.Contains(user.EmployeeNumber);
    }

    /// <inheritdoc/>
    public void InvalidateCache()
    {
        _cachedEmployeeNumbers = null;
        _cachedAtUtc = DateTime.MinValue;
    }

    private bool TryGetCached(out IReadOnlyList<int> employeeNumbers)
    {
        employeeNumbers = _cachedEmployeeNumbers ?? Array.Empty<int>();
        return _cachedEmployeeNumbers is not null
            && DateTime.UtcNow - _cachedAtUtc < CacheLifetime;
    }

    private void Cache(IReadOnlyList<int> employeeNumbers)
    {
        _cachedEmployeeNumbers = employeeNumbers;
        _cachedAtUtc = DateTime.UtcNow;
    }

    private static IReadOnlyList<int> ParseEmployeeNumbers(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return Array.Empty<int>();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<int[]>(rawValue);
            if (parsed is null)
            {
                return Array.Empty<int>();
            }

            return parsed
                .Where(employeeNumber => employeeNumber > 0)
                .Distinct()
                .Order()
                .ToArray();
        }
        catch (JsonException)
        {
            return Array.Empty<int>();
        }
    }
}
