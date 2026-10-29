using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
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
    /// The comparison is limited to the part numbers the receiving app itself received in the same
    /// range. Infor Visual records material that never flows through the app, so counting all of it
    /// would show a coverage gap that says nothing about what the app captured. Both coverage series
    /// and the ERP category split are built from the same filtered rows, so the two halves of the
    /// panel always cover the same material.
    /// <para>
    /// Both category splits on the returned model are left empty here on purpose. The panel's app
    /// side must be scoped to one unit of measure, and that choice belongs to the view model, so the
    /// view model builds the two splits over the five ERP names from the rows it cached and the
    /// comparison returned here. What this method alone can know exactly is the two coverage series,
    /// the ERP's own per-category line counts and whether the ERP half was readable.
    /// </para>
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

        var coverageResult = await _dao.GetCoverageLinesAsync(filter.StartDate, filter.EndDate);
        if (!coverageResult.IsSuccess)
        {
            _logger.LogError(
                $"ReceivingAnalytics comparison: app coverage-line read failed. {coverageResult.ErrorMessage}",
                coverageResult.Exception
            );
            return Model_Dao_Result_Factory.Failure<Model_Tool_ReceivingAnalyticsAppComparison>(
                coverageResult.ErrorMessage,
                coverageResult.Exception
            );
        }

        var appCoverageLines = coverageResult.Data ?? [];

        var partResult = await _dao.GetReceivedPartNumbersAsync(filter.StartDate, filter.EndDate);
        if (!partResult.IsSuccess)
        {
            _logger.LogError(
                $"ReceivingAnalytics comparison: app part number read failed, no ERP line will be drawn. {partResult.ErrorMessage}",
                partResult.Exception
            );

            return Model_Dao_Result_Factory.Success(
                new Model_Tool_ReceivingAnalyticsAppComparison
                {
                    AppCoverageSeries = BuildAppCoverage(appCoverageLines),
                    AppCategoryTotals = BuildAppCategoryTotals(appCoverageLines),
                    ErpAvailable = false,
                }
            );
        }

        var comparison = new Model_Tool_ReceivingAnalyticsAppComparison
        {
            AppCoverageSeries = BuildAppCoverage(appCoverageLines),
            AppCategoryTotals = BuildAppCategoryTotals(appCoverageLines),
        };

        var appPartNumbers = new HashSet<string>(
            (partResult.Data ?? []).Where(part => !string.IsNullOrWhiteSpace(part)),
            StringComparer.OrdinalIgnoreCase
        );

        var erpResult = await _inforVisual.GetReceivingAnalyticsCoverageByPartAsync(
            new Model_InforVisualReceivingAnalyticsFilter
            {
                FromDate = filter.StartDate,
                ToDate = filter.EndDate,
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

        var matchedErpRows = (erpResult.Data ?? [])
            .Where(point => appPartNumbers.Contains(point.PartId.Trim()))
            .ToList();

        comparison.ErpCoverageSeries = BuildErpCoverage(matchedErpRows);
        comparison.ErpCategoryTotals = BuildErpCategoryTotals(matchedErpRows);
        comparison.ErpAvailable = true;

        _logger.LogInfo(
            $"ReceivingAnalytics comparison: {appPartNumbers.Count} app part numbers, "
                + $"app {comparison.AppCoverageSeries.Count} dates, "
                + $"ERP {comparison.ErpCoverageSeries.Count} dates over {matchedErpRows.Count} matched part rows."
        );

        return Model_Dao_Result_Factory.Success(comparison);
    }

    /// <summary>
    /// One coverage point per received date for the app side, counting purchase-order lines the same
    /// way Infor Visual does: each line once, on the date it was last received.
    /// </summary>
    private static List<Model_Tool_ReceivingAnalyticsAppComparison.CoveragePoint> BuildAppCoverage(
        IReadOnlyList<Model_Tool_ReceivingAnalyticsAppCoverageLine> coverageLines
    )
    {
        return coverageLines
            .GroupBy(line => line.LastReceivedDate.Date)
            .Select(group => new Model_Tool_ReceivingAnalyticsAppComparison.CoveragePoint(
                group.Key,
                group.Sum(line => line.LineCount)
            ))
            .OrderBy(point => point.Date)
            .ToList();
    }

    /// <summary>
    /// Purchase-order line counts per ERP category for the app side. The app's part type reaches the
    /// ERP name through the one shared translation, so both halves of the mix name the same five
    /// categories over the same unit.
    /// </summary>
    private static Dictionary<string, int> BuildAppCategoryTotals(
        IReadOnlyList<Model_Tool_ReceivingAnalyticsAppCoverageLine> coverageLines
    )
    {
        var totals = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var line in coverageLines)
        {
            var erpCategory = Translation_ReceivingAnalyticsCategory.ToErpCategory(
                Translation_ReceivingAnalyticsCategory.FromPartNumber(line.PartId)
            );
            if (erpCategory is null)
            {
                continue;
            }

            totals[erpCategory] = totals.TryGetValue(erpCategory, out var running)
                ? running + line.LineCount
                : line.LineCount;
        }

        return totals;
    }

    /// <summary>One coverage point per received date for the ERP side, counted as the ERP counts.</summary>
    private static List<Model_Tool_ReceivingAnalyticsAppComparison.CoveragePoint> BuildErpCoverage(
        IReadOnlyList<Model_InforVisualReceivingCoveragePoint> erpPoints
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
        IReadOnlyList<Model_InforVisualReceivingCoveragePoint> erpPoints
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
