using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Data;
using MTM_Receiving_Application.Module_Settings.Core.Enums;

namespace MTM_Receiving_Application.Module_Core.Services.Authentication;

/// <summary>
/// Loads and caches effective user roles for the active application session.
/// </summary>
public class Service_UserPrivileges : IService_UserPrivileges
{
    private static readonly string[] SupervisorOrAboveRoles =
    {
        "Supervisor",
        "Admin",
        "Developer",
    };
    private static readonly string[] AdminOrAboveRoles = { "Admin", "Developer" };

    private readonly Dao_SettingsCoreRoles _rolesDao;
    private readonly Dao_SettingsCoreUserRoles _userRolesDao;
    private readonly IService_LoggingUtility _logger;
    private readonly HashSet<string> _currentRoles = new(StringComparer.OrdinalIgnoreCase);

    public Service_UserPrivileges(
        Dao_SettingsCoreRoles rolesDao,
        Dao_SettingsCoreUserRoles userRolesDao,
        IService_LoggingUtility logger
    )
    {
        _rolesDao = rolesDao ?? throw new ArgumentNullException(nameof(rolesDao));
        _userRolesDao = userRolesDao ?? throw new ArgumentNullException(nameof(userRolesDao));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public int? CurrentUserId { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyCollection<string> CurrentRoles => _currentRoles;

    /// <inheritdoc/>
    public bool IsInitialized => CurrentUserId.HasValue;

    /// <inheritdoc/>
    public async Task<Model_Dao_Result> InitializeAsync(int employeeNumber)
    {
        Clear();

        if (employeeNumber <= 0)
        {
            return Model_Dao_Result_Factory.Failure(
                "A valid employee number is required to initialize privileges."
            );
        }

        try
        {
            var rolesResult = await _rolesDao.GetAllAsync();
            if (!rolesResult.Success || rolesResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure(
                    rolesResult.ErrorMessage
                        ?? "Failed to load role definitions for the active session.",
                    rolesResult.Exception
                );
            }

            var userRolesResult = await _userRolesDao.GetByUserAsync(employeeNumber);
            if (!userRolesResult.Success || userRolesResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure(
                    userRolesResult.ErrorMessage
                        ?? "Failed to load user-role assignments for the active session.",
                    userRolesResult.Exception
                );
            }

            if (userRolesResult.Data.Count == 0)
            {
                var defaultRole = rolesResult.Data.FirstOrDefault(role =>
                    string.Equals(role.RoleName, "User", StringComparison.OrdinalIgnoreCase)
                );

                if (defaultRole == null)
                {
                    return Model_Dao_Result_Factory.Failure(
                        "Default User role is missing. Cannot initialize privileges for the active session."
                    );
                }

                var assignResult = await _userRolesDao.AssignRoleAsync(
                    employeeNumber,
                    defaultRole.Id
                );
                if (!assignResult.Success)
                {
                    return Model_Dao_Result_Factory.Failure(
                        assignResult.ErrorMessage ?? "Failed to assign the default User role.",
                        assignResult.Exception
                    );
                }

                userRolesResult = await _userRolesDao.GetByUserAsync(employeeNumber);
                if (!userRolesResult.Success || userRolesResult.Data == null)
                {
                    return Model_Dao_Result_Factory.Failure(
                        userRolesResult.ErrorMessage
                            ?? "Failed to reload user-role assignments after default-role assignment.",
                        userRolesResult.Exception
                    );
                }
            }

            var currentRoleIds = userRolesResult.Data.Select(role => role.RoleId).ToHashSet();
            foreach (
                var roleName in rolesResult
                    .Data.Where(role => currentRoleIds.Contains(role.Id))
                    .Select(role => role.RoleName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
            )
            {
                _currentRoles.Add(roleName);
            }

            CurrentUserId = employeeNumber;
            _logger.LogInfo(
                $"Initialized privileges for employee {employeeNumber}: {string.Join(", ", _currentRoles.DefaultIfEmpty("<none>"))}",
                nameof(Service_UserPrivileges)
            );

            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            Clear();
            _logger.LogError(
                "Failed to initialize user privileges.",
                ex,
                nameof(Service_UserPrivileges)
            );
            return Model_Dao_Result_Factory.Failure("Failed to initialize user privileges.", ex);
        }
    }

    /// <inheritdoc/>
    public bool HasRole(string roleName)
    {
        return !string.IsNullOrWhiteSpace(roleName) && _currentRoles.Contains(roleName);
    }

    /// <inheritdoc/>
    public bool HasAnyRole(params string[] roleNames)
    {
        return roleNames.Any(HasRole);
    }

    /// <inheritdoc/>
    public bool HasPermissionLevel(Enum_SettingsPermissionLevel requiredLevel)
    {
        return requiredLevel switch
        {
            Enum_SettingsPermissionLevel.User => true,
            Enum_SettingsPermissionLevel.Supervisor => HasAnyRole(SupervisorOrAboveRoles),
            Enum_SettingsPermissionLevel.Admin => HasAnyRole(AdminOrAboveRoles),
            Enum_SettingsPermissionLevel.Developer => HasRole("Developer"),
            _ => false,
        };
    }

    /// <inheritdoc/>
    public void Clear()
    {
        CurrentUserId = null;
        _currentRoles.Clear();
    }
}
