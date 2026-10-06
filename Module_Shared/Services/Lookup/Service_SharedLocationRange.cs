using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Shared.Contracts.Lookup;
using MTM_Receiving_Application.Module_Shared.Helpers;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;

namespace MTM_Receiving_Application.Module_Shared.Services.Lookup;

/// <summary>
/// Default location-range expander backed by the read-only Infor Visual LOCATION table.
/// </summary>
public sealed class Service_SharedLocationRange : IService_SharedLocationRange
{
    private readonly IService_InforVisual _inforVisualService;

    public Service_SharedLocationRange(IService_InforVisual inforVisualService)
    {
        _inforVisualService =
            inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
    }

    /// <inheritdoc />
    public int MaxLocationsInRange => 60;

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_SharedLocationRangeResult>> ResolveRangeAsync(
        string startInput,
        string stopInput,
        string warehouseCode,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var start = Helper_SharedLocationFormat.Sanitize(startInput);
        if (start is null)
        {
            return Model_Dao_Result_Factory.Failure<Model_SharedLocationRangeResult>(
                $"'{startInput?.Trim()}' is not a recognizable start location. Enter a location such as V-A0-01, R-04, or S-00."
            );
        }

        var stop = Helper_SharedLocationFormat.Sanitize(stopInput);
        if (stop is null)
        {
            return Model_Dao_Result_Factory.Failure<Model_SharedLocationRangeResult>(
                $"'{stopInput?.Trim()}' is not a recognizable stop location. Enter a location such as V-A0-01, R-04, or S-00."
            );
        }

        var wasSwapped = string.Compare(start, stop, StringComparison.OrdinalIgnoreCase) > 0;
        if (wasSwapped)
        {
            (start, stop) = (stop, start);
        }

        // Ask for one extra row so an exactly-full result can be told apart from a truncated one.
        var requested = MaxLocationsInRange + 1;
        var rangeResult = await _inforVisualService.GetLocationsInRangeAsync(
            start,
            stop,
            warehouseCode,
            requested
        );

        if (!rangeResult.IsSuccess || rangeResult.Data is null)
        {
            return Model_Dao_Result_Factory.Failure<Model_SharedLocationRangeResult>(
                rangeResult.ErrorMessage,
                rangeResult.Exception
            );
        }

        var orderedLocations = rangeResult
            .Data.Select(row => row.LocationId)
            .Where(locationId => string.IsNullOrWhiteSpace(locationId) is false)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(locationId => locationId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var wasTruncated = orderedLocations.Count > MaxLocationsInRange;
        var locations = wasTruncated
            ? orderedLocations.Take(MaxLocationsInRange).ToList()
            : orderedLocations;

        return Model_Dao_Result_Factory.Success(
            new Model_SharedLocationRangeResult
            {
                StartLocation = start,
                StopLocation = stop,
                WasSwapped = wasSwapped,
                Locations = locations,
                WasTruncated = wasTruncated,
            }
        );
    }
}
