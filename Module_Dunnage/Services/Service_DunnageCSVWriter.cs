using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.IO;
using System.Dynamic;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
namespace MTM_Receiving_Application.Module_Dunnage.Services
{
    public class Service_DunnageCSVWriter : IService_DunnageCSVWriter
    {
        private readonly IService_MySQL_Dunnage _dunnageService;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_LoggingUtility _logger;
        private readonly IService_ErrorHandler _errorHandler;

        public Service_DunnageCSVWriter(
            IService_MySQL_Dunnage dunnageService,
            IService_UserSessionManager sessionManager,
            IService_LoggingUtility logger,
            IService_ErrorHandler errorHandler)
        {
            _dunnageService = dunnageService;
            _sessionManager = sessionManager;
            _logger = logger;
            _errorHandler = errorHandler;
        }

        /// <summary>
        /// Get RFC 4180 compliant CSV configuration
        /// T103-T109: RFC 4180 CSV Formatting
        /// </summary>
        private CsvConfiguration GetRfc4180Configuration()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // T103: RFC 4180 settings
                Delimiter = ",",              // Standard comma delimiter
                Quote = '"',                  // Double quote for field escaping
                HasHeaderRecord = true,       // Include header row

                // T106: CRLF line endings
                NewLine = "\r\n",            // RFC 4180 requires CRLF

                // T104: Automatic escaping for special characters (commas, quotes, newlines)
                ShouldQuote = args => !string.IsNullOrEmpty(args.Field) &&
                                     (args.Field.Contains(",") ||
                                      args.Field.Contains("\"") ||
                                      args.Field.Contains("\n") ||
                                      args.Field.Contains("\r")),

                // T105: UTF-8 with BOM for Excel/LabelView compatibility
                Encoding = Encoding.UTF8,

                // T108: Invariant culture for numeric formatting (period as decimal separator)
                // Already handled by CultureInfo.InvariantCulture
            };
        }

        public async Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_DunnageLoad> loads)
        {
            // Default to DunnageData.csv for backward compatibility
            return await WriteToCsvAsync(loads, "DunnageData");
        }

        /// <summary>
        /// Write dunnage loads to CSV file (wizard workflow export)
        /// Uses fixed columns based on selected type's specs
        /// </summary>
        /// <param name="loads"></param>
        /// <param name="typeName"></param>
        public async Task<Model_CSVWriteResult> WriteToCsvAsync(List<Model_DunnageLoad> loads, string typeName)
        {
            if (loads == null || loads.Count == 0)
            {
                await _logger.LogWarningAsync("WriteToCsvAsync called with no loads to export");
                return new Model_CSVWriteResult { ErrorMessage = "No loads to export." };
            }

            try
            {
                await _logger.LogInfoAsync($"Starting CSV export for {loads.Count} loads of type '{typeName}'");

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
                    dict["ReceivedDate"] = FormatDateTime(load.ReceivedDate); // T109: yyyy-MM-dd HH:mm:ss
                    dict["Employee"] = load.CreatedBy;

                    // FR-042: Dynamic columns
                    foreach (var key in specKeys)
                    {
                        if (load.Specs != null && load.Specs.TryGetValue(key, out object? value))
                        {
                            dict[key] = value;
                        }
                        else
                        {
                            dict[key] = ""; // FR-043: Empty string if missing
                        }
                    }
                    records.Add(record);
                }

                // Write to local path
                string filename = $"{typeName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var localPath = GetLocalCsvPath(filename);
                await WriteCsvFileAsync(localPath, records);
                await _logger.LogInfoAsync($"Successfully wrote {records.Count} records to local CSV: {localPath}");

                // Write to network path
                string networkPath = "";
                bool networkSuccess = false;
                try
                {
                    networkPath = GetNetworkCsvPath(filename);
                    // Ensure directory exists for network path
                    var networkDir = Path.GetDirectoryName(networkPath);
                    if (!string.IsNullOrEmpty(networkDir) && !Directory.Exists(networkDir))
                    {
                        Directory.CreateDirectory(networkDir);
                    }
                    await WriteCsvFileAsync(networkPath, records);
                    networkSuccess = true;
                    await _logger.LogInfoAsync($"Successfully wrote {records.Count} records to network CSV: {networkPath}");
                }
                catch (Exception ex)
                {
                    await _logger.LogErrorAsync($"Failed to write to network path '{networkPath}': {ex.Message}");
                    // FR-044: Network failure graceful handling
                }

                return new Model_CSVWriteResult
                {
                    LocalSuccess = true,
                    LocalFilePath = localPath,
                    NetworkSuccess = networkSuccess,
                    NetworkFilePath = networkPath,
                    RecordsWritten = records.Count,
                    ErrorMessage = networkSuccess ? "" : "Network write failed. See logs."
                };
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"CSV export failed for type '{typeName}': {ex.Message}");
                await _errorHandler.HandleErrorAsync($"Export failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
                return new Model_CSVWriteResult { ErrorMessage = $"Export failed: {ex.Message}" };
            }
        }

        /// <summary>
        /// Write dunnage loads to CSV with dynamic columns for all spec keys
        /// Used for Manual Entry and Edit Mode exports (all types in one file)
        /// </summary>
        /// <param name="loads"></param>
        /// <param name="allSpecKeys"></param>
        /// <param name="filename"></param>
        public async Task<Model_CSVWriteResult> WriteDynamicCsvAsync(
            List<Model_DunnageLoad> loads,
            List<string> allSpecKeys,
            string? filename = null)
        {
            if (loads == null || loads.Count == 0)
            {
                await _logger.LogWarningAsync("WriteDynamicCsvAsync called with no loads to export");
                return new Model_CSVWriteResult { ErrorMessage = "No loads to export." };
            }

            try
            {
                await _logger.LogInfoAsync($"Starting dynamic CSV export for {loads.Count} loads");

                // Use provided spec keys or fetch all
                var specKeys = allSpecKeys ?? await _dunnageService.GetAllSpecKeysAsync();
                specKeys.Sort(); // Alphabetically sorted

                // Prepare records
                var records = new List<dynamic>();
                foreach (var load in loads)
                {
                    dynamic record = new ExpandoObject();
                    var dict = (IDictionary<string, object>)record;

                    // Fixed columns
                    dict["ID"] = load.LoadUuid;
                    dict["PartID"] = load.PartId;
                    dict["DunnageType"] = load.DunnageType ?? string.Empty;
                    dict["Quantity"] = load.Quantity;
                    dict["PONumber"] = load.PoNumber ?? string.Empty;
                    dict["ReceivedDate"] = FormatDateTime(load.ReceivedDate); // T109: yyyy-MM-dd HH:mm:ss
                    dict["UserId"] = load.CreatedBy;
                    dict["Location"] = load.Location ?? string.Empty;
                    dict["LabelNumber"] = load.LabelNumber ?? string.Empty;

                    // Dynamic spec columns
                    foreach (var key in specKeys)
                    {
                        if (load.Specs != null && load.Specs.TryGetValue(key, out object? value))
                        {
                            dict[key] = value;
                        }
                        else
                        {
                            dict[key] = ""; // Blank cell for specs not applicable to this load's type
                        }
                    }
                    records.Add(record);
                }

                // Generate filename if not provided
                string csvFilename = filename ?? $"DunnageData_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                // Write to local path
                var localPath = GetLocalCsvPath(csvFilename);
                await WriteCsvFileAsync(localPath, records);
                await _logger.LogInfoAsync($"Successfully wrote {records.Count} records to local CSV: {localPath}");

                // Write to network path (best-effort)
                string networkPath = "";
                bool networkSuccess = false;
                string? networkError = null;

                if (await IsNetworkPathAvailableAsync())
                {
                    try
                    {
                        networkPath = GetNetworkCsvPath(csvFilename);
                        var networkDir = Path.GetDirectoryName(networkPath);
                        if (!string.IsNullOrEmpty(networkDir) && !Directory.Exists(networkDir))
                        {
                            Directory.CreateDirectory(networkDir);
                        }
                        await WriteCsvFileAsync(networkPath, records);
                        networkSuccess = true;
                        await _logger.LogInfoAsync($"Successfully wrote {records.Count} records to network CSV: {networkPath}");
                    }
                    catch (Exception ex)
                    {
                        networkError = ex.Message;
                        await _logger.LogErrorAsync($"Failed to write to network path '{networkPath}': {ex.Message}");
                    }
                }
                else
                {
                    await _logger.LogWarningAsync("Network path not available for CSV export");
                }

                return new Model_CSVWriteResult
                {
                    LocalSuccess = true,
                    LocalFilePath = localPath,
                    NetworkSuccess = networkSuccess,
                    NetworkFilePath = networkPath,
                    NetworkError = networkError,
                    RecordsWritten = records.Count
                };
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync($"Dynamic CSV export failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
                return new Model_CSVWriteResult
                {
                    LocalSuccess = false,
                    ErrorMessage = $"Export failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Export selected loads from DataGrid (Manual Entry or Edit Mode)
        /// Includes dynamic spec columns based on types in selection
        /// </summary>
        /// <param name="selectedLoads"></param>
        /// <param name="includeAllSpecColumns"></param>
        public async Task<Model_CSVWriteResult> ExportSelectedLoadsAsync(
            List<Model_DunnageLoad> selectedLoads,
            bool includeAllSpecColumns = false)
        {
            if (selectedLoads == null || selectedLoads.Count == 0)
            {
                return new Model_CSVWriteResult { ErrorMessage = "No loads selected for export." };
            }

            try
            {
                List<string> specKeys;

                if (includeAllSpecColumns)
                {
                    // Include all spec keys across all types
                    specKeys = await _dunnageService.GetAllSpecKeysAsync();
                }
                else
                {
                    // Only include spec keys used by selected loads' types
                    var typeIds = selectedLoads
                        .Where(l => l.TypeId.HasValue)
                        .Select(l => l.TypeId!.Value)
                        .Distinct()
                        .ToList();

                    specKeys = new List<string>();
                    foreach (var typeId in typeIds)
                    {
                        var specs = await _dunnageService.GetSpecsForTypeAsync(typeId);
                        if (specs.IsSuccess && specs.Data != null)
                        {
                            var keys = specs.Data.Select(s => s.SpecKey).Distinct();
                            specKeys.AddRange(keys);
                        }
                    }
                    specKeys = specKeys.Distinct().ToList();
                }

                string filename = $"DunnageSelection_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return await WriteDynamicCsvAsync(selectedLoads, specKeys, filename);
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync($"Selected loads export failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
                return new Model_CSVWriteResult { ErrorMessage = $"Export failed: {ex.Message}" };
            }
        }

        /// <summary>
        /// Validate network path availability (for dual-path writing)
        /// </summary>
        /// <param name="timeout"></param>
        public async Task<bool> IsNetworkPathAvailableAsync(int timeout = 3)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var networkRoot = @"\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files";
                    return Directory.Exists(networkRoot);
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Get local CSV file path for current user
        /// </summary>
        /// <param name="filename"></param>
        public string GetLocalCsvPath(string filename)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "MTM_Receiving_Application");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, filename);
        }

        /// <summary>
        /// Get network CSV file path for current user
        /// </summary>
        /// <param name="filename"></param>
        public string GetNetworkCsvPath(string filename)
        {
            var username = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "Unknown";
            var folder = $@"\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\{username}";
            return Path.Combine(folder, filename);
        }

        private async Task WriteCsvFileAsync(string path, IEnumerable<dynamic> records)
        {
            // T105: UTF-8 with BOM for Excel/LabelView compatibility
            var encoding = new UTF8Encoding(true); // true = include BOM

            await using (var writer = new StreamWriter(path, false, encoding))
            await using (var csv = new CsvWriter(writer, GetRfc4180Configuration()))
            {
                await csv.WriteRecordsAsync(records);
            }
        }

        /// <summary>
        /// Format date/time values as yyyy-MM-dd HH:mm:ss (T109)
        /// </summary>
        /// <param name="value"></param>
        private string FormatDateTime(DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Clear CSV files from both local and network paths
        /// If filenamePattern is provided, only clear files matching the pattern
        /// If null, clear all CSV files in the user directories
        /// </summary>
        /// <param name="filenamePattern">Optional pattern to match (e.g., "DunnageData*.csv")</param>
        /// <returns>Result indicating success/failure for local and network operations</returns>
        public async Task<Model_CSVDeleteResult> ClearCSVFilesAsync(string? filenamePattern = null)
        {
            var result = new Model_CSVDeleteResult();
            var errors = new List<string>();

            try
            {
                await _logger.LogInfoAsync($"Starting CSV file clearing with pattern: {filenamePattern ?? "all"}");

                // Clear local files
                try
                {
                    var localFolder = Path.GetDirectoryName(GetLocalCsvPath("dummy.csv"));
                    if (Directory.Exists(localFolder))
                    {
                        var filesToClear = Directory.GetFiles(localFolder, filenamePattern ?? "*.csv");
                        foreach (var file in filesToClear)
                        {
                            // Overwrite with header only
                            await using var writer = new StreamWriter(file, false, new UTF8Encoding(true));
                            await using var csv = new CsvWriter(writer, GetRfc4180Configuration());
                            csv.WriteHeader<Model_DunnageLoad>();
                            csv.NextRecord();

                            await _logger.LogInfoAsync($"Cleared local CSV file: {file}");
                        }
                        result.LocalDeleted = true;
                    }
                    else
                    {
                        result.LocalDeleted = true; // No directory means no files to clear
                        await _logger.LogInfoAsync("Local CSV directory does not exist - no files to clear");
                    }
                }
                catch (Exception ex)
                {
                    result.LocalDeleted = false;
                    result.LocalError = $"Local clearing failed: {ex.Message}";
                    errors.Add(result.LocalError);
                    await _logger.LogErrorAsync($"Failed to clear local CSV files: {ex.Message}");
                }

                // Clear network files (best-effort)
                try
                {
                    if (await IsNetworkPathAvailableAsync())
                    {
                        var networkFolder = Path.GetDirectoryName(GetNetworkCsvPath("dummy.csv"));
                        if (Directory.Exists(networkFolder))
                        {
                            var filesToClear = Directory.GetFiles(networkFolder, filenamePattern ?? "*.csv");
                            foreach (var file in filesToClear)
                            {
                                await using var writer = new StreamWriter(file, false, new UTF8Encoding(true));
                                await using var csv = new CsvWriter(writer, GetRfc4180Configuration());
                                csv.WriteHeader<Model_DunnageLoad>();
                                csv.NextRecord();

                                await _logger.LogInfoAsync($"Cleared network CSV file: {file}");
                            }
                            result.NetworkDeleted = true;
                        }
                        else
                        {
                            result.NetworkDeleted = true; // No directory means no files to clear
                            await _logger.LogInfoAsync("Network CSV directory does not exist - no files to clear");
                        }
                    }
                    else
                    {
                        result.NetworkDeleted = false;
                        result.NetworkError = "Network path not available";
                        errors.Add("Network path not available");
                        await _logger.LogWarningAsync("Network path not available for CSV clearing");
                    }
                }
                catch (Exception ex)
                {
                    result.NetworkDeleted = false;
                    result.NetworkError = $"Network clearing failed: {ex.Message}";
                    errors.Add(result.NetworkError);
                    await _logger.LogErrorAsync($"Failed to clear network CSV files: {ex.Message}");
                }

                await _logger.LogInfoAsync($"CSV clearing completed. Local: {result.LocalDeleted}, Network: {result.NetworkDeleted}");
            }
            catch (Exception ex)
            {
                var msg = $"Unexpected error during CSV clearing: {ex.Message}";
                result.LocalError = result.LocalError ?? msg;
                await _errorHandler.HandleErrorAsync($"CSV clearing failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
            }

            return result;
        }
    }
}


