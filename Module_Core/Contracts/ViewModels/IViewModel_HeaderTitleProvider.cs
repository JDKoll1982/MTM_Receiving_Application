using System.ComponentModel;

namespace MTM_Receiving_Application.Module_Core.Contracts.ViewModels;

/// <summary>
/// Provides the current title for the shared main-window header.
/// </summary>
public interface IViewModel_HeaderTitleProvider : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the current title that should be shown in the main-window header.
    /// </summary>
    string CurrentHeaderTitle { get; }
}