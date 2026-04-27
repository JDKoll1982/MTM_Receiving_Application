using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Core.Models.Core;
using Windows.System;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Helpers.UI;

public sealed class Helper_KeyboardShortcutsTests
{
    [Fact]
    public void NormalizeKey_ShouldCanonicalizeSlashAliases()
    {
        Helper_KeyboardShortcuts.NormalizeKey("?").Should().Be("Slash");
        Helper_KeyboardShortcuts.NormalizeKey("OemQuestion").Should().Be("Slash");
    }

    [Fact]
    public void ToDisplayText_ShouldFormatFriendlyModifierText()
    {
        var binding = new Model_KeyboardShortcutBinding { Key = "Slash", IsShiftEnabled = true };

        var displayText = Helper_KeyboardShortcuts.ToDisplayText(binding);

        displayText.Should().Be("Shift+Slash / Question (?)");
    }

    [Fact]
    public void GetModifiers_ShouldCombineConfiguredModifierFlags()
    {
        var binding = new Model_KeyboardShortcutBinding
        {
            Key = "Right",
            IsCtrlEnabled = true,
            IsShiftEnabled = true,
        };

        var modifiers = Helper_KeyboardShortcuts.GetModifiers(binding);

        modifiers.Should().Be(VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift);
    }
}
