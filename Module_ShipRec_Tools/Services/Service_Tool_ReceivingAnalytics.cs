using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services;

/// <summary>
/// Service for the Receiving Analytics tool. Reads the History (past received)
/// and Forecast (incoming) datasets from Infor Visual (read-only), maps them to
/// chart points, and computes summary statistics.
/// </summary>
public class Service_Tool_ReceivingAnalytics : IService_Tool_ReceivingAnalytics
{
    private readonly IService_InforVisual _inforVisual;
    private readonly IService_LoggingUtility _logger;

    public Service_Tool_ReceivingAnalytics(
        IService_InforVisual inforVisual,
        IService_LoggingUtility logger
    )
    {
        _inforVisual = inforVisual ?? throw new ArgumentNullException(nameof(inforVisual));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_Tool_ReceivingAnalytics>> GetAnalyticsAsync(
        Model_InforVisualReceivingAnalyticsFilter filter
    )
    {
        var historyResult = await _inforVisual.GetReceivingAnalyticsHistoryAsync(filter);
        if (!historyResult.IsSuccess)
        {
            _logger.LogError(
                $"ReceivingAnalytics: failed to load history. {historyResult.ErrorMessage}",
                historyResult.Exception
            );
            return Model_Dao_Result_Factory.Failure<Model_Tool_ReceivingAnalytics>(
                historyResult.ErrorMessage,
                historyResult.Exception
            );
        }

        var forecastResult = await _inforVisual.GetReceivingAnalyticsForecastAsync(filter);
        if (!forecastResult.IsSuccess)
        {
            _logger.LogError(
                $"ReceivingAnalytics: failed to load forecast. {forecastResult.ErrorMessage}",
                forecastResult.Exception
            );
            return Model_Dao_Result_Factory.Failure<Model_Tool_ReceivingAnalytics>(
                forecastResult.ErrorMessage,
                forecastResult.Exception
            );
        }

        var result = new Model_Tool_ReceivingAnalytics
        {
            History = MapPoints(historyResult.Data ?? []),
            Forecast = MapPoints(forecastResult.Data ?? []),
        };

        _logger.LogInfo(
            $"ReceivingAnalytics: history {result.History.Count}, forecast {result.Forecast.Count} data points."
        );
        return Model_Dao_Result_Factory.Success(result);
    }

    /// <inheritdoc />
    public Task<Model_Tool_ReceivingAnalyticsStats> ComputeStatsAsync(
        IReadOnlyList<Model_Tool_ReceivingAnalyticsPoint> currentPoints,
        IReadOnlyList<Model_Tool_ReceivingAnalyticsPoint>? previousPeriodPoints = null
    )
    {
        if (currentPoints is null || currentPoints.Count == 0)
        {
            return Task.FromResult(new Model_Tool_ReceivingAnalyticsStats());
        }

        var distinctDates = currentPoints.Select(p => p.Date.Date).Distinct().ToList();
        var perDayTotals = currentPoints
            .GroupBy(p => p.Date.Date)
            .Select(g => g.Sum(p => p.LineCount))
            .ToList();

        var totalLines = currentPoints.Sum(p => p.LineCount);
        var peakDay = perDayTotals.Count == 0 ? 0 : perDayTotals.Max();
        var avgPerDay = distinctDates.Count == 0 ? 0.0 : (double)totalLines / distinctDates.Count;

        // Trend vs previous equivalent period: total in the current window compared
        // against the caller-supplied previous window (same length, immediately prior).
        var previousTotal = previousPeriodPoints?.Sum(p => p.LineCount) ?? 0;

        double trend = 0.0;
        if (previousTotal > 0)
        {
            trend = Math.Round(
                ((double)(totalLines - previousTotal) / previousTotal) * 100.0,
                1
            );
        }

        return Task.FromResult(
            new Model_Tool_ReceivingAnalyticsStats
            {
                TotalLines = totalLines,
                AvgPerDay = Math.Round(avgPerDay, 1),
                PeakDay = peakDay,
                TrendVsLastWeek = trend,
            }
        );
    }

    private static List<Model_Tool_ReceivingAnalyticsPoint> MapPoints(
        IReadOnlyList<Model_InforVisualReceivingAnalyticsPoint> source
    )
    {
        return source
            .Select(
                row =>
                    new Model_Tool_ReceivingAnalyticsPoint
                    {
                        Date = row.ActivityDate,
                        Category = row.Category,
                        LineCount = row.LineCount,
                    }
            )
            .OrderBy(p => p.Date)
            .ThenBy(p => p.Category)
            .ToList();
    }
}
