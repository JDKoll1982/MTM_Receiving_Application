using System.Text.Json;
using FluentAssertions;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Core.Services;

public sealed class SettingsManifestReceivingShortcutTests
{
    [Fact]
    public void SettingsManifest_ShouldContainReceivingShortcutDefinitions()
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
                setting.GetProperty("category").GetString() == "Receiving"
                && setting.GetProperty("key").GetString()!.StartsWith("Receiving.Shortcuts.")
            )
            .Select(setting => setting.GetProperty("key").GetString())
            .ToArray();

        keys.Should().Contain("Receiving.Shortcuts.ModeSelection");
        keys.Should().Contain("Receiving.Shortcuts.ClearLabelData");
        keys.Should().Contain("Receiving.Shortcuts.NextStep");
        keys.Should().Contain("Receiving.Shortcuts.BackStep");
        keys.Should().Contain("Receiving.Shortcuts.Help");
        keys.Should().Contain("Receiving.Shortcuts.IsToggleSimpleNavigationEnabled");
    }
}
