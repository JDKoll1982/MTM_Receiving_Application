using System;
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
/// Handler for ResetSettingCommand.
/// </summary>
public class ResetSettingCommandHandler : IRequestHandler<ResetSettingCommand, Model_Dao_Result>
{
    private readonly Dao_SettingsCoreSystem _systemDao;
    private readonly Dao_SettingsCoreUser _userDao;
    private readonly Dao_SettingsCoreAudit _auditDao;
    private readonly ISettingsMetadataRegistry _registry;
    private readonly ISettingsCache _cache;
    private readonly ISettingsEncryptionService _encryptionService;
    private readonly IService_UserSessionManager _sessionManager;

    public ResetSettingCommandHandler(
        Dao_SettingsCoreSystem systemDao,
        Dao_SettingsCoreUser userDao,
        Dao_SettingsCoreAudit auditDao,
        ISettingsMetadataRegistry registry,
        ISettingsCache cache,
        ISettingsEncryptionService encryptionService,
        IService_UserSessionManager sessionManager)
    {
        _systemDao = systemDao;
        _userDao = userDao;
        _auditDao = auditDao;
        _registry = registry;
        _cache = cache;
        _encryptionService = encryptionService;
        _sessionManager = sessionManager;
    }

    public async Task<Model_Dao_Result> Handle(ResetSettingCommand request, CancellationToken cancellationToken)
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

        var actor = _sessionManager.CurrentSession?.User?.WindowsUsername ?? Environment.UserName;
        var valueToStore = definition.IsSensitive ? _encryptionService.Encrypt(definition.DefaultValue) : definition.DefaultValue;

        Model_Dao_Result result;
        if (scope == Enum_SettingsScope.System)
        {
            var currentResult = await _systemDao.GetByKeyAsync(request.Category, request.Key);
            if (currentResult.Success && currentResult.Data?.IsLocked == true)
            {
                return Model_Dao_Result_Factory.Failure("Setting is locked and cannot be reset.");
            }
            result = await _systemDao.UpsertAsync(request.Category, request.Key, valueToStore, definition.DataType.ToString(), definition.IsSensitive, actor);
        }
        else
        {
            result = await _userDao.UpsertAsync(userId!.Value, request.Category, request.Key, valueToStore, definition.DataType.ToString(), actor);
        }

        if (result.Success)
        {
            await _auditDao.InsertAsync(new Model_SettingsAuditEntry
            {
                Scope = scope.ToString(),
                Category = request.Category,
                SettingKey = request.Key,
                OldValue = string.Empty,
                NewValue = definition.DefaultValue,
                ChangeType = "Reset",
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

    private static string BuildCacheKey(Enum_SettingsScope scope, string category, string key, int? userId)
    {
        return scope == Enum_SettingsScope.System
            ? $"system:{category}:{key}"
            : $"user:{userId}:{category}:{key}";
    }
}
