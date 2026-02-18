using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Services
{
    /// <summary>
    /// Placeholder stub — XLS file workflow is being replaced by MySQL.
    /// TODO(SpreadsheetRemoval): Replace all methods with MySQL-backed implementations.
    /// </summary>
    public class Service_DunnageXLSWriter : IService_DunnageXLSWriter
    {
        private const string NotImplementedMessage = "Not implemented yet: spreadsheet workflow is being replaced by MySQL.";
        private readonly IService_LoggingUtility _logger;

        public Service_DunnageXLSWriter(
            IService_MySQL_Dunnage dunnageService,
            IService_UserSessionManager sessionManager,
            IService_LoggingUtility logger,
            IService_ErrorHandler errorHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Model_XLSWriteResult> WriteToXLSAsync(List<Model_DunnageLoad> loads)
        {
            await _logger.LogWarningAsync($"{nameof(WriteToXLSAsync)} called — {NotImplementedMessage}");
            return new Model_XLSWriteResult { ErrorMessage = NotImplementedMessage };
        }

        public async Task<Model_XLSWriteResult> WriteToXLSAsync(List<Model_DunnageLoad> loads, string typeName)
        {
            await _logger.LogWarningAsync($"{nameof(WriteToXLSAsync)} called — {NotImplementedMessage}");
            return new Model_XLSWriteResult { ErrorMessage = NotImplementedMessage };
        }

        public async Task<Model_XLSWriteResult> WriteDynamicXLSAsync(List<Model_DunnageLoad> loads, List<string> allSpecKeys, string? filename = null)
        {
            await _logger.LogWarningAsync($"{nameof(WriteDynamicXLSAsync)} called — {NotImplementedMessage}");
            return new Model_XLSWriteResult { ErrorMessage = NotImplementedMessage };
        }

        public async Task<Model_XLSWriteResult> ExportSelectedLoadsAsync(List<Model_DunnageLoad> selectedLoads, bool includeAllSpecColumns = false)
        {
            await _logger.LogWarningAsync($"{nameof(ExportSelectedLoadsAsync)} called — {NotImplementedMessage}");
            return new Model_XLSWriteResult { ErrorMessage = NotImplementedMessage };
        }

        public Task<bool> IsNetworkPathAvailableAsync(int timeout = 3)
        {
            _logger.LogWarning($"{nameof(IsNetworkPathAvailableAsync)} called — {NotImplementedMessage}");
            return Task.FromResult(false);
        }

        public string GetLocalXLSPath(string filename)
        {
            _logger.LogWarning($"{nameof(GetLocalXLSPath)} called — {NotImplementedMessage}");
            return string.Empty;
        }

        public string GetNetworkXLSPath(string filename)
        {
            _logger.LogWarning($"{nameof(GetNetworkXLSPath)} called — {NotImplementedMessage}");
            return string.Empty;
        }

        public async Task<Model_XLSDeleteResult> ClearXLSFilesAsync(string? filenamePattern = null)
        {
            await _logger.LogWarningAsync($"{nameof(ClearXLSFilesAsync)} called — {NotImplementedMessage}");
            return new Model_XLSDeleteResult { NetworkError = NotImplementedMessage };
        }
    }
}
