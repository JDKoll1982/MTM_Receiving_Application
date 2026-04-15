using System.Threading.Tasks;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

/// <summary>
/// Provides ShipRec tools settings backed by the shared Settings.Core infrastructure.
/// </summary>
public interface IService_ShipRecToolsSettings
{
    Task<Model_Tool_MaterialAvailabilityFieldSettings> GetMaterialAvailabilityFieldSettingsAsync();
}
