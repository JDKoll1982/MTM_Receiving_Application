using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Core.Services.Database;

/// <summary>
/// Centralized error handling service
/// Logs errors and displays user-friendly WinUI 3 dialogs
/// </summary>
public class Service_ErrorHandler : IService_ErrorHandler
{
    private readonly IService_LoggingUtility _loggingService;
    private readonly IService_Window _windowService;

    public Service_ErrorHandler(IService_LoggingUtility loggingService, IService_Window windowService)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
    }

    /// <summary>
    /// Handles an error by logging it and optionally displaying a dialog
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <param name="severity"></param>
    /// <param name="exception"></param>
    /// <param name="showDialog"></param>
    public async Task HandleErrorAsync(
        string errorMessage,
        Enum_ErrorSeverity severity,
        Exception? exception = null,
        bool showDialog = true)
    {
        // Log the error
        await LogErrorAsync(errorMessage, severity, exception);

        // Show dialog if requested
        if (showDialog)
        {
            string title = GetDialogTitle(severity);
            await ShowErrorDialogAsync(title, errorMessage, severity);
        }
    }

    /// <summary>
    /// Logs an error without displaying a dialog
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <param name="severity"></param>
    /// <param name="exception"></param>
    public Task LogErrorAsync(
        string errorMessage,
        Enum_ErrorSeverity severity,
        Exception? exception = null)
    {
        return Task.Run(() =>
        {
            switch (severity)
            {
                case Enum_ErrorSeverity.Info:
                    _loggingService.LogInfo(errorMessage);
                    break;
                case Enum_ErrorSeverity.Warning:
                    _loggingService.LogWarning(errorMessage);
                    break;
                case Enum_ErrorSeverity.Error:
                    _loggingService.LogError(errorMessage, exception);
                    break;
                case Enum_ErrorSeverity.Critical:
                    _loggingService.LogCritical(errorMessage, exception);
                    break;
                case Enum_ErrorSeverity.Fatal:
                    _loggingService.LogFatal(errorMessage, exception);
                    break;
            }
        });
    }

    /// <summary>
    /// Displays a user-friendly error dialog using WinUI 3 ContentDialog
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="severity"></param>
    public async Task ShowErrorDialogAsync(
        string title,
        string message,
        Enum_ErrorSeverity severity)
    {
        try
        {
            // Get the main window's XamlRoot for dialog display
            // Note: App.MainWindow is static but might not be accessible in all contexts (e.g. tests)
            // We use a safe access pattern or dependency injection for window provider in a real app
            // For now, we assume App.MainWindow is available as it's a static property of the App class
            // However, since 'App' is in the root namespace, we might need to qualify it or ensure it's accessible.
            // If 'App' is not found, it might be because we are in a library or test context where App is not defined or accessible.
            // In WinUI 3, (Application.Current as App)?.MainWindow is a common pattern.

            // Using dynamic to avoid direct dependency on App class which might not be visible here
            // or simply checking Application.Current

            // Use IWindowService to get the XamlRoot
            var xamlRoot = _windowService.GetXamlRoot();

            if (xamlRoot == null)
            {
                // Fallback: Just log if we can't show dialog
                _loggingService.LogWarning("Cannot show dialog - XamlRoot is null");
                return;
            }

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = xamlRoot
            };

            // Set dialog style based on severity (could be enhanced with custom styles)
            // For now, just show the dialog
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            // If dialog fails, at least log it
            _loggingService.LogError($"Failed to show error dialog: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Handles a Model_Dao_Result error by logging and optionally displaying
    /// </summary>
    /// <param name="result"></param>
    /// <param name="operationName"></param>
    /// <param name="showDialog"></param>
    public async Task HandleDaoErrorAsync(
        Model_Dao_Result result,
        string operationName,
        bool showDialog = true)
    {
        if (result.Success)
        {
            // Not an error
            return;
        }

        string errorMessage = $"Database operation '{operationName}' failed: {result.ErrorMessage}";

        await HandleErrorAsync(
            errorMessage,
            result.Severity,
            exception: null,
            showDialog: showDialog
        );
    }

    public Task ShowUserErrorAsync(string message, string title, string method)
    {
        return ShowErrorDialogAsync(title, message, Enum_ErrorSeverity.Error);
    }

    public void HandleException(Exception ex, Enum_ErrorSeverity severity, string method, string className)
    {
        _ = HandleErrorAsync($"Exception in {className}.{method}: {ex.Message}", severity, ex, true);
    }

    /// <summary>
    /// Gets appropriate dialog title based on severity
    /// </summary>
    /// <param name="severity"></param>
    private static string GetDialogTitle(Enum_ErrorSeverity severity)
    {
        return severity switch
        {
            Enum_ErrorSeverity.Info => "Information",
            Enum_ErrorSeverity.Warning => "Warning",
            Enum_ErrorSeverity.Error => "Error",
            Enum_ErrorSeverity.Critical => "Critical Error",
            Enum_ErrorSeverity.Fatal => "Fatal Error",
            _ => "Error"
        };
    }
}

