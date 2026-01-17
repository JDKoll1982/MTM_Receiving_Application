using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Data;
using MTM_Receiving_Application.Module_Settings.Core.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Handler for GetSettingQuery.
/// </summary>
public class GetSettingQueryHandler : IRequestHandler<GetSettingQuery, Model_Dao_Result<Model_SettingsValue>>
{
    private readonly Dao_SettingsCoreSystem _systemDao;
    private readonly Dao_SettingsCoreUser _userDao;
    private readonly Dao_SettingsCoreAudit _auditDao;
    private readonly ISettingsMetadataRegistry _registry;
    private readonly ISettingsCache _cache;
    private readonly ISettingsEncryptionService _encryptionService;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_ErrorHandler _errorHandler;
    private readonly IService_Notification _notification;

    public GetSettingQueryHandler(
        Dao_SettingsCoreSystem systemDao,
        Dao_SettingsCoreUser userDao,
        Dao_SettingsCoreAudit auditDao,
        ISettingsMetadataRegistry registry,
        ISettingsCache cache,
        ISettingsEncryptionService encryptionService,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_Notification notification)
    {
        _systemDao = systemDao;
        _userDao = userDao;
        _auditDao = auditDao;
        _registry = registry;
        _cache = cache;
        _encryptionService = encryptionService;
        _sessionManager = sessionManager;
        _errorHandler = errorHandler;
        _notification = notification;
    }

    public async Task<Model_Dao_Result<Model_SettingsValue>> Handle(GetSettingQuery request, CancellationToken cancellationToken)
    {
        var definition = _registry.GetDefinition(request.Category, request.Key);
        if (definition == null)
        {
            return Model_Dao_Result_Factory.Failure<Model_SettingsValue>($"Unknown setting: {request.Category}:{request.Key}");
        }

        var scope = definition.Scope;
        var userId = ResolveUserId(scope, request.UserId);
        if (scope == Enum_SettingsScope.User && userId == null)
        {
            return Model_Dao_Result_Factory.Failure<Model_SettingsValue>("User context required for user-scoped settings.");
        }

        var cacheKey = BuildCacheKey(scope, request.Category, request.Key, userId);
        if (_cache.TryGet(cacheKey, out var cached) && cached != null)
        {
            return Model_Dao_Result_Factory.Success(cached);
        }

        Model_Dao_Result<Model_CoreSetting>? systemResult = null;
        Model_Dao_Result<Model_UserSetting>? userResult = null;

        if (scope == Enum_SettingsScope.System)
        {
            systemResult = await _systemDao.GetByKeyAsync(request.Category, request.Key);
        }
        else
        {
            userResult = await _userDao.GetByKeyAsync(userId!.Value, request.Category, request.Key);
        }

        string resolvedValue;
        bool isMissing = scope == Enum_SettingsScope.System
            ? systemResult?.Success != true || systemResult.Data == null
            : userResult?.Success != true || userResult.Data == null;

        if (isMissing)
        {
            resolvedValue = definition.DefaultValue;
            await WriteDefaultAsync(definition, resolvedValue, userId);
        }
        else
        {
            resolvedValue = scope == Enum_SettingsScope.System
                ? systemResult!.Data!.SettingValue
                : userResult!.Data!.SettingValue;
        }

        string? decryptedValue = resolvedValue;
        if (definition.IsSensitive && !string.IsNullOrWhiteSpace(resolvedValue))
        {
            try
            {
                decryptedValue = _encryptionService.Decrypt(resolvedValue);
            }
            catch (Exception ex)
            {
                await HandleCorruptedValueAsync(definition, resolvedValue, userId, $"Sensitive value could not be decrypted: {ex.Message}");
                decryptedValue = definition.DefaultValue;
            }
        }

        if (!IsValueValid(definition.DataType, decryptedValue ?? string.Empty))
        {
            await HandleCorruptedValueAsync(definition, decryptedValue ?? string.Empty, userId, "Value type mismatch");
            decryptedValue = definition.DefaultValue;
        }

        var settingsValue = new Model_SettingsValue
        {
            Category = request.Category,
            Key = request.Key,
            Scope = scope,
            DataType = definition.DataType,
            Value = decryptedValue ?? string.Empty,
            DisplayValue = definition.IsSensitive ? "••••••" : decryptedValue ?? string.Empty,
            IsSensitive = definition.IsSensitive,
            IsLocked = systemResult?.Data?.IsLocked ?? false,
            UserId = userId
        };

        _cache.Set(cacheKey, settingsValue);
        return Model_Dao_Result_Factory.Success(settingsValue);
    }

    private int? ResolveUserId(Enum_SettingsScope scope, int? userId)
    {
        if (scope == Enum_SettingsScope.System)
        {
            return null;
        }

        if (userId.HasValue)
        {
            return userId.Value;
        }

        return _sessionManager.CurrentSession?.User?.EmployeeNumber;
    }

    private async Task WriteDefaultAsync(Model_SettingsDefinition definition, string defaultValue, int? userId)
    {
        var actor = _sessionManager.CurrentSession?.User?.WindowsUsername ?? Environment.UserName;
        if (definition.Scope == Enum_SettingsScope.System)
        {
            var valueToStore = definition.IsSensitive ? _encryptionService.Encrypt(defaultValue) : defaultValue;
            await _systemDao.UpsertAsync(definition.Category, definition.Key, valueToStore, definition.DataType.ToString(), definition.IsSensitive, actor);
        }
        else if (userId.HasValue)
        {
            var valueToStore = definition.IsSensitive ? _encryptionService.Encrypt(defaultValue) : defaultValue;
            await _userDao.UpsertAsync(userId.Value, definition.Category, definition.Key, valueToStore, definition.DataType.ToString(), actor);
        }

        await _auditDao.InsertAsync(new Model_SettingsAuditEntry
        {
            Scope = definition.Scope.ToString(),
            Category = definition.Category,
            SettingKey = definition.Key,
            OldValue = string.Empty,
            NewValue = defaultValue,
            ChangeType = "DefaultApplied",
            UserId = userId,
            ChangedBy = actor,
            ChangedAt = DateTime.Now,
            WorkstationName = Environment.MachineName
        });
    }

    private async Task HandleCorruptedValueAsync(Model_SettingsDefinition definition, string currentValue, int? userId, string reason)
    {
        var actor = _sessionManager.CurrentSession?.User?.WindowsUsername ?? Environment.UserName;
        var message = $"Setting '{definition.Category}:{definition.Key}' expected {definition.DataType} but found '{currentValue}'. Reason: {reason}. Resetting to default.";

        await _errorHandler.ShowUserErrorAsync(message, "Core Settings Reset", nameof(GetSettingQueryHandler));
        _notification.ShowStatus(message, InfoBarSeverity.Warning);

        await WriteDefaultAsync(definition, definition.DefaultValue, userId);
    }

    private static bool IsValueValid(Enum_SettingsDataType dataType, string value)
    {
        switch (dataType)
        {
            case Enum_SettingsDataType.Int:
                return int.TryParse(value, out _);
            case Enum_SettingsDataType.Decimal:
                return decimal.TryParse(value, out _);
            case Enum_SettingsDataType.Bool:
                return bool.TryParse(value, out _);
            case Enum_SettingsDataType.DateTime:
                return DateTime.TryParse(value, out _);
            case Enum_SettingsDataType.Json:
                try
                {
                    _ = JsonDocument.Parse(value);
                    return true;
                }
                catch
                {
                    return false;
                }
            default:
                return true;
        }
    }

    private static string BuildCacheKey(Enum_SettingsScope scope, string category, string key, int? userId)
    {
        return scope == Enum_SettingsScope.System
            ? $"system:{category}:{key}"
            : $"user:{userId}:{category}:{key}";
    }
}
