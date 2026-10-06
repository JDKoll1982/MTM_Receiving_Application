using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;

namespace MTM_Receiving_Application.Module_Shared.Contracts.Lookup;

/// <summary>
/// Expands an operator-typed location range into the warehouse locations that exist in Infor
/// Visual. Shared by every module that offers "search a span of locations" so formatting,
/// ordering, and auto-swap rules stay identical everywhere.
/// </summary>
public interface IService_SharedLocationRange
{
    /// <summary>
    /// Largest number of locations a single range may expand to. Ranges wider than this are
    /// truncated so one mistyped bound cannot flood the UI with queries.
    /// </summary>
    int MaxLocationsInRange { get; }

    /// <summary>
    /// Formats both bounds to the canonical location form, swaps them when the start sorts after
    /// the stop, then returns every existing location between them (inclusive) ordered by
    /// location ID.
    /// </summary>
    /// <param name="startInput">Raw start location typed or scanned by the operator.</param>
    /// <param name="stopInput">Raw stop location typed or scanned by the operator.</param>
    /// <param name="warehouseCode">Warehouse that scopes the range (for example, "002").</param>
    /// <param name="cancellationToken">Cancellation token for the Infor Visual round-trip.</param>
    Task<Model_Dao_Result<Model_SharedLocationRangeResult>> ResolveRangeAsync(
        string startInput,
        string stopInput,
        string warehouseCode,
        CancellationToken cancellationToken = default
    );
}
