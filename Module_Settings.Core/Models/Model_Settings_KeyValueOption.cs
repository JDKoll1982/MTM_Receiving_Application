namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Reusable display and value pair for settings combo-box options.
/// </summary>
/// <param name="DisplayName">Text shown to the user.</param>
/// <param name="Value">Underlying persisted value.</param>
public sealed record Model_Settings_KeyValueOption(string DisplayName, string Value);
