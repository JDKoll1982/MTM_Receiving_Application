using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Core.Services;

/// <summary>
/// Provides access to application-level settings stored in appsettings JSON files.
/// </summary>
public sealed class Service_AppSettings : IService_AppSettings
{
    private const string AppSettingsSectionName = "AppSettings";
    private const string UseInforVisualMockDataKey = "UseInforVisualMockData";
    private const string DefaultMockPONumberKey = "DefaultMockPONumber";

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IService_LoggingUtility? _logger;

    public Service_AppSettings(
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        IService_LoggingUtility? logger = null
    )
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _hostEnvironment =
            hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        _logger = logger;
    }

    public bool GetUseInforVisualMockData()
    {
        return GetBooleanValue(UseInforVisualMockDataKey, false);
    }

    public string? GetDefaultMockPONumber()
    {
        return GetStringValue(DefaultMockPONumberKey);
    }

    public async Task<Model_Dao_Result> SetUseInforVisualMockDataAsync(bool value)
    {
        try
        {
            var writablePaths = ResolveWritableConfigPaths();
            if (writablePaths.Count == 0)
            {
                return Model_Dao_Result_Factory.Failure(
                    "No writable appsettings files were found."
                );
            }

            foreach (var path in writablePaths)
            {
                JsonObject rootObject;
                if (File.Exists(path))
                {
                    var parsed = JsonNode.Parse(await File.ReadAllTextAsync(path)) as JsonObject;
                    rootObject = parsed ?? new JsonObject();
                }
                else
                {
                    rootObject = new JsonObject();
                }

                var appSettingsObject = rootObject[AppSettingsSectionName] as JsonObject;
                if (appSettingsObject == null)
                {
                    appSettingsObject = new JsonObject();
                    rootObject[AppSettingsSectionName] = appSettingsObject;
                }

                appSettingsObject[UseInforVisualMockDataKey] = value;

                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(path, rootObject.ToJsonString(JsonOptions));
            }

            _logger?.LogInfo(
                $"Set AppSettings:{UseInforVisualMockDataKey} to {value} in {writablePaths.Count} appsettings file(s)."
            );
            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                $"Failed to update AppSettings:{UseInforVisualMockDataKey}: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure(
                $"Failed to update AppSettings:{UseInforVisualMockDataKey}: {ex.Message}",
                ex
            );
        }
    }

    private bool GetBooleanValue(string key, bool fallback)
    {
        var value = GetJsonValue(key);
        if (value is JsonValue jsonValue && jsonValue.TryGetValue<bool>(out var parsed))
        {
            return parsed;
        }

        return _configuration.GetValue($"{AppSettingsSectionName}:{key}", fallback);
    }

    private string? GetStringValue(string key)
    {
        var value = GetJsonValue(key);
        if (value is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var parsed))
        {
            return parsed;
        }

        return _configuration.GetValue<string>($"{AppSettingsSectionName}:{key}");
    }

    private JsonNode? GetJsonValue(string key)
    {
        foreach (var path in ResolveReadableConfigPaths())
        {
            if (!File.Exists(path))
            {
                continue;
            }

            var rootObject = JsonNode.Parse(File.ReadAllText(path)) as JsonObject;
            var appSettingsObject = rootObject?[AppSettingsSectionName] as JsonObject;
            if (
                appSettingsObject != null
                && appSettingsObject.TryGetPropertyValue(key, out var value)
            )
            {
                return value;
            }
        }

        return null;
    }

    private IReadOnlyList<string> ResolveReadableConfigPaths()
    {
        var fileNames = GetConfigFileNames();
        var results = new List<string>();

        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        while (currentDirectory != null)
        {
            foreach (var fileName in fileNames)
            {
                var candidate = Path.Combine(currentDirectory.FullName, fileName);
                if (
                    File.Exists(candidate)
                    && !results.Contains(candidate, StringComparer.OrdinalIgnoreCase)
                )
                {
                    results.Add(candidate);
                }
            }

            currentDirectory = currentDirectory.Parent;
        }

        return results;
    }

    private IReadOnlyList<string> ResolveWritableConfigPaths()
    {
        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory != null)
        {
            foreach (var fileName in GetConfigFileNames())
            {
                var candidate = Path.Combine(currentDirectory.FullName, fileName);
                if (File.Exists(candidate))
                {
                    results.Add(candidate);
                }
            }

            currentDirectory = currentDirectory.Parent;
        }

        return results.ToList();
    }

    private IReadOnlyList<string> GetConfigFileNames()
    {
        var fileNames = new List<string>();
        if (!string.IsNullOrWhiteSpace(_hostEnvironment.EnvironmentName))
        {
            fileNames.Add($"appsettings.{_hostEnvironment.EnvironmentName}.json");
        }

        fileNames.Add("appsettings.json");
        return fileNames;
    }
}
