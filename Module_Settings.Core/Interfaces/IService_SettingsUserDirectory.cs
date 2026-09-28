using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// Read-only user directory used by settings pages that must list application users.
/// </summary>
public interface IService_SettingsUserDirectory
{
    /// <summary>
    /// Returns every application user, including deactivated accounts.
    /// </summary>
    Task<Model_Dao_Result<List<Model_User>>> GetAllUsersAsync();
}
