using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Provides a shared main-window header back action that pages or hosted views can register.
/// </summary>
public interface IService_HeaderBackNavigation : INotifyPropertyChanged
{
    /// <summary>
    /// Gets a value indicating whether the shared header back button should be visible.
    /// </summary>
    bool IsBackButtonVisible { get; }

    /// <summary>
    /// Gets the tooltip shown for the shared header back button.
    /// </summary>
    string BackButtonToolTip { get; }

    /// <summary>
    /// Registers a shared header back action.
    /// </summary>
    /// <param name="action">The back action to run.</param>
    /// <param name="toolTip">The tooltip shown for the back button.</param>
    void RegisterBackAction(Func<Task> action, string toolTip = "Back");

    /// <summary>
    /// Executes the current shared header back action.
    /// </summary>
    /// <returns>A task that completes when the back action finishes.</returns>
    Task ExecuteBackActionAsync();

    /// <summary>
    /// Clears the shared header back action.
    /// </summary>
    void ClearBackAction();
}
