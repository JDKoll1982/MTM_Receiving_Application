using CsvHelper;
using CsvHelper.Configuration;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Receiving.Services
{
    /// <summary>
    /// Service for writing receiving data to CSV files.
    /// Respects the user-configured CSV save location from settings.
    /// Falls back to default network path if not configured.
    /// </summary>
    public class Service_CSVWriter : IService_CSVWriter
    {
        private const string SettingsCategory = "Receiving";
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_LoggingUtility _logger;
        private readonly IService_SettingsCoreFacade _settingsCore;

        public Service_CSVWriter(
            IService_UserSessionManager sessionManager,
            IService_LoggingUtility logger,
            IService_SettingsCoreFacade settingsCore)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        }

        private async Task<string> GetNetworkCSVPathInternalAsync()
        {
            _logger.LogInfo("Resolving network CSV path...");
            try
            {
                // First, try to get user-configured path from settings
                var currentUserId = _sessionManager.CurrentSession?.User?.EmployeeNumber;
                if (currentUserId.HasValue)
                {
                    var settingResult = await _settingsCore.GetSettingAsync(
                        SettingsCategory,
                        ReceivingSettingsKeys.Defaults.CsvSaveLocation,
                        currentUserId.Value);

                    if (settingResult.IsSuccess && !string.IsNullOrWhiteSpace(settingResult.Data?.Value))
                    {
                        var configuredPath = settingResult.Data.Value;
                        _logger.LogInfo($"Using user-configured CSV path: {configuredPath}");
                        
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

                        return Path.Combine(configuredPath, "ReceivingData.csv");
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

                var networkBase = @"\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files";
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
                return Path.Combine(userDir, "ReceivingData.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error resolving default network path: {ex.Message}");
                return @"\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\UnknownUser\ReceivingData.csv";
            }
        }

        public async Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_ReceivingLoad> loads)
        {
            _logger.LogInfo($"Starting WriteToCSVAsync for {loads?.Count ?? 0} loads.");
            if (loads == null || loads.Count == 0)
            {
                throw new ArgumentException("Loads list cannot be null or empty", nameof(loads));
            }

            var result = new Model_CSVWriteResult
            {
                RecordsWritten = loads.Count,
                LocalSuccess = true
            };

            try
            {
                _logger.LogInfo("Attempting network CSV write...");
                var networkPath = await GetNetworkCSVPathInternalAsync();
                _logger.LogInfo($"Network path resolved: {networkPath}");
                await WriteToFileAsync(networkPath, loads);
                result.NetworkSuccess = true;
                result.NetworkFilePath = networkPath;
                _logger.LogInfo("Network CSV write successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Network CSV write failed: {ex.Message}");
                result.NetworkSuccess = false;
                result.NetworkError = ex.Message;
                throw new InvalidOperationException("Failed to write network CSV file", ex);
            }

            return result;
        }

        public async Task WriteToFileAsync(string filePath, List<Model_ReceivingLoad> loads, bool append = true)
        {
            _logger.LogInfo($"WriteToFileAsync called for: {filePath}, Append: {append}");
            await Task.Run(async () =>
            {
                try
                {
                    bool isNewFile = !File.Exists(filePath) || !append;
                    _logger.LogInfo($"File exists check for {filePath}: {!isNewFile}");

                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        // We handle header manually
                    };

                    var fileMode = append ? FileMode.Append : FileMode.Create;
                    await using var stream = new FileStream(filePath, fileMode, FileAccess.Write, FileShare.Read);
                    await using var writer = new StreamWriter(stream);
                    await using var csv = new CsvWriter(writer, config);

                    if (isNewFile)
                    {
                        csv.WriteHeader<Model_ReceivingLoad>();
                        csv.NextRecord();
                    }

                    foreach (var load in loads)
                    {
                        csv.WriteRecord(load);
                        csv.NextRecord();
                    }

                    await writer.FlushAsync();
                    _logger.LogInfo($"Successfully wrote to {filePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error writing to file {filePath}: {ex.Message}");
                    throw;
                }
            });
        }

        public async Task<List<Model_ReceivingLoad>> ReadFromCSVAsync(string filePath)
        {
            _logger.LogInfo($"ReadFromCSVAsync called for: {filePath}");
            return await Task.Run(() =>
            {
                try
                {
                    if (!File.Exists(filePath))
                    {
                        throw new FileNotFoundException($"CSV file not found: {filePath}");
                    }

                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HeaderValidated = null,
                        MissingFieldFound = null
                    };

                    using var reader = new StreamReader(filePath);
                    using var csv = new CsvReader(reader, config);

                    var records = csv.GetRecords<Model_ReceivingLoad>();
                    var loads = new List<Model_ReceivingLoad>(records);

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

        public async Task<Model_CSVDeleteResult> ClearCSVFilesAsync()
        {
            var result = new Model_CSVDeleteResult();

            try
            {
                var networkPath = await GetNetworkCSVPathInternalAsync();
                if (File.Exists(networkPath))
                {
                    using var writer = new StreamWriter(networkPath);
                    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                    csv.WriteHeader<Model_ReceivingLoad>();
                    csv.NextRecord();
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

        public async Task<Model_CSVExistenceResult> CheckCSVFilesExistAsync()
        {
            var result = new Model_CSVExistenceResult();

            try
            {
                var networkPath = await GetNetworkCSVPathInternalAsync();
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

        public async Task<string> GetNetworkCSVPathAsync() => await GetNetworkCSVPathInternalAsync();
    }
}

