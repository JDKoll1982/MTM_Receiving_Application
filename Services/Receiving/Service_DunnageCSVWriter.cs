using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Dunnage;

namespace MTM_Receiving_Application.Services.Receiving
{
    public class Service_DunnageCSVWriter : IService_DunnageCSVWriter
    {
        private readonly IService_MySQL_Dunnage _dunnageService;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly ILoggingService _logger;
        private readonly IService_ErrorHandler _errorHandler;

        public Service_DunnageCSVWriter(
            IService_MySQL_Dunnage dunnageService,
            IService_UserSessionManager sessionManager,
            ILoggingService logger,
            IService_ErrorHandler errorHandler)
        {
            _dunnageService = dunnageService;
            _sessionManager = sessionManager;
            _logger = logger;
            _errorHandler = errorHandler;
        }

        public async Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_DunnageLoad> loads)
        {
            if (loads == null || loads.Count == 0)
            {
                return new Model_CSVWriteResult { ErrorMessage = "No loads to export." };
            }

            try
            {
                // FR-046: Get dynamic spec keys
                var specKeys = await _dunnageService.GetAllSpecKeysAsync();
                specKeys.Sort(); // FR-042: Alphabetically sorted

                // Prepare records
                var records = new List<dynamic>();
                foreach (var load in loads)
                {
                    dynamic record = new ExpandoObject();
                    var dict = (IDictionary<string, object>)record;

                    // FR-041: Fixed columns
                    dict["DunnageType"] = load.DunnageType;
                    dict["PartID"] = load.PartId;
                    dict["Quantity"] = load.Quantity;
                    dict["PONumber"] = load.PoNumber;
                    dict["ReceivedDate"] = load.ReceivedDate.ToString("yyyy-MM-dd HH:mm:ss");
                    dict["Employee"] = load.CreatedBy;

                    // FR-042: Dynamic columns
                    foreach (var key in specKeys)
                    {
                        if (load.Specs != null && load.Specs.ContainsKey(key))
                        {
                            dict[key] = load.Specs[key];
                        }
                        else
                        {
                            dict[key] = ""; // FR-043: Empty string if missing
                        }
                    }
                    records.Add(record);
                }

                // Write to local path
                var localPath = GetLocalPath();
                await WriteCsvFileAsync(localPath, records);

                // Write to network path
                string networkPath = "";
                bool networkSuccess = false;
                try
                {
                    networkPath = GetNetworkPath();
                    // Ensure directory exists for network path
                    var networkDir = Path.GetDirectoryName(networkPath);
                    if (!string.IsNullOrEmpty(networkDir) && !Directory.Exists(networkDir))
                    {
                        Directory.CreateDirectory(networkDir);
                    }
                    await WriteCsvFileAsync(networkPath, records);
                    networkSuccess = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to write to network path", ex, "Service_DunnageCSVWriter.WriteToCSVAsync");
                    // FR-044: Network failure graceful handling
                }

                return new Model_CSVWriteResult
                {
                    LocalSuccess = true,
                    LocalFilePath = localPath,
                    NetworkSuccess = networkSuccess,
                    NetworkFilePath = networkPath,
                    ErrorMessage = networkSuccess ? "" : "Network write failed. See logs."
                };
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync($"Export failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
                return new Model_CSVWriteResult { ErrorMessage = $"Export failed: {ex.Message}" };
            }
        }

        private async Task WriteCsvFileAsync(string path, IEnumerable<dynamic> records)
        {
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(records);
            }
        }

        private string GetLocalPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "MTM_Receiving_Application");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, "DunnageData.csv");
        }

        private string GetNetworkPath()
        {
            var username = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "Unknown";
            var folder = $@"\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\{username}";
            return Path.Combine(folder, "DunnageData.csv");
        }
    }
}
