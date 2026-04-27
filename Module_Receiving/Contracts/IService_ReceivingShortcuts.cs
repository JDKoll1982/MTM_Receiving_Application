using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Settings.Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Contracts;

/// <summary>
/// Retrieves and persists user-configurable keyboard shortcuts for the receiving workflow.
/// </summary>
public interface IService_ReceivingShortcuts
{
    /// <summary>
    /// Gets the current user's receiving workflow shortcuts, falling back to defaults when needed.
    /// </summary>
    /// <returns>The current shortcut configuration.</returns>
    Task<Model_Settings_ReceivingShortcuts> GetShortcutsAsync();

    /// <summary>
    /// Saves the current user's receiving workflow shortcuts.
    /// </summary>
    /// <param name="shortcuts">The shortcut configuration to persist.</param>
    Task SaveShortcutsAsync(Model_Settings_ReceivingShortcuts shortcuts);

    /// <summary>
    /// Saves only the simple navigation toggle state without forcing the caller to persist other edits.
    /// </summary>
    /// <param name="isEnabled">Whether Ctrl+T should enable plain arrow navigation.</param>
    Task SaveToggleSimpleNavigationEnabledAsync(bool isEnabled);
}
