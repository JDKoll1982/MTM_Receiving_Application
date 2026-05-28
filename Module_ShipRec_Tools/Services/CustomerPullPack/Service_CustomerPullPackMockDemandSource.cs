using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack;

public sealed class Service_CustomerPullPackMockDemandSource : IService_CustomerPullPackDemandSource
{
    private readonly IService_CustomerPullPackMockDataCatalog _mockDataCatalog;
    private readonly Service_CustomerPullPackMockWaitlistSource _mockWaitlistSource;
    private readonly IService_LoggingUtility? _logger;

    public Service_CustomerPullPackMockDemandSource(
        IService_CustomerPullPackMockDataCatalog mockDataCatalog,
        Service_CustomerPullPackMockWaitlistSource mockWaitlistSource,
        IService_LoggingUtility? logger = null
    )
    {
        _mockDataCatalog = mockDataCatalog;
        _mockWaitlistSource = mockWaitlistSource;
        _logger = logger;
    }

    public Task<Model_Dao_Result<List<Model_CustomerPullPack_DemandLine>>> GetDemandAsync(
        Model_CustomerPullPack_DemandFilter filter
    )
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (string.IsNullOrWhiteSpace(filter.CustomerId))
        {
            return Task.FromResult(
                Model_Dao_Result_Factory.Failure<List<Model_CustomerPullPack_DemandLine>>(
                    "Customer ID is required."
                )
            );
        }

        if (filter.DateTo < filter.DateFrom)
        {
            return Task.FromResult(
                Model_Dao_Result_Factory.Failure<List<Model_CustomerPullPack_DemandLine>>(
                    "DateTo cannot be earlier than DateFrom."
                )
            );
        }

        var normalizedCustomerId = filter.CustomerId.Trim().ToUpperInvariant();
        var locationRows = _mockDataCatalog.GetLocationRows();

        var demandLines = _mockDataCatalog
            .GetDemandRows()
            .Where(row =>
                string.Equals(
                    row.CustomerId,
                    normalizedCustomerId,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .Where(row =>
                row.PullDate.Date >= filter.DateFrom.Date && row.PullDate.Date <= filter.DateTo.Date
            )
            .Select(row => CreateDemandLine(row, locationRows))
            .ToList();

        return Task.FromResult(
            Model_Dao_Result_Factory.Success(
                Dao_CustomerPullPackDemand.ApplyLocalFiltersAndSorting(demandLines, filter, _logger)
            )
        );
    }

    private Model_CustomerPullPack_DemandLine CreateDemandLine(
        Model_InforVisualCustomerPullPackDemandRow row,
        IReadOnlyList<Model_InforVisualCustomerPullPackLocationRow> locationRows
    )
    {
        var linkedEntry = _mockWaitlistSource.GetBySourceLineKey(row.SourceLineKey);
        var locationOptions = Dao_CustomerPullPackDemand.BuildMockLocationOptions(
            row.SourceLineKey,
            row.ParentPartId,
            locationRows
        );

        if (linkedEntry is not null && linkedEntry.SelectedLocations.Count > 0)
        {
            foreach (var option in locationOptions)
            {
                option.Selected = linkedEntry.SelectedLocations.Contains(
                    option.LocationId,
                    StringComparer.OrdinalIgnoreCase
                );
            }
        }

        return new Model_CustomerPullPack_DemandLine
        {
            SourceLineKey = row.SourceLineKey,
            CustomerId = row.CustomerId,
            CustomerName = row.CustomerName,
            CustomerOrderId = row.CustomerOrderId,
            ParentPartId = row.ParentPartId,
            SourceLocationId = row.SourceLocationId,
            ShipQuantity = row.ShipQuantity,
            PullDate = row.PullDate,
            OldestAdded = row.PullDate,
            QuantityToPack = row.QuantityToPack,
            FgOnHandQuantity = row.FgOnHandQuantity,
            FgLocationId = row.FgLocationId,
            ShortageFlag = row.ShortageFlag,
            LateOrderFlag = row.LateOrderFlag,
            PulledFlag = row.PulledFlag,
            HasLinkedWaitlist = linkedEntry is not null || row.HasLinkedWaitlist,
            LinkedWaitlistId = linkedEntry?.WaitlistId ?? row.LinkedWaitlistId,
            WaitlistStateDisplay = Dao_CustomerPullPackDemand.BuildWaitlistStateDisplay(
                linkedEntry is not null || row.HasLinkedWaitlist,
                linkedEntry?.CurrentStatus.ToString() ?? row.LinkedWaitlistStatus,
                linkedEntry?.WaitlistId ?? row.LinkedWaitlistId
            ),
            RecheckIndicator = linkedEntry?.RecheckIndicator ?? row.RecheckIndicator,
            SubPartAvailabilitySummary = Dao_CustomerPullPackDemand.BuildMockAvailabilitySummary(
                row.SourceLineKey,
                row.ParentPartId,
                row.FgOnHandQuantity,
                locationRows
            ),
            RequesterNote = linkedEntry?.RequesterContextNote ?? row.RequesterNote,
            LocationOptions = locationOptions,
        };
    }
}
