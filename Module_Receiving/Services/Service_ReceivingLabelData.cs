using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Module_Receiving.Services
{
    /// <summary>
    /// Placeholder stub — label-data file workflow is being replaced by MySQL.
    /// TODO: Replace all methods with database-backed implementations.
    /// </summary>
    public class Service_ReceivingLabelData : IService_ReceivingLabelData
    {
        private const string NotImplementedMessage =
            "Not implemented yet: spreadsheet workflow is being replaced by MySQL.";
        private readonly IService_LoggingUtility _logger;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_SettingsCoreFacade _settingsCore;

        public Service_ReceivingLabelData(
            IService_UserSessionManager sessionManager,
            IService_LoggingUtility logger,
            IService_SettingsCoreFacade settingsCore
        )
        {
            _sessionManager =
                sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        }

        public async Task<Model_LabelDataSaveResult> SaveLabelDataAsync(
            List<Model_ReceivingLoad> loads
        )
        {
            await _logger.LogWarningAsync(
                $"{nameof(SaveLabelDataAsync)} called — {NotImplementedMessage}"
            );
            return new Model_LabelDataSaveResult { ErrorMessage = NotImplementedMessage };
        }

        public async Task SaveLabelDataToPathAsync(
            string filePath,
            List<Model_ReceivingLoad> loads,
            bool append = true
        )
        {
            await _logger.LogWarningAsync(
                $"{nameof(SaveLabelDataToPathAsync)} called — {NotImplementedMessage}"
            );
        }

        public async Task<List<Model_ReceivingLoad>> LoadLabelDataAsync(string filePath)
        {
            await _logger.LogWarningAsync(
                $"{nameof(LoadLabelDataAsync)} called — {NotImplementedMessage}"
            );
            return new List<Model_ReceivingLoad>();
        }

        public async Task<Model_LabelDataClearResult> ClearLabelDataAsync()
        {
            await _logger.LogWarningAsync(
                $"{nameof(ClearLabelDataAsync)} called — {NotImplementedMessage}"
            );
            return new Model_LabelDataClearResult { ArchiveQueueError = NotImplementedMessage };
        }

        public async Task<Model_LabelDataAvailabilityResult> CheckLabelDataAvailabilityAsync()
        {
            await _logger.LogWarningAsync(
                $"{nameof(CheckLabelDataAvailabilityAsync)} called — {NotImplementedMessage}"
            );
            return new Model_LabelDataAvailabilityResult
            {
                ArchiveQueueAvailable = false,
                ArchiveQueueAccessible = false,
            };
        }

        public Task<string> GetLabelDataPathAsync()
        {
            _logger.LogWarning($"{nameof(GetLabelDataPathAsync)} called — {NotImplementedMessage}");
            return Task.FromResult(string.Empty);
        }
    }
}
