using System.Text.Json;
using FluentAssertions;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Core.Services;

/// <summary>
/// Guards the manifest's defaults against the type each setting declares.
/// </summary>
/// <remarks>
/// A default that does not parse is not a cosmetic problem. The settings handler validates every value it
/// reads, so a bad default is rejected, reset to itself, and rejected again on the next launch — a warning
/// toast and two writes every single start, forever. That is exactly what an empty default on a
/// <c>Json</c> setting did.
/// </remarks>
public sealed class SettingsManifestDefaultsTests
{
    private static JsonElement LoadSettings()
    {
        var manifestPath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "..",
                "Module_Settings.Core",
                "Defaults",
                "settings.manifest.json"
            )
        );

        File.Exists(manifestPath).Should().BeTrue("the shared settings manifest must exist");

        using var stream = File.OpenRead(manifestPath);
        using var document = JsonDocument.Parse(stream);

        return document.RootElement.GetProperty("settings").Clone();
    }

    private static bool ParsesAs(string dataType, string value) =>
        dataType switch
        {
            "Int" => int.TryParse(value, out _),
            "Decimal" => decimal.TryParse(value, out _),
            "Bool" => bool.TryParse(value, out _),
            "DateTime" => DateTime.TryParse(value, out _),
            "Json" => IsJson(value),
            _ => true,
        };

    private static bool IsJson(string value)
    {
        // An empty string is not JSON, and neither is a bare word; both would be rejected on read.
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            using var _ = JsonDocument.Parse(value);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    [Fact]
    public void Manifest_ShouldDeclareADefaultThatParses_ForEverySetting()
    {
        var offenders = new List<string>();

        foreach (var setting in LoadSettings().EnumerateArray())
        {
            var dataType = setting.GetProperty("dataType").GetString() ?? string.Empty;
            var value = setting.GetProperty("defaultValue").GetString() ?? string.Empty;

            if (!ParsesAs(dataType, value))
            {
                offenders.Add(
                    $"{setting.GetProperty("category").GetString()}:"
                        + $"{setting.GetProperty("key").GetString()} ({dataType}) = '{value}'"
                );
            }
        }

        offenders
            .Should()
            .BeEmpty("a default the handler rejects resets to itself and warns on every launch");
    }

    [Fact]
    public void Manifest_ShouldGiveEveryJsonSettingAParseableDefault()
    {
        var jsonSettings = LoadSettings()
            .EnumerateArray()
            .Where(setting => setting.GetProperty("dataType").GetString() == "Json")
            .ToArray();

        jsonSettings.Should().NotBeEmpty("the app stores structured preferences as JSON");

        foreach (var setting in jsonSettings)
        {
            var value = setting.GetProperty("defaultValue").GetString() ?? string.Empty;

            value
                .Should()
                .NotBeNullOrWhiteSpace(
                    $"{setting.GetProperty("key").GetString()} is read as JSON, so it needs a JSON default"
                );

            IsJson(value)
                .Should()
                .BeTrue($"{setting.GetProperty("key").GetString()} must hold a JSON document");
        }
    }
}
