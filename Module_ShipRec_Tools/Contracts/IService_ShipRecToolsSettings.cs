using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

/// <summary>
/// Provides ShipRec tools settings backed by the shared Settings.Core infrastructure.
/// </summary>
public interface IService_ShipRecToolsSettings
{
    Task<Model_Tool_MaterialAvailabilityFieldSettings> GetMaterialAvailabilityFieldSettingsAsync();

    Task<Model_Tool_POLineSpecSearchOptions> GetPOLineSpecSearchOptionsAsync();

    Task<Model_Dao_Result> SavePOLineSpecSearchOptionsAsync(
        Model_Tool_POLineSpecSearchOptions options
    );
}
