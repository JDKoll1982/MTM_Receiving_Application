using System;
using System.Collections.Generic;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Pagination helpers for settings workflow hubs.
/// </summary>
public sealed class Service_SettingsPagination : IService_SettingsPagination
{
    public int PageSize => 6;

    public bool ShouldShowPagination(int totalCount)
    {
        if (totalCount <= 0)
        {
            return false;
        }

        return totalCount > PageSize;
    }

    public int GetTotalPages(int totalCount)
    {
        if (totalCount <= 0)
        {
            return 0;
        }

        return (int)Math.Ceiling(totalCount / (double)PageSize);
    }

    public IReadOnlyList<int> GetPageIndices(int totalCount, int pageNumber)
    {
        if (totalCount <= 0)
        {
            return Array.Empty<int>();
        }

        var totalPages = GetTotalPages(totalCount);
        if (totalPages == 0)
        {
            return Array.Empty<int>();
        }

        var safePageNumber = Math.Clamp(pageNumber, 1, totalPages);
        var startIndex = (safePageNumber - 1) * PageSize;
        var endIndexExclusive = Math.Min(startIndex + PageSize, totalCount);

        var indices = new List<int>(Math.Max(0, endIndexExclusive - startIndex));
        for (var i = startIndex; i < endIndexExclusive; i++)
        {
            indices.Add(i);
        }

        return indices;
    }
}
