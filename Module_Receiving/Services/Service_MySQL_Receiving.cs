using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Receiving.Services
{
    /// <summary>
    /// Service for saving receiving load data to MySQL database.
    /// </summary>
    public class Service_MySQL_Receiving : IService_MySQL_Receiving
    {
        private readonly Dao_ReceivingLoad _receivingLoadDao;
        private readonly Dao_ReceivingLabelData _receivingLabelDataDao;
        private readonly IService_LoggingUtility _logger;

        public Service_MySQL_Receiving(
            Dao_ReceivingLoad receivingLoadDao,
            Dao_ReceivingLabelData receivingLabelDataDao,
            IService_LoggingUtility logger)
        {
            _receivingLoadDao = receivingLoadDao;
            _receivingLabelDataDao = receivingLabelDataDao;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Constructor for backward compatibility if needed, but DI should handle it
        public Service_MySQL_Receiving(string connectionString, IService_LoggingUtility logger)
        {
            _receivingLoadDao = new Dao_ReceivingLoad(connectionString);
            _receivingLabelDataDao = new Dao_ReceivingLabelData(connectionString);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> SaveReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
        {
            if (loads == null)
            {
                return 0;
            }

            _logger.LogInfo($"Saving {loads.Count} loads to receiving label queue table.");

            var result = await _receivingLabelDataDao.SaveLoadsAsync(loads);

            if (result.IsSuccess)
            {
                _logger.LogInfo($"Successfully saved {result.Data} loads.");
                return result.Data;
            }
            else
            {
                _logger.LogError($"Failed to save loads: {result.ErrorMessage}", result.Exception);
                throw new InvalidOperationException(result.ErrorMessage, result.Exception);
            }
        }

        public async Task<int> UpdateReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
        {
            if (loads == null)
            {
                return 0;
            }

            _logger.LogInfo($"Updating {loads.Count} loads in database.");

            var result = await _receivingLoadDao.UpdateLoadsAsync(loads);

            if (result.IsSuccess)
            {
                _logger.LogInfo($"Successfully updated {result.Data} loads.");
                return result.Data;
            }
            else
            {
                _logger.LogError($"Failed to update loads: {result.ErrorMessage}", result.Exception);
                throw new InvalidOperationException(result.ErrorMessage, result.Exception);
            }
        }

        public async Task<int> DeleteReceivingLoadsAsync(List<Model_ReceivingLoad> loads)
        {
            if (loads == null)
            {
                return 0;
            }

            _logger.LogInfo($"Deleting {loads.Count} loads from database.");

            var result = await _receivingLoadDao.DeleteLoadsAsync(loads);

            if (result.IsSuccess)
            {
                _logger.LogInfo($"Successfully deleted {result.Data} loads.");
                return result.Data;
            }
            else
            {
                _logger.LogError($"Failed to delete loads: {result.ErrorMessage}", result.Exception);
                throw new InvalidOperationException(result.ErrorMessage, result.Exception);
            }
        }

        public async Task<List<Model_ReceivingLoad>> GetReceivingHistoryAsync(string partID, DateTime startDate, DateTime endDate)
        {
            var result = await _receivingLoadDao.GetHistoryAsync(partID, startDate, endDate);

            if (result.IsSuccess)
            {
                return result.Data ?? new List<Model_ReceivingLoad>();
            }
            else
            {
                _logger.LogError($"Failed to get receiving history: {result.ErrorMessage}", result.Exception);
                // TODO: Consider whether to throw an exception or return an empty list. For now, returning empty list to avoid breaking calling code.
                return new List<Model_ReceivingLoad>();
            }
        }

        public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllReceivingLoadsAsync(DateTime startDate, DateTime endDate)
        {
            _logger.LogInfo($"Retrieving all receiving loads from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            var result = await _receivingLoadDao.GetAllAsync(startDate, endDate);

            if (result.IsSuccess)
            {
                _logger.LogInfo($"Retrieved {result.Data?.Count ?? 0} receiving loads from database");
                return result;
            }
            else
            {
                _logger.LogError($"Failed to retrieve receiving loads: {result.ErrorMessage}", result.Exception);
                return result;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            // TODO: Implement actual connection test logic, possibly by trying a simple query via DAO or adding a TestConnection method to DAO.
            return true;
        }

        public async Task<Model_Dao_Result<int>> ClearLabelDataToHistoryAsync(string archivedBy)
        {
            _logger.LogInfo($"Clearing receiving label queue to history by user: {archivedBy}");
            var result = await _receivingLabelDataDao.ClearLabelDataToHistoryAsync(archivedBy);

            if (result.IsSuccess)
            {
                _logger.LogInfo($"Clear Label Data completed. Rows moved: {result.Data}");
            }
            else
            {
                _logger.LogError($"Clear Label Data failed: {result.ErrorMessage}", result.Exception);
            }

            return result;
        }
    }
}
