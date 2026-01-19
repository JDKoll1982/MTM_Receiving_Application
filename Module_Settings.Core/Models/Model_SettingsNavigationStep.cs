using System;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Represents a single selectable settings view within a settings navigation hub.
/// </summary>
public sealed class Model_SettingsNavigationStep
{
    public Model_SettingsNavigationStep(string title, Type viewType)
    {
        Title = string.IsNullOrWhiteSpace(title) ? "Settings" : title;
        ViewType = viewType ?? throw new ArgumentNullException(nameof(viewType));
    }

    /// <summary>
    /// Display title for the step. Used for header text and step button content.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// View type to navigate to when the step is selected.
    /// </summary>
    public Type ViewType { get; }
}
