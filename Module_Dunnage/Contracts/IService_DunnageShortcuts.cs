using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Settings.Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Contracts;

/// <summary>
/// Loads and persists Dunnage workflow keyboard shortcut settings.
/// </summary>
public interface IService_DunnageShortcuts
{
    Task<Model_Settings_DunnageShortcuts> GetShortcutsAsync();

    Task SaveShortcutsAsync(Model_Settings_DunnageShortcuts shortcuts);

    Task SaveToggleSimpleNavigationEnabledAsync(bool isEnabled);
}
