using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Data;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Services;

/// <summary>
/// Service for managing routing label history.
/// Handles archiving labels and retrieving historical data with date-based grouping.
/// </summary>
public class Service_Routing_History : IService_Routing_History
{
    private readonly Dao_Routing_Label _daoLabel;
    private readonly IService_LoggingUtility _logger;

    public Service_Routing_History(
        Dao_Routing_Label daoLabel,
        IService_LoggingUtility logger)
    {
        _daoLabel = daoLabel ?? throw new ArgumentNullException(nameof(daoLabel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Model_Dao_Result<int>> ArchiveLabelsAsync(List<int> labelIds)
    {
        try
        {
            if (labelIds == null || labelIds.Count == 0)
            {
                return Model_Dao_Result_Factory.Failure<int>("No labels provided for archiving");
            }

            _logger.LogInfo($"Archiving {labelIds.Count} routing labels");
            return await _daoLabel.ArchiveAsync(labelIds);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error archiving labels: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<int>($"Error archiving labels: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_Routing_Label>>> GetHistoryByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInfo($"Retrieving routing history from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            if (startDate > endDate)
            {
                return Model_Dao_Result_Factory.Failure<List<Model_Routing_Label>>("Start date must be before end date");
            }

            return await _daoLabel.GetHistoryByDateRangeAsync(startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving history: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Routing_Label>>($"Error retrieving history: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<Dictionary<DateTime, List<Model_Routing_Label>>>> GetHistoryGroupedByDateAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInfo($"Retrieving routing history grouped by date from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            // Get history data
            var historyResult = await GetHistoryByDateRangeAsync(startDate, endDate);
            if (!historyResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<Dictionary<DateTime, List<Model_Routing_Label>>>(historyResult.Message);
            }

            // Group by date
            var groupedData = historyResult.Data
                .GroupBy(l => l.CreatedDate.Date)
                .OrderByDescending(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderBy(l => l.LabelNumber).ToList());

            _logger.LogInfo($"Retrieved {groupedData.Count} days of history with {historyResult.Data.Count} total labels");
            return Model_Dao_Result_Factory.Success(groupedData, $"Retrieved {groupedData.Count} days of history");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error grouping history by date: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Dictionary<DateTime, List<Model_Routing_Label>>>($"Error grouping history: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_Routing_Label>>> GetTodayHistoryAsync()
    {
        try
        {
            _logger.LogInfo("Retrieving today's routing history");
            var today = DateTime.Today;
            return await _daoLabel.GetHistoryByDateRangeAsync(today, today);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving today's history: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Routing_Label>>($"Error retrieving today's history: {ex.Message}", ex);
        }
    }
}
