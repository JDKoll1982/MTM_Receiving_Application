using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Reads and persists application-level settings stored in appsettings JSON files.
/// </summary>
public interface IService_AppSettings
{
    bool GetUseInforVisualMockData();

    string? GetDefaultMockPONumber();

    Task<Model_Dao_Result> SetUseInforVisualMockDataAsync(bool value);
}
