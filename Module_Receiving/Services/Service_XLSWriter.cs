using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Receiving.Services
{
    /// <summary>
    /// Placeholder stub — XLS file workflow is being replaced by MySQL.
    /// TODO: Replace all methods with database-backed implementations.
    /// </summary>
    public class Service_XLSWriter : IService_XLSWriter
    {
        private const string NotImplementedMessage = "Not implemented yet: spreadsheet workflow is being replaced by MySQL.";
        private readonly IService_LoggingUtility _logger;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_SettingsCoreFacade _settingsCore;

        public Service_XLSWriter(
            IService_UserSessionManager sessionManager,
            IService_LoggingUtility logger,
            IService_SettingsCoreFacade settingsCore)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        }

        public async Task<Model_XLSWriteResult> WriteToXLSAsync(List<Model_ReceivingLoad> loads)
        {
            await _logger.LogWarningAsync($"{nameof(WriteToXLSAsync)} called — {NotImplementedMessage}");
            return new Model_XLSWriteResult { ErrorMessage = NotImplementedMessage };
        }

        public async Task WriteToFileAsync(string filePath, List<Model_ReceivingLoad> loads, bool append = true)
        {
            await _logger.LogWarningAsync($"{nameof(WriteToFileAsync)} called — {NotImplementedMessage}");
        }

        public async Task<List<Model_ReceivingLoad>> ReadFromXLSAsync(string filePath)
        {
            await _logger.LogWarningAsync($"{nameof(ReadFromXLSAsync)} called — {NotImplementedMessage}");
            return new List<Model_ReceivingLoad>();
        }

        public async Task<Model_XLSDeleteResult> ClearXLSFilesAsync()
        {
            await _logger.LogWarningAsync($"{nameof(ClearXLSFilesAsync)} called — {NotImplementedMessage}");
            return new Model_XLSDeleteResult { NetworkError = NotImplementedMessage };
        }

        public async Task<Model_XLSExistenceResult> CheckXLSFilesExistAsync()
        {
            await _logger.LogWarningAsync($"{nameof(CheckXLSFilesExistAsync)} called — {NotImplementedMessage}");
            return new Model_XLSExistenceResult { NetworkExists = false, NetworkAccessible = false };
        }

        public Task<string> GetNetworkXLSPathAsync()
        {
            _logger.LogWarning($"{nameof(GetNetworkXLSPathAsync)} called — {NotImplementedMessage}");
            return Task.FromResult(string.Empty);
        }
    }
}

