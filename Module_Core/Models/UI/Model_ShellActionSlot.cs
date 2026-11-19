using System.Windows.Input;

namespace MTM_Receiving_Application.Module_Core.Models.UI;

/// <summary>
/// One named position in the shell action bar's right-hand group.
/// </summary>
/// <remarks>
/// Back, Next, Clear Label Data and Help share this shape, so the bar renders every position the
/// same way and a page-specific verb swaps in by changing only the label and the command.
/// </remarks>
public sealed class Model_ShellActionSlot
{
    /// <summary>
    /// Gets a value indicating whether the control is shown. False removes it instead of
    /// disabling it, which is how a page reports that it cannot go back.
    /// </summary>
    public bool IsVisible { get; init; }

    /// <summary>
    /// Gets a value indicating whether the control is available. False renders it disabled but
    /// still present.
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Gets the button text.
    /// </summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// Gets the tooltip text.
    /// </summary>
    public string Tooltip { get; init; } = string.Empty;

    /// <summary>
    /// Gets the automation and screen-reader name.
    /// </summary>
    public string AccessibilityName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the command the page already used for this action. The bar never supplies its own.
    /// </summary>
    public ICommand? Command { get; init; }

    /// <summary>
    /// Gets a value indicating whether a divider follows this position.
    /// </summary>
    public bool ShowDividerAfter { get; init; }

    /// <summary>
    /// Gets a position that is not shown.
    /// </summary>
    public static Model_ShellActionSlot Hidden { get; } = new();
}
