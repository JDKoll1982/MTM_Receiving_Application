using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Dynamic;
using ClosedXML.Excel;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Dunnage.Services
{
    public class Service_DunnageXLSWriter : IService_DunnageXLSWriter
    {
        private readonly IService_MySQL_Dunnage _dunnageService;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_LoggingUtility _logger;
        private readonly IService_ErrorHandler _errorHandler;

        public Service_DunnageXLSWriter(
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

        public async Task<Model_XLSWriteResult> WriteToXLSAsync(List<Model_DunnageLoad> loads)
        {
            // Default to DunnageData.xls for backward compatibility
            return await WriteToXLSAsync(loads, "DunnageData");
        }

        public async Task<Model_XLSWriteResult> WriteToXLSAsync(List<Model_DunnageLoad> loads, string typeName)
        {
            if (loads == null || loads.Count == 0)
            {
                await _logger.LogWarningAsync("WriteToXLSAsync called with no loads to export");
                return new Model_XLSWriteResult { ErrorMessage = "No loads to export." };
            }

            try
            {
                await _logger.LogInfoAsync($"Starting XLS export for {loads.Count} loads of type '{typeName}'");

                // Get dynamic spec keys
                var specKeys = await _dunnageService.GetAllSpecKeysAsync();
                specKeys.Sort();

                // Write to local path
                string filename = $"{typeName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var localPath = GetLocalXLSPath(filename);
                await WriteXlsFileAsync(localPath, loads, specKeys);
                await _logger.LogInfoAsync($"Successfully wrote {loads.Count} records to local XLS: {localPath}");

                // Write to network path
                string networkPath = "";
                bool networkSuccess = false;
                try
                {
                    networkPath = GetNetworkXLSPath(filename);
                    var networkDir = Path.GetDirectoryName(networkPath);
                    if (!string.IsNullOrEmpty(networkDir) && !Directory.Exists(networkDir))
                    {
                        Directory.CreateDirectory(networkDir);
                    }
                    await WriteXlsFileAsync(networkPath, loads, specKeys);
                    networkSuccess = true;
                    await _logger.LogInfoAsync($"Successfully wrote {loads.Count} records to network XLS: {networkPath}");
                }
                catch (Exception ex)
                {
                    await _logger.LogErrorAsync($"Failed to write to network path '{networkPath}': {ex.Message}");
                }

                return new Model_XLSWriteResult
                {
                    LocalSuccess = true,
                    LocalFilePath = localPath,
                    NetworkSuccess = networkSuccess,
                    NetworkFilePath = networkPath,
                    RecordsWritten = loads.Count,
                    ErrorMessage = networkSuccess ? "" : "Network write failed. See logs."
                };
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"XLS export failed for type '{typeName}': {ex.Message}");
                await _errorHandler.HandleErrorAsync($"Export failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
                return new Model_XLSWriteResult { ErrorMessage = $"Export failed: {ex.Message}" };
            }
        }

        public async Task<Model_XLSWriteResult> WriteDynamicXLSAsync(
            List<Model_DunnageLoad> loads,
            List<string> allSpecKeys,
            string? filename = null)
        {
            if (loads == null || loads.Count == 0)
            {
                await _logger.LogWarningAsync("WriteDynamicXLSAsync called with no loads to export");
                return new Model_XLSWriteResult { ErrorMessage = "No loads to export." };
            }

            try
            {
                await _logger.LogInfoAsync($"Starting dynamic XLS export for {loads.Count} loads");

                var specKeys = allSpecKeys ?? await _dunnageService.GetAllSpecKeysAsync();
                specKeys.Sort();

                string xlsFilename = filename ?? $"DunnageData_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                var localPath = GetLocalXLSPath(xlsFilename);
                await WriteDynamicXLSFileAsync(localPath, loads, specKeys);
                await _logger.LogInfoAsync($"Successfully wrote {loads.Count} records to local XLS: {localPath}");

                string networkPath = "";
                bool networkSuccess = false;
                string? networkError = null;

                if (await IsNetworkPathAvailableAsync())
                {
                    try
                    {
                        networkPath = GetNetworkXLSPath(xlsFilename);
                        var networkDir = Path.GetDirectoryName(networkPath);
                        if (!string.IsNullOrEmpty(networkDir) && !Directory.Exists(networkDir))
                        {
                            Directory.CreateDirectory(networkDir);
                        }
                        await WriteDynamicXLSFileAsync(networkPath, loads, specKeys);
                        networkSuccess = true;
                        await _logger.LogInfoAsync($"Successfully wrote {loads.Count} records to network XLS: {networkPath}");
                    }
                    catch (Exception ex)
                    {
                        networkError = ex.Message;
                        await _logger.LogErrorAsync($"Failed to write to network path '{networkPath}': {ex.Message}");
                    }
                }
                else
                {
                    await _logger.LogWarningAsync("Network path not available for XLS export");
                }

                return new Model_XLSWriteResult
                {
                    LocalSuccess = true,
                    LocalFilePath = localPath,
                    NetworkSuccess = networkSuccess,
                    NetworkFilePath = networkPath,
                    NetworkError = networkError,
                    RecordsWritten = loads.Count
                };
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync($"Dynamic XLS export failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
                return new Model_XLSWriteResult
                {
                    LocalSuccess = false,
                    ErrorMessage = $"Export failed: {ex.Message}"
                };
            }
        }

        public async Task<Model_XLSWriteResult> ExportSelectedLoadsAsync(
            List<Model_DunnageLoad> selectedLoads,
            bool includeAllSpecColumns = false)
        {
            if (selectedLoads == null || selectedLoads.Count == 0)
            {
                return new Model_XLSWriteResult { ErrorMessage = "No loads selected for export." };
            }

            try
            {
                List<string> specKeys;

                if (includeAllSpecColumns)
                {
                    specKeys = await _dunnageService.GetAllSpecKeysAsync();
                }
                else
                {
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

                string filename = $"DunnageSelection_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return await WriteDynamicXLSAsync(selectedLoads, specKeys, filename);
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync($"Selected loads export failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
                return new Model_XLSWriteResult { ErrorMessage = $"Export failed: {ex.Message}" };
            }
        }

        public async Task<bool> IsNetworkPathAvailableAsync(int timeout = 3)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var networkRoot = @"\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User XLS Files";
                    return Directory.Exists(networkRoot);
                }
                catch
                {
                    return false;
                }
            });
        }

        public string GetLocalXLSPath(string filename)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "MTM_Receiving_Application");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, filename);
        }

        public string GetNetworkXLSPath(string filename)
        {
            var username = _sessionManager.CurrentSession?.User?.WindowsUsername ?? "Unknown";
            var folder = $@"\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User XLS Files\{username}";
            return Path.Combine(folder, filename);
        }

        private async Task WriteXlsFileAsync(string path, List<Model_DunnageLoad> loads, List<string> specKeys)
        {
            await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Dunnage Data");

                // Fixed columns
                int col = 1;
                worksheet.Cell(1, col++).Value = "DunnageType";
                worksheet.Cell(1, col++).Value = "PartID";
                worksheet.Cell(1, col++).Value = "Quantity";
                worksheet.Cell(1, col++).Value = "PONumber";
                worksheet.Cell(1, col++).Value = "ReceivedDate";
                worksheet.Cell(1, col++).Value = "Employee";

                // Dynamic spec columns
                foreach (var key in specKeys)
                {
                    worksheet.Cell(1, col++).Value = key;
                }

                // Format header
                var headerRange = worksheet.Range(1, 1, 1, col - 1);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Write data rows
                int row = 2;
                foreach (var load in loads)
                {
                    col = 1;
                    worksheet.Cell(row, col++).Value = load.DunnageType ?? "";
                    worksheet.Cell(row, col++).Value = load.PartId ?? "";
                    worksheet.Cell(row, col++).Value = load.Quantity;
                    worksheet.Cell(row, col++).Value = load.PoNumber ?? "";
                    worksheet.Cell(row, col++).Value = load.ReceivedDate.ToString("yyyy-MM-dd HH:mm:ss");
                    worksheet.Cell(row, col++).Value = load.CreatedBy ?? "";

                    foreach (var key in specKeys)
                    {
                        if (load.Specs != null && load.Specs.TryGetValue(key, out object? value))
                        {
                            worksheet.Cell(row, col++).Value = value?.ToString() ?? "";
                        }
                        else
                        {
                            worksheet.Cell(row, col++).Value = "";
                        }
                    }
                    row++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(path);
            });
        }

        private async Task WriteDynamicXLSFileAsync(string path, List<Model_DunnageLoad> loads, List<string> specKeys)
        {
            await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Dunnage Data");

                // Fixed columns
                int col = 1;
                worksheet.Cell(1, col++).Value = "ID";
                worksheet.Cell(1, col++).Value = "PartID";
                worksheet.Cell(1, col++).Value = "DunnageType";
                worksheet.Cell(1, col++).Value = "Quantity";
                worksheet.Cell(1, col++).Value = "PONumber";
                worksheet.Cell(1, col++).Value = "ReceivedDate";
                worksheet.Cell(1, col++).Value = "UserId";
                worksheet.Cell(1, col++).Value = "Location";
                worksheet.Cell(1, col++).Value = "LabelNumber";

                // Dynamic spec columns
                foreach (var key in specKeys)
                {
                    worksheet.Cell(1, col++).Value = key;
                }

                // Format header
                var headerRange = worksheet.Range(1, 1, 1, col - 1);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Write data rows
                int row = 2;
                foreach (var load in loads)
                {
                    col = 1;
                    worksheet.Cell(row, col++).Value = load.LoadUuid.ToString();
                    worksheet.Cell(row, col++).Value = load.PartId ?? "";
                    worksheet.Cell(row, col++).Value = load.DunnageType ?? "";
                    worksheet.Cell(row, col++).Value = load.Quantity;
                    worksheet.Cell(row, col++).Value = load.PoNumber ?? "";
                    worksheet.Cell(row, col++).Value = load.ReceivedDate.ToString("yyyy-MM-dd HH:mm:ss");
                    worksheet.Cell(row, col++).Value = load.CreatedBy ?? "";
                    worksheet.Cell(row, col++).Value = load.Location ?? "";
                    worksheet.Cell(row, col++).Value = load.LabelNumber ?? "";

                    foreach (var key in specKeys)
                    {
                        if (load.Specs != null && load.Specs.TryGetValue(key, out object? value))
                        {
                            worksheet.Cell(row, col++).Value = value?.ToString() ?? "";
                        }
                        else
                        {
                            worksheet.Cell(row, col++).Value = "";
                        }
                    }
                    row++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(path);
            });
        }

        public async Task<Model_XLSDeleteResult> ClearXLSFilesAsync(string? filenamePattern = null)
        {
            var result = new Model_XLSDeleteResult();
            var errors = new List<string>();

            try
            {
                await _logger.LogInfoAsync($"Starting XLS file clearing with pattern: {filenamePattern ?? "all"}");

                // Clear local files
                try
                {
                    var localFolder = Path.GetDirectoryName(GetLocalXLSPath("dummy.xlsx"));
                    if (Directory.Exists(localFolder))
                    {
                        var filesToClear = Directory.GetFiles(localFolder, filenamePattern ?? "*.xlsx");
                        foreach (var file in filesToClear)
                        {
                            File.Delete(file);
                            await _logger.LogInfoAsync($"Deleted local XLS file: {file}");
                        }
                        result.LocalDeleted = true;
                    }
                    else
                    {
                        result.LocalDeleted = true;
                        await _logger.LogInfoAsync("Local XLS directory does not exist - no files to clear");
                    }
                }
                catch (Exception ex)
                {
                    result.LocalDeleted = false;
                    result.LocalError = $"Local clearing failed: {ex.Message}";
                    errors.Add(result.LocalError);
                    await _logger.LogErrorAsync($"Failed to clear local XLS files: {ex.Message}");
                }

                // Clear network files
                try
                {
                    if (await IsNetworkPathAvailableAsync())
                    {
                        var networkFolder = Path.GetDirectoryName(GetNetworkXLSPath("dummy.xlsx"));
                        if (Directory.Exists(networkFolder))
                        {
                            var filesToClear = Directory.GetFiles(networkFolder, filenamePattern ?? "*.xlsx");
                            foreach (var file in filesToClear)
                            {
                                File.Delete(file);
                                await _logger.LogInfoAsync($"Deleted network XLS file: {file}");
                            }
                            result.NetworkDeleted = true;
                        }
                        else
                        {
                            result.NetworkDeleted = true;
                            await _logger.LogInfoAsync("Network XLS directory does not exist - no files to clear");
                        }
                    }
                    else
                    {
                        result.NetworkDeleted = false;
                        result.NetworkError = "Network path not available";
                        errors.Add("Network path not available");
                        await _logger.LogWarningAsync("Network path not available for XLS clearing");
                    }
                }
                catch (Exception ex)
                {
                    result.NetworkDeleted = false;
                    result.NetworkError = $"Network clearing failed: {ex.Message}";
                    errors.Add(result.NetworkError);
                    await _logger.LogErrorAsync($"Failed to clear network XLS files: {ex.Message}");
                }

                await _logger.LogInfoAsync($"XLS clearing completed. Local: {result.LocalDeleted}, Network: {result.NetworkDeleted}");
            }
            catch (Exception ex)
            {
                var msg = $"Unexpected error during XLS clearing: {ex.Message}";
                result.LocalError = result.LocalError ?? msg;
                await _errorHandler.HandleErrorAsync($"XLS clearing failed: {ex.Message}", Enum_ErrorSeverity.Error, ex, true);
            }

            return result;
        }
    }
}
