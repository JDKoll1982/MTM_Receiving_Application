using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.ReceivingModule.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.ReceivingModule.Services
{
    /// <summary>
    /// Service for persisting and restoring session state to/from JSON file.
    /// Session file location: %APPDATA%\MTM_Receiving_Application\session.json
    /// </summary>
    public class Service_SessionManager : IService_SessionManager
    {
        private readonly string _sessionPath;
        private readonly IService_LoggingUtility _logger;

        public Service_SessionManager(IService_LoggingUtility logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "MTM_Receiving_Application");

            // Ensure directory exists
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _sessionPath = Path.Combine(appFolder, "session.json");
        }

        public async Task SaveSessionAsync(Model_ReceivingSession session)
        {
            _logger.LogInfo("SaveSessionAsync started.");
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                _logger.LogInfo("Serializing session...");
                var json = JsonSerializer.Serialize(session, options);
                _logger.LogInfo($"Writing session to file: {_sessionPath}");
                await File.WriteAllTextAsync(_sessionPath, json);
                _logger.LogInfo("Session saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save session to file: {ex.Message}", ex);
                throw new InvalidOperationException("Failed to save session to file", ex);
            }
        }

        public async Task<Model_ReceivingSession?> LoadSessionAsync()
        {
            if (!File.Exists(_sessionPath))
            {
                return null;
            }

            try
            {
                var json = await File.ReadAllTextAsync(_sessionPath);

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                return JsonSerializer.Deserialize<Model_ReceivingSession>(json, options);
            }
            catch (JsonException)
            {
                // Corrupted file - delete and return null
                try
                {
                    File.Delete(_sessionPath);
                }
                catch
                {
                    // Ignore delete errors
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load session from file", ex);
            }
        }

        public async Task<bool> ClearSessionAsync()
        {
            if (!File.Exists(_sessionPath))
            {
                return false;
            }

            try
            {
                await Task.Run(() => File.Delete(_sessionPath));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SessionExists()
        {
            return File.Exists(_sessionPath);
        }

        public string GetSessionFilePath()
        {
            return _sessionPath;
        }
    }
}
