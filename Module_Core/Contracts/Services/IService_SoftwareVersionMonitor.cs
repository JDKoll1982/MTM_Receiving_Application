using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Verifies the running application version against the required database version.
/// </summary>
public interface IService_SoftwareVersionMonitor
{
    Task<Model_Dao_Result> ValidateOnStartupAsync(XamlRoot? dialogXamlRoot = null);

    void StartMonitoring();

    void StopMonitoring();
}
