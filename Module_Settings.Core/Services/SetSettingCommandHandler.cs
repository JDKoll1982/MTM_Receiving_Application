using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Data;
using MTM_Receiving_Application.Module_Settings.Core.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Handler for SetSettingCommand.
/// </summary>
public class SetSettingCommandHandler : IRequestHandler<SetSettingCommand, Model_Dao_Result>
{
    private readonly Dao_SettingsCoreSystem _systemDao;
    private readonly Dao_SettingsCoreUser _userDao;
    private readonly Dao_SettingsCoreAudit _auditDao;
    private readonly Dao_SettingsCoreRoles _rolesDao;
    private readonly Dao_SettingsCoreUserRoles _userRolesDao;
    private readonly ISettingsMetadataRegistry _registry;
    private readonly ISettingsCache _cache;
    private readonly ISettingsEncryptionService _encryptionService;
    private readonly IService_UserSessionManager _sessionManager;

    public SetSettingCommandHandler(
        Dao_SettingsCoreSystem systemDao,
        Dao_SettingsCoreUser userDao,
        Dao_SettingsCoreAudit auditDao,
        Dao_SettingsCoreRoles rolesDao,
        Dao_SettingsCoreUserRoles userRolesDao,
        ISettingsMetadataRegistry registry,
        ISettingsCache cache,
        ISettingsEncryptionService encryptionService,
        IService_UserSessionManager sessionManager)
    {
        _systemDao = systemDao;
        _userDao = userDao;
        _auditDao = auditDao;
        _rolesDao = rolesDao;
        _userRolesDao = userRolesDao;
        _registry = registry;
        _cache = cache;
        _encryptionService = encryptionService;
        _sessionManager = sessionManager;
    }

    public async Task<Model_Dao_Result> Handle(SetSettingCommand request, CancellationToken cancellationToken)
    {
        var definition = _registry.GetDefinition(request.Category, request.Key);
        if (definition == null)
        {
            return Model_Dao_Result_Factory.Failure($"Unknown setting: {request.Category}:{request.Key}");
        }

        var scope = definition.Scope;
        var userId = ResolveUserId(scope, request.UserId);
        if (scope == Enum_SettingsScope.User && userId == null)
        {
            return Model_Dao_Result_Factory.Failure("User context required for user-scoped settings.");
        }

        if (!await HasPermissionAsync(definition.PermissionLevel, userId))
        {
            return Model_Dao_Result_Factory.Failure("Insufficient permissions to modify this setting.");
        }

        if (!IsValueValid(definition.DataType, request.Value))
        {
            return Model_Dao_Result_Factory.Failure($"Invalid value for {definition.DataType}.");
        }

        if (scope == Enum_SettingsScope.System)
        {
            var currentResult = await _systemDao.GetByKeyAsync(request.Category, request.Key);
            if (currentResult.Success && currentResult.Data?.IsLocked == true)
            {
                return Model_Dao_Result_Factory.Failure("Setting is locked and cannot be modified.");
            }
        }

        var actor = _sessionManager.CurrentSession?.User?.WindowsUsername ?? Environment.UserName;
        var storedValue = definition.IsSensitive ? _encryptionService.Encrypt(request.Value) : request.Value;

        Model_Dao_Result result;
        if (scope == Enum_SettingsScope.System)
        {
            result = await _systemDao.UpsertAsync(request.Category, request.Key, storedValue, definition.DataType.ToString(), definition.IsSensitive, actor);
        }
        else
        {
            result = await _userDao.UpsertAsync(userId!.Value, request.Category, request.Key, storedValue, definition.DataType.ToString(), actor);
        }

        if (result.Success)
        {
            await _auditDao.InsertAsync(new Model_SettingsAuditEntry
            {
                Scope = scope.ToString(),
                Category = request.Category,
                SettingKey = request.Key,
                OldValue = string.Empty,
                NewValue = request.Value,
                ChangeType = "Set",
                UserId = userId,
                ChangedBy = actor,
                ChangedAt = DateTime.Now,
                WorkstationName = Environment.MachineName
            });

            _cache.Remove(BuildCacheKey(scope, request.Category, request.Key, userId));
        }

        return result;
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

    private async Task<bool> HasPermissionAsync(Enum_SettingsPermissionLevel required, int? userId)
    {
        if (required == Enum_SettingsPermissionLevel.User)
        {
            return true;
        }

        if (!userId.HasValue)
        {
            return false;
        }

        var rolesResult = await _rolesDao.GetAllAsync();
        if (!rolesResult.Success || rolesResult.Data == null)
        {
            return false;
        }

        var userRolesResult = await _userRolesDao.GetByUserAsync(userId.Value);
        if (!userRolesResult.Success || userRolesResult.Data == null)
        {
            return false;
        }

        var userRoleIds = userRolesResult.Data.Select(r => r.RoleId).ToHashSet();
        var userRoleNames = rolesResult.Data
            .Where(r => userRoleIds.Contains(r.Id))
            .Select(r => r.RoleName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return required switch
        {
            Enum_SettingsPermissionLevel.Supervisor => userRoleNames.Contains("Supervisor") || userRoleNames.Contains("Admin") || userRoleNames.Contains("Developer"),
            Enum_SettingsPermissionLevel.Admin => userRoleNames.Contains("Admin") || userRoleNames.Contains("Developer"),
            Enum_SettingsPermissionLevel.Developer => userRoleNames.Contains("Developer"),
            _ => false
        };
    }

    private static string BuildCacheKey(Enum_SettingsScope scope, string category, string key, int? userId)
    {
        return scope == Enum_SettingsScope.System
            ? $"system:{category}:{key}"
            : $"user:{userId}:{category}:{key}";
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
}
