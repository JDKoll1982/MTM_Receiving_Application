using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Settings.Core.Helpers;

/// <summary>
/// Reads and writes the machine-local appsettings.local.json override file that
/// sits next to the running application. The Database Config feature uses it to
/// persist the MySQL database target and the SyncTool launch paths; keys it
/// defines are applied at startup (see App.ConfigureAppConfiguration).
/// </summary>
public static class Helper_LocalAppConfigFile
{
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    /// <summary>Gets the absolute path of the local override file.</summary>
    public static string OverrideFilePath =>
        Path.Combine(AppContext.BaseDirectory, "appsettings.local.json");

    /// <summary>
    /// Reads the override file as a JSON object, or returns an empty object when
    /// the file is missing or cannot be parsed (callers fall back to defaults).
    /// </summary>
    public static async Task<JsonObject> ReadRootAsync()
    {
        try
        {
            if (!File.Exists(OverrideFilePath))
            {
                return new JsonObject();
            }

            var text = await File.ReadAllTextAsync(OverrideFilePath);
            return JsonNode.Parse(text) as JsonObject ?? new JsonObject();
        }
        catch
        {
            // Never fail callers because of a malformed local file.
            return new JsonObject();
        }
    }

    /// <summary>Persists the supplied JSON object to the override file.</summary>
    public static async Task WriteRootAsync(JsonObject root)
    {
        var directory = Path.GetDirectoryName(OverrideFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(OverrideFilePath, root.ToJsonString(WriteOptions));
    }
}
