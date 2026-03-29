using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Enums;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Provides the current user's effective roles and permission checks for the active session.
/// </summary>
public interface IService_UserPrivileges
{
    /// <summary>
    /// Gets the employee number for the user whose privileges are currently loaded.
    /// </summary>
    int? CurrentUserId { get; }

    /// <summary>
    /// Gets the currently loaded role names for the active user.
    /// </summary>
    IReadOnlyCollection<string> CurrentRoles { get; }

    /// <summary>
    /// Gets a value indicating whether privileges have been initialized for the current session.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Loads the current user's roles into session-scoped memory.
    /// </summary>
    /// <param name="employeeNumber">The employee number to load roles for.</param>
    /// <returns>A result describing whether initialization completed successfully.</returns>
    Task<Model_Dao_Result> InitializeAsync(int employeeNumber);

    /// <summary>
    /// Returns true when the current user has the provided role.
    /// </summary>
    /// <param name="roleName">The role name to check.</param>
    bool HasRole(string roleName);

    /// <summary>
    /// Returns true when the current user has any of the provided roles.
    /// </summary>
    /// <param name="roleNames">The role names to check.</param>
    bool HasAnyRole(params string[] roleNames);

    /// <summary>
    /// Returns true when the current user satisfies the required settings permission level.
    /// </summary>
    /// <param name="requiredLevel">The required permission level.</param>
    bool HasPermissionLevel(Enum_SettingsPermissionLevel requiredLevel);

    /// <summary>
    /// Clears all cached role information for the current session.
    /// </summary>
    void Clear();
}
