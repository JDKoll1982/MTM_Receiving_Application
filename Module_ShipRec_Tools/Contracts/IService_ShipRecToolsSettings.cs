using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
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

    /// <summary>Returns the system-wide Heat / Lot Management switch. Defaults to on.</summary>
    Task<bool> GetLotMarkingEnabledAsync();

    /// <summary>Returns what the resolver does when a computed marking is already held.</summary>
    Task<Enum_LotMarkingCollisionPolicy> GetLotMarkingCollisionPolicyAsync();

    /// <summary>Returns the shape glyphs the palette may hand out.</summary>
    Task<IReadOnlyCollection<string>> GetLotMarkingEnabledShapesAsync();

    /// <summary>Returns the color codes the palette may hand out.</summary>
    Task<IReadOnlyCollection<string>> GetLotMarkingEnabledColorsAsync();

    /// <summary>Returns whether a marking draws its shape before its color.</summary>
    Task<bool> GetLotMarkingShapeFirstAsync();

    /// <summary>Returns the on-time target percentage for Receiving Analytics. Null when no target is configured.</summary>
    Task<decimal?> GetReceivingAnalyticsOnTimeTargetAsync();
}
