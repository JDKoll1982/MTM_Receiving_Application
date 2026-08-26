using System.ComponentModel;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Contracts;

/// <summary>
/// Coordinates which scanner page is active inside the module host.
/// </summary>
public interface IService_ScannerNavigation : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the currently selected scanner page.
    /// </summary>
    Enum_ScannerPage CurrentPage { get; }

    /// <summary>
    /// Shows the workbench page.
    /// </summary>
    void ShowWorkbench();

    /// <summary>
    /// Shows the history page.
    /// </summary>
    void ShowHistory();

    /// <summary>
    /// Shows the settings page.
    /// </summary>
    void ShowSettings();
}
