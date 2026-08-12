using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reporting.Models;

namespace MTM_Receiving_Application.Module_Reporting.Contracts;

public interface IService_ReportingSettings
{
    Task<Model_ReportingPreviewSettings> GetPreviewSettingsAsync(int? userId = null);

    Task<Model_Dao_Result> SavePreviewSettingsAsync(
        Model_ReportingPreviewSettings settings,
        int? userId = null
    );
}
