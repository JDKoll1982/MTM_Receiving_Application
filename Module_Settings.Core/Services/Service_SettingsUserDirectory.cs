using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Data.Authentication;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// User directory backed by the authentication user store.
/// </summary>
public class Service_SettingsUserDirectory : IService_SettingsUserDirectory
{
    private readonly Dao_User _userDao;

    public Service_SettingsUserDirectory(Dao_User userDao)
    {
        _userDao = userDao ?? throw new ArgumentNullException(nameof(userDao));
    }

    /// <inheritdoc/>
    public Task<Model_Dao_Result<List<Model_User>>> GetAllUsersAsync()
    {
        return _userDao.GetAllAsync();
    }
}
