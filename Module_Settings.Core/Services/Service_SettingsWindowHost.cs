using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
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

    public Service_SettingsWindowHost(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void ShowSettingsWindow()
    {
        if (_settingsWindow == null)
        {
            _settingsWindow = _serviceProvider.GetRequiredService<View_Settings_CoreWindow>();
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        }

        _settingsWindow.Activate();
    }
}
