using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
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
using System.Threading;
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
            catch (IOException ioEx)
            {
                var networkPath = await GetNetworkXLSPathInternalAsync();
                var errorMessage = $"Unable to write to required network file '{networkPath}'. The file may be locked by another process. No fallback file was created.";

                _logger.LogError($"{errorMessage} Error: {ioEx.Message}", ioEx);
                result.NetworkSuccess = false;
                result.NetworkFilePath = networkPath;
                result.NetworkError = errorMessage;
                throw new InvalidOperationException(errorMessage, ioEx);
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
                var fileDirectory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(fileDirectory) && !Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }

                const int maxRetries = 5;
                int retryCount = 0;
                Exception? lastException = null;

                while (retryCount < maxRetries)
                {
                    try
                    {
                        bool fileExists = File.Exists(filePath);
                        bool useExisting = fileExists && append;
                        _logger.LogInfo($"File exists check for {filePath}: exists={fileExists}, useExisting={useExisting}, retry={retryCount}");

                        using var workbook = useExisting ? new XLWorkbook(filePath) : new XLWorkbook();
                        var worksheet = workbook.Worksheets.FirstOrDefault() ?? workbook.Worksheets.Add("Receiving Data");
                        EnsureHeaders(worksheet);

                        // Find next empty row
                        int nextRow = worksheet.LastRowUsed()?.RowNumber() + 1 ?? 2;
                        _logger.LogInfo($"Writing to rows starting at: {nextRow}");

                        // Write data
                        foreach (var load in loads)
                        {
                            worksheet.Cell(nextRow, 1).Value = load.LoadID.ToString();
                            worksheet.Cell(nextRow, 2).Value = load.LoadNumber;
                            worksheet.Cell(nextRow, 3).Value = load.PartID ?? string.Empty;
                            worksheet.Cell(nextRow, 4).Value = load.PartDescription ?? string.Empty;
                            worksheet.Cell(nextRow, 5).Value = load.PartType ?? string.Empty;
                            worksheet.Cell(nextRow, 6).Value = load.PoNumber ?? string.Empty;
                            worksheet.Cell(nextRow, 7).Value = load.PoLineNumber ?? string.Empty;
                            worksheet.Cell(nextRow, 8).Value = load.PoVendor ?? string.Empty;
                            worksheet.Cell(nextRow, 9).Value = load.PoStatus ?? string.Empty;
                            worksheet.Cell(nextRow, 10).Value = load.PoDueDate?.ToString("yyyy-MM-dd") ?? string.Empty;
                            worksheet.Cell(nextRow, 11).Value = load.QtyOrdered;
                            worksheet.Cell(nextRow, 12).Value = load.WeightQuantity;
                            worksheet.Cell(nextRow, 13).Value = load.UnitOfMeasure ?? "EA";
                            worksheet.Cell(nextRow, 14).Value = load.HeatLotNumber ?? string.Empty;
                            worksheet.Cell(nextRow, 15).Value = load.RemainingQuantity;
                            worksheet.Cell(nextRow, 16).Value = load.PackagesPerLoad;
                            worksheet.Cell(nextRow, 17).Value = load.PackageTypeName ?? string.Empty;
                            worksheet.Cell(nextRow, 18).Value = load.WeightPerPackage;
                            worksheet.Cell(nextRow, 19).Value = load.IsNonPOItem ? "Yes" : "No";
                            worksheet.Cell(nextRow, 20).Value = load.ReceivedDate.ToString("yyyy-MM-dd HH:mm:ss");
                            worksheet.Cell(nextRow, 21).Value = load.UserId ?? string.Empty;
                            worksheet.Cell(nextRow, 22).Value = load.EmployeeNumber;
                            worksheet.Cell(nextRow, 23).Value = load.IsQualityHoldRequired ? "Yes" : "No";
                            worksheet.Cell(nextRow, 24).Value = load.IsQualityHoldAcknowledged ? "Yes" : "No";
                            worksheet.Cell(nextRow, 25).Value = load.QualityHoldRestrictionType ?? string.Empty;
                            nextRow++;
                        }

                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(filePath);
                        EnableWorkbookSharing(filePath);

                        _logger.LogInfo($"Successfully wrote {loads.Count} records to {filePath}");
                        return; // Success - exit retry loop
                    }
                    catch (IOException ioEx) when (retryCount < maxRetries - 1)
                    {
                        // File is locked by another process
                        lastException = ioEx;
                        retryCount++;
                        int delayMs = 100 * (int)Math.Pow(2, retryCount); // Exponential backoff: 200ms, 400ms, 800ms, 1600ms
                        _logger.LogWarning($"File locked, retry {retryCount}/{maxRetries} after {delayMs}ms: {ioEx.Message}");
                        Thread.Sleep(delayMs);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error writing to file {filePath}: {ex.Message}", ex);
                        _logger.LogError($"Stack trace: {ex.StackTrace}");
                        throw;
                    }
                }

                // If we get here, all retries failed
                _logger.LogError($"Failed to write to {filePath} after {maxRetries} retries");
                throw new IOException($"Unable to write to {filePath} - file may be locked by another process", lastException);
            });
        }

        private static void EnsureHeaders(IXLWorksheet worksheet)
        {
            if (!string.IsNullOrWhiteSpace(worksheet.Cell(1, 1).GetString()))
            {
                return;
            }

            worksheet.Cell(1, 1).Value = "LoadID";
            worksheet.Cell(1, 2).Value = "Load Number";
            worksheet.Cell(1, 3).Value = "Part ID";
            worksheet.Cell(1, 4).Value = "Part Description";
            worksheet.Cell(1, 5).Value = "Part Type";
            worksheet.Cell(1, 6).Value = "PO Number";
            worksheet.Cell(1, 7).Value = "PO Line Number";
            worksheet.Cell(1, 8).Value = "PO Vendor";
            worksheet.Cell(1, 9).Value = "PO Status";
            worksheet.Cell(1, 10).Value = "PO Due Date";
            worksheet.Cell(1, 11).Value = "Qty Ordered";
            worksheet.Cell(1, 12).Value = "Weight/Quantity (lbs)";
            worksheet.Cell(1, 13).Value = "Unit of Measure";
            worksheet.Cell(1, 14).Value = "Heat/Lot Number";
            worksheet.Cell(1, 15).Value = "Remaining Quantity";
            worksheet.Cell(1, 16).Value = "Packages Per Load";
            worksheet.Cell(1, 17).Value = "Package Type";
            worksheet.Cell(1, 18).Value = "Weight Per Package";
            worksheet.Cell(1, 19).Value = "Is Non-PO Item";
            worksheet.Cell(1, 20).Value = "Received Date";
            worksheet.Cell(1, 21).Value = "User ID";
            worksheet.Cell(1, 22).Value = "Employee Number";
            worksheet.Cell(1, 23).Value = "Quality Hold Required";
            worksheet.Cell(1, 24).Value = "Quality Hold Acknowledged";
            worksheet.Cell(1, 25).Value = "Quality Hold Restriction Type";

            var headerRange = worksheet.Range("A1:Y1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        private void EnableWorkbookSharing(string filePath)
        {
            try
            {
                using var document = SpreadsheetDocument.Open(filePath, true);
                var workbookPart = document.WorkbookPart;

                if (workbookPart == null)
                {
                    return;
                }

                if (workbookPart.WorkbookUserDataPart == null)
                {
                    var userDataPart = workbookPart.AddNewPart<WorkbookUserDataPart>();
                    userDataPart.Users = new Users();
                }

                if (workbookPart.WorkbookRevisionHeaderPart == null)
                {
                    var revisionHeaderPart = workbookPart.AddNewPart<WorkbookRevisionHeaderPart>();
                    revisionHeaderPart.Headers = new Headers { Guid = Guid.NewGuid().ToString("B") };
                    revisionHeaderPart.AddNewPart<WorkbookRevisionLogPart>();
                }

                workbookPart.Workbook?.Save();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Unable to enable shared workbook metadata: {ex.Message}");
            }
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
                            LoadID = Guid.TryParse(row.Cell(1).GetString(), out var loadId) ? loadId : Guid.NewGuid(),
                            LoadNumber = row.Cell(2).GetValue<int>(),
                            PartID = row.Cell(3).GetString(),
                            PartDescription = row.Cell(4).GetString(),
                            PartType = row.Cell(5).GetString(),
                            PoNumber = row.Cell(6).GetString(),
                            PoLineNumber = row.Cell(7).GetString(),
                            PoVendor = row.Cell(8).GetString(),
                            PoStatus = row.Cell(9).GetString(),
                            PoDueDate = DateTime.TryParse(row.Cell(10).GetString(), out var dueDate) ? dueDate : null,
                            QtyOrdered = row.Cell(11).GetValue<decimal>(),
                            WeightQuantity = row.Cell(12).GetValue<decimal>(),
                            UnitOfMeasure = row.Cell(13).GetString(),
                            HeatLotNumber = row.Cell(14).GetString(),
                            RemainingQuantity = row.Cell(15).GetValue<int>(),
                            PackagesPerLoad = row.Cell(16).GetValue<int>(),
                            PackageTypeName = row.Cell(17).GetString(),
                            WeightPerPackage = row.Cell(18).GetValue<decimal>(),
                            IsNonPOItem = row.Cell(19).GetString().Equals("Yes", StringComparison.OrdinalIgnoreCase),
                            ReceivedDate = DateTime.TryParse(row.Cell(20).GetString(), out var date) ? date : DateTime.Now,
                            UserId = row.Cell(21).GetString(),
                            EmployeeNumber = row.Cell(22).GetValue<int>(),
                            IsQualityHoldRequired = row.Cell(23).GetString().Equals("Yes", StringComparison.OrdinalIgnoreCase),
                            IsQualityHoldAcknowledged = row.Cell(24).GetString().Equals("Yes", StringComparison.OrdinalIgnoreCase),
                            QualityHoldRestrictionType = row.Cell(25).GetString()
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

                    // Add ALL headers (same as WriteToFileAsync)
                    worksheet.Cell(1, 1).Value = "LoadID";
                    worksheet.Cell(1, 2).Value = "Load Number";
                    worksheet.Cell(1, 3).Value = "Part ID";
                    worksheet.Cell(1, 4).Value = "Part Description";
                    worksheet.Cell(1, 5).Value = "Part Type";
                    worksheet.Cell(1, 6).Value = "PO Number";
                    worksheet.Cell(1, 7).Value = "PO Line Number";
                    worksheet.Cell(1, 8).Value = "PO Vendor";
                    worksheet.Cell(1, 9).Value = "PO Status";
                    worksheet.Cell(1, 10).Value = "PO Due Date";
                    worksheet.Cell(1, 11).Value = "Qty Ordered";
                    worksheet.Cell(1, 12).Value = "Weight/Quantity (lbs)";
                    worksheet.Cell(1, 13).Value = "Unit of Measure";
                    worksheet.Cell(1, 14).Value = "Heat/Lot Number";
                    worksheet.Cell(1, 15).Value = "Remaining Quantity";
                    worksheet.Cell(1, 16).Value = "Packages Per Load";
                    worksheet.Cell(1, 17).Value = "Package Type";
                    worksheet.Cell(1, 18).Value = "Weight Per Package";
                    worksheet.Cell(1, 19).Value = "Is Non-PO Item";
                    worksheet.Cell(1, 20).Value = "Received Date";
                    worksheet.Cell(1, 21).Value = "User ID";
                    worksheet.Cell(1, 22).Value = "Employee Number";
                    worksheet.Cell(1, 23).Value = "Quality Hold Required";
                    worksheet.Cell(1, 24).Value = "Quality Hold Acknowledged";
                    worksheet.Cell(1, 25).Value = "Quality Hold Restriction Type";

                    var headerRange = worksheet.Range("A1:Y1");
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
