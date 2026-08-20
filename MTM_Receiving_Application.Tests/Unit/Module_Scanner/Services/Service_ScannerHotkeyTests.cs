using FluentAssertions;
using MTM_Receiving_Application.Module_Scanner.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Services;

public sealed class Service_ScannerHotkeyTests
{
    [Theory]
    [InlineData("Ctrl+Alt+M", 0x0001 | 0x0002, 0x4D)] // MOD_ALT | MOD_CONTROL, VK_M
    [InlineData("Ctrl+Alt+N", 0x0001 | 0x0002, 0x4E)] // MOD_ALT | MOD_CONTROL, VK_N
    [InlineData("Alt+Shift+F5", 0x0001 | 0x0004, 0x74)] // MOD_ALT | MOD_SHIFT, VK_F5
    [InlineData("ctrl+alt+m", 0x0001 | 0x0002, 0x4D)] // Case-insensitive
    [InlineData("Control+Alt+M", 0x0001 | 0x0002, 0x4D)] // "Control" alias
    public void TryParseChord_ShouldParse_WhenChordIsValid(
        string chord,
        uint expectedModifiers,
        ushort expectedVirtualKey
    )
    {
        var parsed = Service_ScannerHotkey.TryParseChord(
            chord,
            out var modifiers,
            out var virtualKey
        );

        parsed.Should().BeTrue();
        modifiers.Should().Be(expectedModifiers);
        virtualKey.Should().Be(expectedVirtualKey);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("M")]
    [InlineData("Ctrl")]
    [InlineData("Ctrl+Alt")]
    [InlineData("Super+M")]
    [InlineData("Ctrl+Alt+")]
    [InlineData("Ctrl+Alt+123")]
    public void TryParseChord_ShouldFail_WhenChordIsInvalid(string? chord)
    {
        var parsed = Service_ScannerHotkey.TryParseChord(
            chord!,
            out var modifiers,
            out var virtualKey
        );

        parsed.Should().BeFalse();
        modifiers.Should().Be(0);
        virtualKey.Should().Be(0);
    }
}
