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

    /// <summary>
    /// Lists the lots that receiving lines carried inside the range, each with the marking the ledger
    /// holds for it, how many receiving lines carry it, when it was last seen, and which part numbers
    /// carry it. Optionally narrowed by a search over lot and part numbers, and by the one part number
    /// a user has chosen.
    /// </summary>
    /// <remarks>
    /// Read only, and not to be confused with <see cref="GetAllMarkingsAsync"/>, which the resolver's
    /// probe reads on every new lot and which stays as lean as it is for that reason. The counts, dates
    /// and parts are worked out when the list is read and are never stored.
    /// The list is read from the receiving lines rather than from the ledger, so material that came in
    /// is listed even when its marking was never recorded, or was recorded under a lot number or a
    /// supplier spelled differently. A lot like that shows the codes its own lines carry, which is what
    /// a handler saw on the receipt, and nothing is recorded behind it until the ledger catches up.
    /// With no part chosen the list is already scoped: it holds a lot only when at least one enabled
    /// part carries it.
    /// </remarks>
    /// <param name="searchText">
    /// What the list is narrowed to, or null for the whole of it. The text is matched against the lot
    /// number, ignoring separators and case, and against the part numbers the lot is carried by, so a
    /// handler who knows either one can narrow the list with one box.
    /// </param>
    /// <param name="partId">
    /// The part number to narrow the list to, or null for the whole of the enabled scope.
    /// </param>
    /// <param name="filter">
    /// The date range and order to read the list by, or null for no date limit and newest first.
    /// </param>
    Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> GetLedgerAsync(
        string? searchText,
        string? partId = null,
        Model_Tool_LotMarkingFilter? filter = null
    );

    /// <summary>Finds markings whose heat number contains the supplied text.</summary>
    Task<Model_Dao_Result<List<Model_Tool_LotMarking>>> SearchMarkingsByHeatAsync(string? searchText);

    /// <summary>
    /// Returns the marking for a lot, creating it the first time the lot is seen.
    /// Returns <c>null</c> data when the lot has no heat number, which is not an error.
    /// A marking already recorded is always returned unchanged.
    /// </summary>
    /// <remarks>
    /// A marking is only ever created for a part number on the enabled list. The list is consulted
    /// here, when a marking would be created, and never when a marking is read, so a lot recorded
    /// before its part was removed still resolves to the marking a handler has already seen.
    /// The lot is looked for under the supplier group first and then under the supplier name the
    /// receipt carries, because one vendor can be keyed by its official id on one receipt and by its
    /// name on the next. Without that second look the same physical lot would be handed a second
    /// marking, and a handler would see one pile of material in two colors.
    /// </remarks>
    /// <param name="partId">
    /// The part number on the receipt. The lot gets no marking unless this part is on the enabled
    /// list, and an empty value gets no marking at all because a marking that cannot be attributed
    /// to a part cannot be shown to belong to one.
    /// </param>
    /// <param name="supplierKey">
    /// The supplier group: the official supplier id when the caller resolved one, otherwise the
    /// supplier name. Resolving a name to the official id is the caller's job, because the
    /// vendor lookup belongs to the receiving side. An empty value uses the shared group for
    /// receipts with no supplier.
    /// </param>
    /// <param name="receiptSupplierName">
    /// The supplier as the receipt spells it, when the caller has it. It is the second spelling a lot
    /// is looked for under, and it is what a new marking records as the supplier name so the two
    /// spellings stay related. Null when the caller has only the group.
    /// </param>
    Task<Model_Dao_Result<Model_Tool_LotMarking?>> ResolveMarkingAsync(
        string? heatNumber,
        string? partId,
        string? supplierKey,
        Enum_LotMarkingCollisionPolicy policy,
        string? receiptSupplierName = null
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

    /// <summary>
    /// Works out everything Update All Lots would do, writing nothing: which marked rows no longer
    /// agree with the list for the lot number they now carry, which lot numbers would become lots, and
    /// which lots no longer appear on any receiving row.
    /// </summary>
    /// <remarks>
    /// Safe to call before the user confirms. A lot whose marking is protected is never planned for
    /// change or for removal, and is reported instead, because protection means a person deliberately
    /// fixed that marking.
    /// </remarks>
    Task<Model_Dao_Result<Model_Tool_LotMarkingRealignPreview>> PreviewUpdateAllLotsAsync();

    /// <summary>
    /// Applies Update All Lots: rows are given the marking their lot number now calls for, lot numbers
    /// that had none become lots, and lots whose number appears on no receiving row are removed so
    /// their shape and color are free again.
    /// </summary>
    /// <remarks>
    /// The plan is worked out again from the data as it stands at the moment it runs rather than taken
    /// as an argument, so an interval between the report and the confirmation can never cause a stale
    /// change.
    /// </remarks>
    Task<Model_Dao_Result<Model_Tool_LotMarkingRealignResult>> UpdateAllLotsAsync(
        string actingUser
    );
}
