using System.Collections.Generic;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Settings.Core.Helpers;

/// <summary>
/// Shared option catalog for settings pages that let users configure keyboard shortcuts.
/// </summary>
public static class Helper_Settings_KeyboardShortcutOptions
{
    /// <summary>
    /// Common non-modifier keys supported by workflow shortcut settings.
    /// </summary>
    public static IReadOnlyList<Model_Settings_KeyValueOption> CommonKeys { get; } =
        BuildCommonKeys();

    private static IReadOnlyList<Model_Settings_KeyValueOption> BuildCommonKeys()
    {
        var options = new List<Model_Settings_KeyValueOption>
        {
            new("Left Arrow", "Left"),
            new("Right Arrow", "Right"),
            new("Up Arrow", "Up"),
            new("Down Arrow", "Down"),
            new("Enter", "Enter"),
            new("Space", "Space"),
            new("Slash / Question (?)", "Slash"),
        };

        for (var letter = 'A'; letter <= 'Z'; letter++)
        {
            var value = letter.ToString();
            options.Add(new Model_Settings_KeyValueOption(value, value));
        }

        for (var number = 0; number <= 9; number++)
        {
            options.Add(new Model_Settings_KeyValueOption(number.ToString(), $"Number{number}"));
        }

        for (var functionKey = 1; functionKey <= 12; functionKey++)
        {
            var value = $"F{functionKey}";
            options.Add(new Model_Settings_KeyValueOption(value, value));
        }

        return options;
    }
}
