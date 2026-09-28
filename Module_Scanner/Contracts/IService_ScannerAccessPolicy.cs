using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Systems;

namespace MTM_Receiving_Application.Module_Scanner.Contracts;

/// <summary>
/// Plant-wide Scanner access policy. The allow-list is a single system-scoped setting so an
/// admin or developer grants access per user name once for every workstation.
/// </summary>
public interface IService_ScannerAccessPolicy
{
    /// <summary>
    /// Reads the allow-list of employee numbers permitted to use the Scanner module.
    /// </summary>
    Task<Model_Dao_Result<IReadOnlyList<int>>> GetAllowedEmployeeNumbersAsync();

    /// <summary>
    /// Replaces the allow-list with the supplied employee numbers.
    /// </summary>
    Task<Model_Dao_Result> SaveAllowedEmployeeNumbersAsync(
        IReadOnlyCollection<int> employeeNumbers
    );

    /// <summary>
    /// Returns true when the supplied user may use the Scanner module. While no allow-list has
    /// been configured the legacy developer rule applies; once a list exists it is absolute and
    /// only listed employees are allowed.
    /// </summary>
    Task<bool> IsUserAllowedAsync(Model_User? user, string? fallbackWindowsUserName = null);

    /// <summary>
    /// Drops the cached allow-list so the next check re-reads the stored setting.
    /// </summary>
    void InvalidateCache();
}
