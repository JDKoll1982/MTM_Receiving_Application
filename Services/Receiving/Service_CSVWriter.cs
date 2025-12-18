using CsvHelper;
using CsvHelper.Configuration;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Services.Receiving
{
    /// <summary>
    /// Service for writing receiving data to CSV files.
    /// Handles both local (%APPDATA%) and network paths with graceful fallback.
    /// </summary>
    public class Service_CSVWriter : IService_CSVWriter
    {
        private readonly string _localCSVPath;
        private readonly IService_UserSessionManager _sessionManager;

        public Service_CSVWriter(IService_UserSessionManager sessionManager)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // Ensure directory exists
            var appDir = Path.Combine(appDataPath, "MTM_Receiving_Application");
            if (!Directory.Exists(appDir))
            {
                Directory.CreateDirectory(appDir);
            }
            _localCSVPath = Path.Combine(appDir, "ReceivingData.csv");
        }

        private string GetNetworkCSVPathInternal()
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
                
                // Ensure directory exists
                if (!Directory.Exists(userDir))
                {
                    try { Directory.CreateDirectory(userDir); } catch { }
                }
                return Path.Combine(userDir, "ReceivingData.csv");
            }
            catch
            {
                return @"\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\UnknownUser\ReceivingData.csv";
            }
        }

        public async Task<CSVWriteResult> WriteToCSVAsync(List<Model_ReceivingLoad> loads)
        {
            if (loads == null || loads.Count == 0)
                throw new ArgumentException("Loads list cannot be null or empty", nameof(loads));

            var result = new CSVWriteResult { RecordsWritten = loads.Count };

            // Write to local CSV (required - failure is critical)
            try
            {
                await WriteToFileAsync(_localCSVPath, loads);
                result.LocalSuccess = true;
            }
            catch (Exception ex)
            {
                result.LocalSuccess = false;
                result.LocalError = ex.Message;
                throw new InvalidOperationException("Failed to write local CSV file", ex);
            }

            // Attempt network CSV (optional - failure is not critical)
            try
            {
                var networkPath = GetNetworkCSVPathInternal();
                await WriteToFileAsync(networkPath, loads);
                result.NetworkSuccess = true;
            }
            catch (Exception ex)
            {
                result.NetworkSuccess = false;
                result.NetworkError = ex.Message;
                // Don't throw - graceful degradation
            }

            return result;
        }

        public async Task WriteToFileAsync(string filePath, List<Model_ReceivingLoad> loads)
        {
            bool isNewFile = !File.Exists(filePath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // We handle header manually
            };

            using var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config);

            if (isNewFile)
            {
                csv.WriteHeader<Model_ReceivingLoad>();
                csv.NextRecord();
            }

            await Task.Run(() =>
            {
                foreach (var load in loads)
                {
                    csv.WriteRecord(load);
                    csv.NextRecord();
                }
            });

            await writer.FlushAsync();
        }

        public async Task<CSVDeleteResult> DeleteCSVFilesAsync()
        {
            var result = new CSVDeleteResult();

            // Delete local CSV
            if (File.Exists(_localCSVPath))
            {
                try
                {
                    await Task.Run(() => File.Delete(_localCSVPath));
                    result.LocalDeleted = true;
                }
                catch (Exception ex)
                {
                    result.LocalDeleted = false;
                    result.LocalError = ex.Message;
                }
            }

            // Delete network CSV
            var networkPath = GetNetworkCSVPathInternal();
            if (File.Exists(networkPath))
            {
                try
                {
                    await Task.Run(() => File.Delete(networkPath));
                    result.NetworkDeleted = true;
                }
                catch (Exception ex)
                {
                    result.NetworkDeleted = false;
                    result.NetworkError = ex.Message;
                }
            }

            return result;
        }

        public async Task<CSVExistenceResult> CheckCSVFilesExistAsync()
        {
            var result = new CSVExistenceResult();

            await Task.Run(() =>
            {
                result.LocalExists = File.Exists(_localCSVPath);

                try
                {
                    var networkPath = GetNetworkCSVPathInternal();
                    result.NetworkExists = File.Exists(networkPath);
                    result.NetworkAccessible = true;
                }
                catch
                {
                    result.NetworkExists = false;
                    result.NetworkAccessible = false;
                }
            });

            return result;
        }

        public string GetLocalCSVPath() => _localCSVPath;

        public string GetNetworkCSVPath() => GetNetworkCSVPathInternal();
    }
}
