using ClosedXML.Excel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Receiving.Services
{
    /// <summary>
    /// Service for writing receiving data to XLS files.
    /// Respects the user-configured XLS save location from settings.
    /// Falls back to default network path if not configured.
    /// </summary>
    public class Service_XLSWriter : IService_XLSWriter
    {
        private const string SettingsCategory = "Receiving";
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_LoggingUtility _logger;
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

        private async Task<string> GetNetworkXLSPathInternalAsync()
        {
            _logger.LogInfo("Resolving network XLS path...");
            try
            {
                // First, try to get user-configured path from settings
                var currentUserId = _sessionManager.CurrentSession?.User?.EmployeeNumber;
                if (currentUserId.HasValue)
                {
                    var settingResult = await _settingsCore.GetSettingAsync(
                        SettingsCategory,
                        ReceivingSettingsKeys.Defaults.XlsSaveLocation,
                        currentUserId.Value);

                    if (settingResult.IsSuccess && !string.IsNullOrWhiteSpace(settingResult.Data?.Value))
                    {
                        var configuredPath = settingResult.Data.Value;
                        _logger.LogInfo($"Using user-configured XLS path: {configuredPath}");
                        
                        // Ensure directory exists
                        try
                        {
                            if (!Directory.Exists(configuredPath))
                            {
                                _logger.LogInfo($"Creating directory: {configuredPath}");
                                Directory.CreateDirectory(configuredPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Failed to create configured directory: {ex.Message}");
                        }

                        return Path.Combine(configuredPath, "ReceivingData.xlsx");
                    }
                }

                // Fall back to default network path if not configured
                return GetDefaultNetworkPath();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error resolving network path: {ex.Message}");
                return GetDefaultNetworkPath();
            }
        }

        private string GetDefaultNetworkPath()
        {
            try
            {
                string userName;
                if (_sessionManager.CurrentSession?.User != null)
                {
                    userName = _sessionManager.CurrentSession.User.WindowsUsername;
                }
                else
                {
                    userName = Environment.UserName;
                }

                var networkBase = @"\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User XLS Files";
                var userDir = Path.Combine(networkBase, userName);

                _logger.LogInfo($"Using default network path: {userDir}");
                // Ensure directory exists
                if (!Directory.Exists(userDir))
                {
                    try
                    {
                        _logger.LogInfo($"Creating network directory: {userDir}");
                        Directory.CreateDirectory(userDir);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to create network directory: {ex.Message}");
                    }
                }
                return Path.Combine(userDir, "ReceivingData.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error resolving default network path: {ex.Message}");
                return @"\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User XLS Files\UnknownUser\ReceivingData.xlsx";
            }
        }

        public async Task<Model_XLSWriteResult> WriteToXLSAsync(List<Model_ReceivingLoad> loads)
        {
            _logger.LogInfo($"Starting WriteToXLSAsync for {loads?.Count ?? 0} loads.");
            if (loads == null || loads.Count == 0)
            {
                throw new ArgumentException("Loads list cannot be null or empty", nameof(loads));
            }

            var result = new Model_XLSWriteResult
            {
                RecordsWritten = loads.Count,
                LocalSuccess = true
            };

            try
            {
                _logger.LogInfo("Attempting network XLS write...");
                var networkPath = await GetNetworkXLSPathInternalAsync();
                _logger.LogInfo($"Network path resolved: {networkPath}");
                await WriteToFileAsync(networkPath, loads);
                result.NetworkSuccess = true;
                result.NetworkFilePath = networkPath;
                _logger.LogInfo("Network XLS write successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Network XLS write failed: {ex.Message}");
                result.NetworkSuccess = false;
                result.NetworkError = ex.Message;
                throw new InvalidOperationException("Failed to write network XLS file", ex);
            }

            return result;
        }

        public async Task WriteToFileAsync(string filePath, List<Model_ReceivingLoad> loads, bool append = true)
        {
            _logger.LogInfo($"WriteToFileAsync called for: {filePath}, Append: {append}, Loads count: {loads?.Count ?? 0}");
            
            if (loads == null || loads.Count == 0)
            {
                _logger.LogWarning("No loads to write - loads list is null or empty");
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    bool isNewFile = !File.Exists(filePath) || !append;
                    _logger.LogInfo($"File exists check for {filePath}: exists={File.Exists(filePath)}, isNewFile={isNewFile}");

                    IXLWorkbook workbook;
                    IXLWorksheet worksheet;

                    if (isNewFile || !File.Exists(filePath))
                    {
                        // Create new workbook
                        workbook = new XLWorkbook();
                        worksheet = workbook.Worksheets.Add("Receiving Data");
                        
                        // Add headers
                        worksheet.Cell(1, 1).Value = "Load Number";
                        worksheet.Cell(1, 2).Value = "Part ID";
                        worksheet.Cell(1, 3).Value = "PO Number";
                        worksheet.Cell(1, 4).Value = "Quantity";
                        worksheet.Cell(1, 5).Value = "Weight (lbs)";
                        worksheet.Cell(1, 6).Value = "Heat/Lot Number";
                        worksheet.Cell(1, 7).Value = "Received Date";
                        worksheet.Cell(1, 8).Value = "Employee";
                        
                        // Format header row
                        var headerRange = worksheet.Range("A1:H1");
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    }
                    else
                    {
                        // Open existing workbook
                        workbook = new XLWorkbook(filePath);
                        worksheet = workbook.Worksheet(1);
                    }

                    // Find next empty row
                    int nextRow = worksheet.LastRowUsed()?.RowNumber() + 1 ?? 2;

                    // Write data
                    foreach (var load in loads)
                    {
                        worksheet.Cell(nextRow, 1).Value = load.LoadNumber;
                        worksheet.Cell(nextRow, 2).Value = load.PartID ?? string.Empty;
                        worksheet.Cell(nextRow, 3).Value = load.PoNumber ?? string.Empty;
                        worksheet.Cell(nextRow, 4).Value = load.WeightQuantity;
                        worksheet.Cell(nextRow, 5).Value = load.WeightQuantity;
                        worksheet.Cell(nextRow, 6).Value = load.HeatLotNumber ?? string.Empty;
                        worksheet.Cell(nextRow, 7).Value = load.ReceivedDate.ToString("yyyy-MM-dd HH:mm:ss");
                        worksheet.Cell(nextRow, 8).Value = load.EmployeeNumber;
                        nextRow++;
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Save workbook
                    workbook.SaveAs(filePath);
                    workbook.Dispose();

                    _logger.LogInfo($"Successfully wrote {loads.Count} records to {filePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error writing to file {filePath}: {ex.Message}", ex);
                    _logger.LogError($"Stack trace: {ex.StackTrace}");
                    throw;
                }
            });
        }

        public async Task<List<Model_ReceivingLoad>> ReadFromXLSAsync(string filePath)
        {
            _logger.LogInfo($"ReadFromXLSAsync called for: {filePath}");
            return await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(filePath))
                    {
                        throw new FileNotFoundException($"XLS file not found: {filePath}");
                    }

                    var loads = new List<Model_ReceivingLoad>();
                    
                    using var workbook = new XLWorkbook(filePath);
                    var worksheet = workbook.Worksheet(1);
                    
                    // Skip header row, start from row 2
                    var rows = worksheet.RowsUsed().Skip(1);
                    
                    foreach (var row in rows)
                    {
                        var load = new Model_ReceivingLoad
                        {
                            LoadNumber = row.Cell(1).GetValue<int>(),
                            PartID = row.Cell(2).GetString(),
                            PoNumber = row.Cell(3).GetString(),
                            WeightQuantity = row.Cell(4).GetValue<decimal>(),
                            HeatLotNumber = row.Cell(6).GetString(),
                            ReceivedDate = DateTime.TryParse(row.Cell(7).GetString(), out var date) ? date : DateTime.Now
                        };
                        loads.Add(load);
                    }

                    _logger.LogInfo($"Successfully read {loads.Count} records from {filePath}");
                    return loads;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error reading from file {filePath}: {ex.Message}");
                    throw;
                }
            });
        }

        public async Task<Model_XLSDeleteResult> ClearXLSFilesAsync()
        {
            var result = new Model_XLSDeleteResult();

            try
            {
                var networkPath = await GetNetworkXLSPathInternalAsync();
                if (File.Exists(networkPath))
                {
                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("Receiving Data");
                    
                    // Add headers only
                    worksheet.Cell(1, 1).Value = "Load Number";
                    worksheet.Cell(1, 2).Value = "Part ID";
                    worksheet.Cell(1, 3).Value = "PO Number";
                    worksheet.Cell(1, 4).Value = "Quantity";
                    worksheet.Cell(1, 5).Value = "Weight (lbs)";
                    worksheet.Cell(1, 6).Value = "Heat/Lot Number";
                    worksheet.Cell(1, 7).Value = "Received Date";
                    worksheet.Cell(1, 8).Value = "Employee";
                    
                    var headerRange = worksheet.Range("A1:H1");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    
                    workbook.SaveAs(networkPath);
                    result.NetworkDeleted = true;
                }
            }
            catch (Exception ex)
            {
                result.NetworkDeleted = false;
                result.NetworkError = ex.Message;
            }

            return result;
        }

        public async Task<Model_XLSExistenceResult> CheckXLSFilesExistAsync()
        {
            var result = new Model_XLSExistenceResult();

            try
            {
                var networkPath = await GetNetworkXLSPathInternalAsync();
                result.NetworkExists = File.Exists(networkPath);
                result.NetworkAccessible = true;
            }
            catch
            {
                result.NetworkExists = false;
                result.NetworkAccessible = false;
            }

            return result;
        }

        public async Task<string> GetNetworkXLSPathAsync() => await GetNetworkXLSPathInternalAsync();
    }
}
