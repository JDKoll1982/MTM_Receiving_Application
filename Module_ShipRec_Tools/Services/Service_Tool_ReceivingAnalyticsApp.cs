using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services;

/// <summary>
/// Service for the Receiving Analytics App Data tab. Reads the app's own completed work from
/// MySQL through <see cref="Dao_Tool_ReceivingAnalyticsApp"/> for the three app panels, and reads
/// Infor Visual (read-only) for the comparison panel. Expected failures are returned as typed
/// results; nothing here throws for them.
/// </summary>
public sealed class Service_Tool_ReceivingAnalyticsApp : IService_Tool_ReceivingAnalyticsApp
{
    private readonly Dao_Tool_ReceivingAnalyticsApp _dao;
    private readonly IService_InforVisual _inforVisual;
    private readonly IService_LoggingUtility _logger;

    public Service_Tool_ReceivingAnalyticsApp(
        Dao_Tool_ReceivingAnalyticsApp dao,
        IService_InforVisual inforVisual,
        IService_LoggingUtility logger
    )
    {
        _dao = dao ?? throw new ArgumentNullException(nameof(dao));
        _inforVisual = inforVisual ?? throw new ArgumentNullException(nameof(inforVisual));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_Tool_ReceivingAnalyticsAppDataset>> GetAppDataAsync(
        Model_Tool_ReceivingAnalyticsAppFilter filter
    )
    {
        ArgumentNullException.ThrowIfNull(filter);

        var activityTask = _dao.GetActivityByDayAsync(filter.StartDate, filter.EndDate);
        var onTimeTask = _dao.GetOnTimeByDayAsync(filter.StartDate, filter.EndDate);
        var supplierTask = _dao.GetSupplierTotalsAsync(filter.StartDate, filter.EndDate);
        var missingTask = _dao.GetMissingLoadNumberCountAsync(filter.StartDate, filter.EndDate);

        await Task.WhenAll([activityTask, onTimeTask, supplierTask, missingTask]);

        var failures = new List<string>();
        Exception? firstException = null;

        void Collect(string read, Model_Dao_Result result)
        {
            if (result.IsSuccess)
            {
                return;
            }

            failures.Add($"{read}: {result.ErrorMessage}");
            firstException ??= result.Exception;
        }

        Collect("activity by day", activityTask.Result);
        Collect("on time by day", onTimeTask.Result);
        Collect("supplier totals", supplierTask.Result);
        Collect("missing load number count", missingTask.Result);

        if (failures.Count > 0)
        {
            var message = "Receiving Analytics app data failed. " + string.Join("; ", failures);
            _logger.LogError(message, firstException);
            return Model_Dao_Result_Factory.Failure<Model_Tool_ReceivingAnalyticsAppDataset>(
                message,
                firstException
            );
        }

        var dataset = new Model_Tool_ReceivingAnalyticsAppDataset
        {
            ActivityRows = activityTask.Result.Data ?? [],
            OnTimeRows = onTimeTask.Result.Data ?? [],
            SupplierRows = supplierTask.Result.Data ?? [],
            MissingLoadNumberCount = missingTask.Result.Data,
        };

        _logger.LogInfo(
            $"ReceivingAnalytics app data: {dataset.ActivityRows.Count} activity rows, "
                + $"{dataset.OnTimeRows.Count} on-time rows, {dataset.SupplierRows.Count} supplier rows, "
                + $"{dataset.MissingLoadNumberCount} receipts without a load number."
        );

        return Model_Dao_Result_Factory.Success(dataset);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Both category splits on the returned model are left empty here on purpose. The panel's app
    /// side must be scoped to one unit of measure, and that choice belongs to the view model, so the
    /// view model builds the two splits over the five ERP names from the rows it cached and the
    /// comparison returned here. What this method alone can know exactly is the two coverage series,
    /// the ERP's own per-category line counts and whether the ERP half was readable.
    /// </remarks>
    public async Task<
        Model_Dao_Result<Model_Tool_ReceivingAnalyticsAppComparison>
    > GetComparisonAsync(Model_Tool_ReceivingAnalyticsAppFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var activityResult = await _dao.GetActivityByDayAsync(filter.StartDate, filter.EndDate);
        if (!activityResult.IsSuccess)
        {
            _logger.LogError(
                $"ReceivingAnalytics comparison: app activity read failed. {activityResult.ErrorMessage}",
                activityResult.Exception
            );
            return Model_Dao_Result_Factory.Failure<Model_Tool_ReceivingAnalyticsAppComparison>(
                activityResult.ErrorMessage,
                activityResult.Exception
            );
        }

        var comparison = new Model_Tool_ReceivingAnalyticsAppComparison
        {
            AppCoverageSeries = BuildAppCoverage(activityResult.Data ?? []),
        };

        var erpResult = await _inforVisual.GetReceivingAnalyticsHistoryAsync(
            new Model_InforVisualReceivingAnalyticsFilter
            {
                FromDate = filter.StartDate,
                ToDate = filter.EndDate,
                ScopeParts = true,
                ScopeCoils = true,
                ScopeFlat = true,
                ScopeOutside = true,
                ScopeUninventoried = true,
                MaxResults = 50000,
            }
        );

        if (!erpResult.IsSuccess)
        {
            _logger.LogError(
                $"ReceivingAnalytics comparison: ERP read failed, comparison panel left without an ERP line. {erpResult.ErrorMessage}",
                erpResult.Exception
            );
            comparison.ErpAvailable = false;
            return Model_Dao_Result_Factory.Success(comparison);
        }

        comparison.ErpCoverageSeries = BuildErpCoverage(
            erpResult.Data ?? new List<Model_InforVisualReceivingAnalyticsPoint>()
        );
        comparison.ErpCategoryTotals = BuildErpCategoryTotals(
            erpResult.Data ?? new List<Model_InforVisualReceivingAnalyticsPoint>()
        );
        comparison.ErpAvailable = true;

        _logger.LogInfo(
            $"ReceivingAnalytics comparison: app {comparison.AppCoverageSeries.Count} dates, "
                + $"ERP {comparison.ErpCoverageSeries.Count} dates."
        );

        return Model_Dao_Result_Factory.Success(comparison);
    }

    /// <summary>
    /// One coverage point per received date for the app side. A receipt row's label count is a day
    /// total repeated on every category and unit row of that day, so it is read once per day and
    /// never summed across rows.
    /// </summary>
    private static List<Model_Tool_ReceivingAnalyticsAppComparison.CoveragePoint> BuildAppCoverage(
        IReadOnlyList<Model_Tool_ReceivingAnalyticsAppActivityRow> activityRows
    )
    {
        return activityRows
            .GroupBy(row => row.BucketDate.Date)
            .Select(group => new Model_Tool_ReceivingAnalyticsAppComparison.CoveragePoint(
                group.Key,
                group.Max(row => row.LabelCount)
            ))
            .OrderBy(point => point.Date)
            .ToList();
    }

    /// <summary>One coverage point per received date for the ERP side, counted as the ERP counts.</summary>
    private static List<Model_Tool_ReceivingAnalyticsAppComparison.CoveragePoint> BuildErpCoverage(
        IReadOnlyList<Model_InforVisualReceivingAnalyticsPoint> erpPoints
    )
    {
        return erpPoints
            .GroupBy(point => point.ActivityDate.Date)
            .Select(group => new Model_Tool_ReceivingAnalyticsAppComparison.CoveragePoint(
                group.Key,
                group.Sum(point => point.LineCount)
            ))
            .OrderBy(point => point.Date)
            .ToList();
    }

    /// <summary>
    /// Receipt line counts per ERP category over the same dates. The names are the ERP's own, so
    /// Outside Service and Uninventoried stay separate rather than being folded into a category
    /// the app has.
    /// </summary>
    private static Dictionary<string, int> BuildErpCategoryTotals(
        IReadOnlyList<Model_InforVisualReceivingAnalyticsPoint> erpPoints
    )
    {
        var totals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var point in erpPoints)
        {
            if (string.IsNullOrWhiteSpace(point.Category))
            {
                continue;
            }

            var category = point.Category.Trim();
            totals[category] = totals.TryGetValue(category, out var running)
                ? running + point.LineCount
                : point.LineCount;
        }

        return totals;
    }
}
