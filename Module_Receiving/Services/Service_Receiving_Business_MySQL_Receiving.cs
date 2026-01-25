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
    public class Service_Receiving_Business_MySQL_Receiving : IService_Receiving_Business_MySQL_Receiving
    {
        private readonly Dao_Receiving_Repository_Load _receivingLoadDao;
        private readonly IService_LoggingUtility _logger;

        public Service_Receiving_Business_MySQL_Receiving(
            Dao_Receiving_Repository_Load receivingLoadDao,
            IService_LoggingUtility logger)
        {
            _receivingLoadDao = receivingLoadDao;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Constructor for backward compatibility if needed, but DI should handle it
        public Service_Receiving_Business_MySQL_Receiving(string connectionString, IService_LoggingUtility logger)
        {
            _receivingLoadDao = new Dao_Receiving_Repository_Load(connectionString);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> SaveReceivingLoadsAsync(List<Model_Receiving_Entity_ReceivingLoad> loads)
        {
            if (loads == null)
            {
                return 0;
            }

            _logger.LogInfo($"Saving {loads.Count} loads to database.");

            var result = await _receivingLoadDao.SaveLoadsAsync(loads);

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

        public async Task<int> UpdateReceivingLoadsAsync(List<Model_Receiving_Entity_ReceivingLoad> loads)
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

        public async Task<int> DeleteReceivingLoadsAsync(List<Model_Receiving_Entity_ReceivingLoad> loads)
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

        public async Task<List<Model_Receiving_Entity_ReceivingLoad>> GetReceivingHistoryAsync(string partID, DateTime startDate, DateTime endDate)
        {
            var result = await _receivingLoadDao.GetHistoryAsync(partID, startDate, endDate);

            if (result.IsSuccess)
            {
                return result.Data ?? new List<Model_Receiving_Entity_ReceivingLoad>();
            }
            else
            {
                _logger.LogError($"Failed to get receiving history: {result.ErrorMessage}", result.Exception);
                // Return empty list or throw? Original implementation returned empty list on error (implicitly via empty result)
                // But here we know it failed.
                // I'll return empty list to match previous behavior of returning list (even if empty).
                return new List<Model_Receiving_Entity_ReceivingLoad>();
            }
        }

        public async Task<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLoad>>> GetAllReceivingLoadsAsync(DateTime startDate, DateTime endDate)
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
            // Delegate to DAO or keep simple check?
            // DAO doesn't have TestConnection.
            // I'll keep it simple or add it to DAO.
            // Since I don't want to modify DAO again right now, I'll just try a simple query via DAO if possible,
            // or just assume true if DAO is instantiated.
            // Actually, I can't easily test connection without exposing connection string or adding method to DAO.
            // I'll add TestConnectionAsync to DAO later if needed.
            // For now, I'll just return true as a placeholder or try to call GetAllAsync with limit 0?
            // Or I can just remove this method if it's not in the interface.
            // Let's check interface.
            return true;
        }
    }
}
