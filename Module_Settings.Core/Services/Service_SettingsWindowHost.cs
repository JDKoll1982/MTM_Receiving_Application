using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Views;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Ensures a single instance of the Core Settings window.
/// </summary>
public class Service_SettingsWindowHost : IService_SettingsWindowHost
{
    private readonly IServiceProvider _serviceProvider;
    private Window? _settingsWindow;
    private Window? _ownerWindow;
    private Control? _ownerRootElement;

    public Service_SettingsWindowHost(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void ShowSettingsWindow()
    {
        if (_settingsWindow == null)
        {
            _settingsWindow = _serviceProvider.GetRequiredService<View_Settings_CoreWindow>();
            _settingsWindow.Closed += (_, _) =>
            {
                _settingsWindow?.SetAlwaysOnTop(false);
                _settingsWindow = null;
                if (_ownerRootElement != null)
                {
                    _ownerRootElement.IsEnabled = true;
                    _ownerWindow?.BringToFront();
                }
            };
        }

        _ownerWindow ??= App.MainWindow;
        if (_ownerWindow != null)
        {
            _ownerRootElement ??= _ownerWindow.Content as Control;
            if (_ownerRootElement != null)
            {
                _ownerRootElement.IsEnabled = false;
            }
        }

        _settingsWindow.Activate();
        _settingsWindow.SetAlwaysOnTop(true);
        _settingsWindow.BringToFront();
    }
}
