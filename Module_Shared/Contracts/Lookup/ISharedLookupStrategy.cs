using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Shared.Enums;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;

namespace MTM_Receiving_Application.Module_Shared.Contracts.Lookup;

/// <summary>
/// Per-domain lookup strategy used by the shared typed lookup workflow.
/// </summary>
public interface ISharedLookupStrategy
{
    Enum_SharedLookupType LookupType { get; }

    Model_Dao_Result ValidateRawInput(string rawInput);

    Model_SharedLookupFormattingResult ApplyFormatting(Model_SharedLookupRequest request);

    Task<Model_Dao_Result<bool>> HasExactMatchAsync(
        string formattedValue,
        Model_SharedLookupRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> SearchFuzzyAsync(
        string formattedValue,
        Model_SharedLookupRequest request,
        CancellationToken cancellationToken = default
    );

    string SelectBestFuzzyResult(
        string formattedValue,
        IReadOnlyList<Model_FuzzySearchResult> candidates
    );
}
