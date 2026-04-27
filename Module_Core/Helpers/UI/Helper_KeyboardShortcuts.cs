using System;
using System.Collections.Generic;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Models.Core;
using Windows.System;
using Windows.UI.Core;

namespace MTM_Receiving_Application.Module_Core.Helpers.UI;

/// <summary>
/// Shared helper methods for converting persisted shortcut settings into WinUI keyboard accelerators.
/// </summary>
public static class Helper_KeyboardShortcuts
{
    private const int OemQuestionVirtualKeyCode = 191;

    private static readonly IReadOnlyDictionary<string, VirtualKey> KeyAliases = new Dictionary<
        string,
        VirtualKey
    >(StringComparer.OrdinalIgnoreCase)
    {
        ["Left"] = VirtualKey.Left,
        ["Right"] = VirtualKey.Right,
        ["Up"] = VirtualKey.Up,
        ["Down"] = VirtualKey.Down,
        ["Enter"] = VirtualKey.Enter,
        ["Space"] = VirtualKey.Space,
        ["Slash"] = (VirtualKey)OemQuestionVirtualKeyCode,
        ["Question"] = (VirtualKey)OemQuestionVirtualKeyCode,
        ["OemQuestion"] = (VirtualKey)OemQuestionVirtualKeyCode,
        ["/"] = (VirtualKey)OemQuestionVirtualKeyCode,
        ["?"] = (VirtualKey)OemQuestionVirtualKeyCode,
    };

    private static readonly IReadOnlyDictionary<string, string> DisplayAliases = new Dictionary<
        string,
        string
    >(StringComparer.OrdinalIgnoreCase)
    {
        ["Left"] = "Left",
        ["Right"] = "Right",
        ["Up"] = "Up",
        ["Down"] = "Down",
        ["Enter"] = "Enter",
        ["Space"] = "Space",
        ["Slash"] = "Slash / Question (?)",
        ["Question"] = "Slash / Question (?)",
        ["OemQuestion"] = "Slash / Question (?)",
        ["/"] = "Slash / Question (?)",
        ["?"] = "Slash / Question (?)",
    };

    /// <summary>
    /// Creates a WinUI keyboard accelerator from a persisted binding.
    /// </summary>
    /// <param name="binding">The persisted binding definition.</param>
    /// <returns>A keyboard accelerator, or <c>null</c> when the binding is invalid.</returns>
    public static KeyboardAccelerator? CreateAccelerator(Model_KeyboardShortcutBinding? binding)
    {
        if (binding is null || !TryParseVirtualKey(binding.Key, out var key))
        {
            return null;
        }

        return new KeyboardAccelerator { Key = key, Modifiers = GetModifiers(binding) };
    }

    /// <summary>
    /// Converts shortcut flags into WinUI modifier flags.
    /// </summary>
    /// <param name="binding">The shortcut binding to inspect.</param>
    /// <returns>The combined WinUI modifier flags.</returns>
    public static VirtualKeyModifiers GetModifiers(Model_KeyboardShortcutBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        var modifiers = VirtualKeyModifiers.None;

        if (binding.IsCtrlEnabled)
        {
            modifiers |= VirtualKeyModifiers.Control;
        }

        if (binding.IsShiftEnabled)
        {
            modifiers |= VirtualKeyModifiers.Shift;
        }

        if (binding.IsAltEnabled)
        {
            modifiers |= VirtualKeyModifiers.Menu;
        }

        if (binding.IsWindowsEnabled)
        {
            modifiers |= VirtualKeyModifiers.Windows;
        }

        return modifiers;
    }

    /// <summary>
    /// Normalizes persisted key values so future views can store stable names independent of aliases.
    /// </summary>
    /// <param name="key">The stored or user-entered key value.</param>
    /// <returns>A canonical key value suitable for storage.</returns>
    public static string NormalizeKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        var trimmedKey = key.Trim();

        if (KeyAliases.TryGetValue(trimmedKey, out var parsedAlias))
        {
            return parsedAlias == (VirtualKey)OemQuestionVirtualKeyCode ? "Slash" : trimmedKey;
        }

        return Enum.TryParse<VirtualKey>(trimmedKey, true, out var parsedKey)
            ? parsedKey.ToString()
            : trimmedKey;
    }

    /// <summary>
    /// Produces human-readable text for settings pages and status notes.
    /// </summary>
    /// <param name="binding">The binding to format.</param>
    /// <returns>A concise shortcut label.</returns>
    public static string ToDisplayText(Model_KeyboardShortcutBinding? binding)
    {
        if (binding is null || string.IsNullOrWhiteSpace(binding.Key))
        {
            return "Not Set";
        }

        var parts = new List<string>();

        if (binding.IsCtrlEnabled)
        {
            parts.Add("Ctrl");
        }

        if (binding.IsShiftEnabled)
        {
            parts.Add("Shift");
        }

        if (binding.IsAltEnabled)
        {
            parts.Add("Alt");
        }

        if (binding.IsWindowsEnabled)
        {
            parts.Add("Win");
        }

        var normalizedKey = NormalizeKey(binding.Key);
        parts.Add(
            DisplayAliases.TryGetValue(normalizedKey, out var displayKey)
                ? displayKey
                : normalizedKey
        );

        return string.Join("+", parts);
    }

    /// <summary>
    /// Prevents plain-arrow navigation shortcuts from hijacking text editing interactions.
    /// </summary>
    /// <param name="focusedElement">The currently focused element.</param>
    /// <param name="binding">The binding being invoked.</param>
    /// <returns><c>true</c> when the shortcut should be ignored for the current focus target.</returns>
    public static bool ShouldIgnoreForFocusedInput(
        object? focusedElement,
        Model_KeyboardShortcutBinding? binding
    )
    {
        if (focusedElement is null || binding is null)
        {
            return false;
        }

        var normalizedKey = NormalizeKey(binding.Key);
        var isPlainArrowShortcut =
            (
                normalizedKey.Equals("Left", StringComparison.OrdinalIgnoreCase)
                || normalizedKey.Equals("Right", StringComparison.OrdinalIgnoreCase)
            )
            && GetModifiers(binding) == VirtualKeyModifiers.None;

        if (!isPlainArrowShortcut)
        {
            return false;
        }

        return focusedElement
            is TextBox
                or RichEditBox
                or AutoSuggestBox
                or PasswordBox
                or NumberBox
                or ComboBox;
    }

    /// <summary>
    /// Determines whether a routed key event matches a persisted shortcut binding.
    /// </summary>
    /// <param name="key">The routed event key.</param>
    /// <param name="binding">The persisted shortcut binding.</param>
    /// <returns><c>true</c> when the current key and modifier state match the binding exactly.</returns>
    public static bool DoesCurrentKeyEventMatch(
        VirtualKey key,
        Model_KeyboardShortcutBinding? binding
    )
    {
        if (binding is null || !TryParseVirtualKey(binding.Key, out var expectedKey))
        {
            return false;
        }

        return key == expectedKey && DoCurrentModifiersMatch(binding);
    }

    /// <summary>
    /// Determines whether the currently pressed modifier state matches a binding exactly.
    /// </summary>
    /// <param name="binding">The persisted shortcut binding.</param>
    /// <returns><c>true</c> when the current modifier state matches the binding.</returns>
    public static bool DoCurrentModifiersMatch(Model_KeyboardShortcutBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        return IsModifierPressed(VirtualKey.Control) == binding.IsCtrlEnabled
            && IsModifierPressed(VirtualKey.Shift) == binding.IsShiftEnabled
            && IsModifierPressed(VirtualKey.Menu) == binding.IsAltEnabled
            && IsWindowsModifierPressed() == binding.IsWindowsEnabled;
    }

    private static bool IsWindowsModifierPressed()
    {
        return IsModifierPressed(VirtualKey.LeftWindows)
            || IsModifierPressed(VirtualKey.RightWindows);
    }

    private static bool IsModifierPressed(VirtualKey key)
    {
        var state = InputKeyboardSource.GetKeyStateForCurrentThread(key);
        return (state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
    }

    /// <summary>
    /// Parses a persisted key value into the WinRT <see cref="VirtualKey"/> enum.
    /// </summary>
    /// <param name="key">The stored key value.</param>
    /// <param name="virtualKey">The parsed virtual key.</param>
    /// <returns><c>true</c> when the key could be parsed.</returns>
    public static bool TryParseVirtualKey(string? key, out VirtualKey virtualKey)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            virtualKey = default;
            return false;
        }

        var normalizedKey = NormalizeKey(key);

        if (KeyAliases.TryGetValue(normalizedKey, out virtualKey))
        {
            return true;
        }

        if (Enum.TryParse(normalizedKey, true, out virtualKey))
        {
            return true;
        }

        if (uint.TryParse(normalizedKey, out var rawValue))
        {
            virtualKey = (VirtualKey)rawValue;
            return true;
        }

        virtualKey = default;
        return false;
    }
}
