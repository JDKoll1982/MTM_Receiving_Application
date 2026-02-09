using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;

/// <summary>
/// Error handler for the Settings window. Shows error dialogs within the Settings window
/// context instead of the main window.
/// </summary>
public interface IService_SettingsErrorHandler
{
    /// <summary>
    /// Handles an error and displays it in the Settings window.
    /// </summary>
    /// <param name="message">Error message to display</param>
    /// <param name="title">Dialog title</param>
    /// <param name="severity">Error severity level</param>
    /// <returns>Task representing the async operation</returns>
    Task HandleErrorAsync(string message, string title, Enum_ErrorSeverity severity = Enum_ErrorSeverity.Error);

    /// <summary>
    /// Handles an error with exception details and displays it in the Settings window.
    /// </summary>
    /// <param name="message">Error message to display</param>
    /// <param name="title">Dialog title</param>
    /// <param name="ex">Exception object for logging</param>
    /// <param name="severity">Error severity level</param>
    /// <returns>Task representing the async operation</returns>
    Task HandleErrorAsync(string message, string title, Exception ex, Enum_ErrorSeverity severity = Enum_ErrorSeverity.Error);

    /// <summary>
    /// Shows a success message in the Settings window.
    /// </summary>
    /// <param name="message">Success message to display</param>
    /// <param name="title">Dialog title</param>
    /// <returns>Task representing the async operation</returns>
    Task ShowSuccessAsync(string message, string title);
}
