using System.IO;
using System.Text.Json;
using Xunit;

namespace MTM_Receiving_Application.Tests.CopilotForms;

public class CopilotFormsConfigTests
{
    [Fact]
    public void FeaturesConfig_ShouldNotReferenceBulkInventoryModule()
    {
        var configPath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "..",
                "docs",
                "CopilotForms",
                "data",
                "copilot-forms.config.json"
            )
        );

        Assert.True(File.Exists(configPath), "the CopilotForms feature catalog must exist");

        using var stream = File.OpenRead(configPath);
        using var document = JsonDocument.Parse(stream);

        var modules = document.RootElement.GetProperty("features");
        var hasBulkInventoryFeature = modules
            .EnumerateArray()
            .Any(feature =>
                feature.TryGetProperty("module", out var moduleName)
                && moduleName.GetString() == "Module_Bulk_Inventory"
            );

        Assert.False(hasBulkInventoryFeature);
    }
}
