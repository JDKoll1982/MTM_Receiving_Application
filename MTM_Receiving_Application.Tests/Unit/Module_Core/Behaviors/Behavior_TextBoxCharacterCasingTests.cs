using System.Collections.Generic;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Behaviors;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Behaviors;

public sealed class Behavior_TextBoxCharacterCasingTests
{
    [Fact]
    public void NormalizeText_ShouldPreserveConfiguredPresetFillerCasing_WhenValueMatchesException()
    {
        var exceptions = new List<string> { "Old Flatstock", "Refer to Vendor Tag" };

        var result = Behavior_TextBoxCharacterCasing.NormalizeText("  old flatstock  ", exceptions);

        result.Should().Be("old flatstock");
    }

    [Fact]
    public void NormalizeText_ShouldUppercaseFreeformText_WhenValueDoesNotMatchException()
    {
        var exceptions = new List<string> { "Old Flatstock", "Refer to Vendor Tag" };

        var result = Behavior_TextBoxCharacterCasing.NormalizeText("  heat-12ab  ", exceptions);

        result.Should().Be("HEAT-12AB");
    }
}
