using System.Text.Json;
using FluentAssertions;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Core.Services;

public sealed class SettingsManifestDunnageShortcutTests
{
    [Fact]
    public void SettingsManifest_ShouldContainDunnageShortcutDefinitions()
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

        var keys = document
            .RootElement.GetProperty("settings")
            .EnumerateArray()
            .Where(setting =>
                setting.GetProperty("category").GetString() == "Dunnage"
                && setting.GetProperty("key").GetString()!.StartsWith("Dunnage.Shortcuts.")
            )
            .Select(setting => setting.GetProperty("key").GetString())
            .ToArray();

        keys.Should().Contain("Dunnage.Shortcuts.ModeSelection");
        keys.Should().Contain("Dunnage.Shortcuts.ClearLabelData");
        keys.Should().Contain("Dunnage.Shortcuts.NextStep");
        keys.Should().Contain("Dunnage.Shortcuts.BackStep");
        keys.Should().Contain("Dunnage.Shortcuts.Help");
        keys.Should().Contain("Dunnage.Shortcuts.IsToggleSimpleNavigationEnabled");
    }
}
