using System.Text.Json;
using FluentAssertions;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Core.Services;

public sealed class SettingsManifestScannerAccessTests
{
    [Fact]
    public void SettingsManifest_ShouldContainScannerAccessDefinition()
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

        var definition = document
            .RootElement.GetProperty("settings")
            .EnumerateArray()
            .Single(setting =>
                setting.GetProperty("category").GetString() == "Scanner"
                && setting.GetProperty("key").GetString()
                    == "Scanner.Access.AllowedEmployeeNumbers"
            );

        definition.GetProperty("scope").GetString().Should().Be("System");
        definition.GetProperty("permissionLevel").GetString().Should().Be("Admin");
        definition.GetProperty("dataType").GetString().Should().Be("Json");
        definition.GetProperty("defaultValue").GetString().Should().Be("[]");
    }
}
