using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Services.Database;

/// <summary>
/// Centralized error handling service
/// Logs errors and displays user-friendly WinUI 3 dialogs
/// </summary>
public class Service_ErrorHandler : IService_ErrorHandler
{
    private readonly ILoggingService _loggingService;

    public Service_ErrorHandler(ILoggingService loggingService)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
    }

    /// <summary>
    /// Handles an error by logging it and optionally displaying a dialog
    /// </summary>
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
    public async Task ShowErrorDialogAsync(
        string title,
        string message,
        Enum_ErrorSeverity severity)
    {
        try
        {
            // Get the main window's XamlRoot for dialog display
            var xamlRoot = App.MainWindow?.Content?.XamlRoot;

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

    /// <summary>
    /// Gets appropriate dialog title based on severity
    /// </summary>
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
