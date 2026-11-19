using System.ComponentModel;
using MTM_Receiving_Application.Module_Core.Models.UI;

namespace MTM_Receiving_Application.Module_Core.Contracts.ViewModels;

/// <summary>
/// Supplies the shell action bar with the positions and commands the active page offers.
/// </summary>
/// <remarks>
/// The shell owns the bar, so a page never draws its own navigation row. The contract speaks
/// only in booleans, strings, and commands, which keeps the shell independent of every module's
/// view-model types.
/// </remarks>
public interface IViewModel_ShellActionProvider : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the current snapshot. It is replaced whole on every step or page change, and
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> is raised for this member alone.
    /// </summary>
    Model_ShellActionBarState ShellActions { get; }

    /// <summary>
    /// Persists the signed-in user's default receiving mode without changing the active page.
    /// </summary>
    /// <param name="storedModeValue">
    /// The stored value to keep: guided, manual, edit, or null to clear the default.
    /// </param>
    void SetDefaultMode(string? storedModeValue);
}
