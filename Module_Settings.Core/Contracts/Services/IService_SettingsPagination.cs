using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;

/// <summary>
/// Provides pagination helpers for settings workflow hubs.
/// </summary>
public interface IService_SettingsPagination
{
    /// <summary>
    /// Number of step buttons per page.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Returns true when pagination should be shown for the given total item count.
    /// </summary>
    /// <param name="totalCount"></param>
    bool ShouldShowPagination(int totalCount);

    /// <summary>
    /// Calculates total pages given total count.
    /// </summary>
    /// <param name="totalCount"></param>
    int GetTotalPages(int totalCount);

    /// <summary>
    /// Returns 0-based indices for items on the requested 1-based page.
    /// </summary>
    /// <param name="totalCount"></param>
    /// <param name="pageNumber"></param>
    IReadOnlyList<int> GetPageIndices(int totalCount, int pageNumber);
}
