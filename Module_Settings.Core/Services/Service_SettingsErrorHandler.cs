using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Views;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using Microsoft.UI.Xaml;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Error handler for the Settings window. Shows error dialogs within the Settings window
/// context instead of the main application window.
/// </summary>
public class Service_SettingsErrorHandler : IService_SettingsErrorHandler
{
    private readonly IService_LoggingUtility _logger;

    public Service_SettingsErrorHandler(IService_LoggingUtility logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleErrorAsync(string message, string title, Enum_ErrorSeverity severity = Enum_ErrorSeverity.Error)
    {
        _logger.LogError($"{title}: {message}");

        var settingsWindow = View_Settings_CoreWindow.GetInstance();
        if (settingsWindow == null)
        {
            // Fallback if Settings window not available
            await ShowDialogAsync(null, title, message, severity);
            return;
        }

        await ShowDialogAsync(settingsWindow, title, message, severity);
    }

    public async Task HandleErrorAsync(string message, string title, Exception ex, Enum_ErrorSeverity severity = Enum_ErrorSeverity.Error)
    {
        _logger.LogError($"{title}: {message} - Exception: {ex?.Message}");

        var settingsWindow = View_Settings_CoreWindow.GetInstance();
        if (settingsWindow == null)
        {
            await ShowDialogAsync(null, title, message, severity);
            return;
        }

        await ShowDialogAsync(settingsWindow, title, message, severity);
    }

    public async Task ShowSuccessAsync(string message, string title)
    {
        _logger.LogInfo($"{title}: {message}");

        var settingsWindow = View_Settings_CoreWindow.GetInstance();
        if (settingsWindow == null)
        {
            await ShowDialogAsync(null, title, message, Enum_ErrorSeverity.Info);
            return;
        }

        await ShowDialogAsync(settingsWindow, title, message, Enum_ErrorSeverity.Info);
    }

    private async Task ShowDialogAsync(Window? parentWindow, string title, string message, Enum_ErrorSeverity severity)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = new TextBlock { Text = message, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap },
            CloseButtonText = "OK",
            XamlRoot = parentWindow?.Content?.XamlRoot
        };

        // Style the dialog based on severity
        var backgroundColor = severity switch
        {
            Enum_ErrorSeverity.Error => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
            Enum_ErrorSeverity.Warning => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Orange),
            _ => new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green)
        };

        try
        {
            if (parentWindow != null)
            {
                // Show dialog in Settings window context
                await dialog.ShowAsync();
            }
            else
            {
                // Fallback to showing dialog without parent window
                await dialog.ShowAsync();
            }
        }
        catch
        {
            // Silently fail if dialog can't be shown
        }
    }
}
