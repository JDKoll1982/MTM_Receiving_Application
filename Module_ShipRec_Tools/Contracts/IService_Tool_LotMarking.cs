using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;

/// <summary>
/// Service contract for the Heat / Lot Management tool: resolves a lot's shape and color,
/// keeps the enabled-part list, and lets a user correct and protect a marking.
/// </summary>
public interface IService_Tool_LotMarking
{
    /// <summary>Lists every recorded marking, newest first.</summary>
    Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> GetAllMarkingsAsync();

    /// <summary>Finds markings whose heat number contains the supplied text.</summary>
    Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> SearchMarkingsByHeatAsync(string? searchText);

    /// <summary>
    /// Returns the marking for a lot, creating it the first time the lot is seen.
    /// Returns <c>null</c> data when the lot has no heat number, which is not an error.
    /// A marking already recorded is always returned unchanged.
    /// </summary>
    /// <param name="supplierKey">
    /// The supplier group: the official supplier id when the caller resolved one, otherwise the
    /// supplier name. Resolving a name to the official id is the caller's job, because the
    /// vendor lookup belongs to the receiving side. An empty value uses the shared group for
    /// receipts with no supplier.
    /// </param>
    Task<Model_Dao_Result<Model_Tool_LotMarking?>> ResolveMarkingAsync(
        string? heatNumber,
        string? supplierKey,
        Enum_LotMarkingCollisionPolicy policy
    );

    /// <summary>Replaces a marking by hand. A reason is required and the acting user is recorded.</summary>
    Task<Model_Dao_Result> OverrideMarkingAsync(
        int id,
        string shapeGlyph,
        string colorCode,
        string reason,
        string actingUser
    );

    /// <summary>Protects a marking from being recomputed, or releases it.</summary>
    Task<Model_Dao_Result> SetMarkingLockedAsync(int id, bool isLocked, string actingUser);

    /// <summary>Lists the part numbers that receive markings, enabled ones first.</summary>
    Task<Model_Dao_Result<List<Model_Tool_LotMarkingScope>>> GetEnabledPartsAsync();

    /// <summary>Finds enabled-list rows whose part number contains the supplied text.</summary>
    Task<Model_Dao_Result<List<Model_Tool_LotMarkingScope>>> SearchPartsAsync(string? searchText);

    /// <summary>Adds a part number to the enabled list, or puts a removed part back.</summary>
    Task<Model_Dao_Result<int>> AddPartAsync(string partId, string actingUser);

    /// <summary>Removes a part from the enabled list without touching markings already recorded.</summary>
    Task<Model_Dao_Result> SetPartEnabledAsync(int id, bool isEnabled);

    /// <summary>Deletes a list row outright, for correcting a part added by mistake.</summary>
    Task<Model_Dao_Result> RemovePartAsync(int id);

    /// <summary>Loads a marking onto an existing receiving queue row.</summary>
    Task<Model_Dao_Result> ApplyMarkingToQueueRowAsync(
        string loadId,
        string? shapeGlyph,
        string? colorCode
    );

    /// <summary>
    /// Reports how many rows and how many distinct lots filling in the unmarked lots of one part
    /// would cover. Writes nothing, so it is safe to call before the user confirms.
    /// </summary>
    /// <remarks>
    /// Two spellings that normalize to one heat number count as one lot, and a row whose heat
    /// number is missing or blank is not a lot at all and is not counted.
    /// </remarks>
    Task<Model_Dao_Result<Model_Tool_LotMarkingBackfillPreview>> PreviewUnmarkedLotMarkingsAsync(
        string partId
    );

    /// <summary>
    /// Fills in every unmarked lot of one part, returning the number of rows written. Only rows
    /// that carry no marking are touched, so running it twice writes nothing the second time.
    /// </summary>
    /// <remarks>
    /// Does nothing and returns <c>0</c> when the system-wide switch is off or when there is
    /// nothing unmarked. A lot that already has a recorded marking is filled in with that same
    /// marking rather than a new one, because a marking is never recomputed.
    /// </remarks>
    Task<Model_Dao_Result<int>> FillUnmarkedLotMarkingsAsync(string partId, string actingUser);
}
