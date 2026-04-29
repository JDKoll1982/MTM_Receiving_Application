using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Core.Services;

public class Service_Window : IService_Window
{
    public XamlRoot? GetXamlRoot()
    {
        return App.MainWindow?.Content?.XamlRoot;
    }

    public Task<bool> NavigateToSettingsPageAsync(Type pageType)
    {
        ArgumentNullException.ThrowIfNull(pageType);

        if (App.MainWindow is MainWindow mainWindow)
        {
            return mainWindow.NavigateToSettingsPageAsync(pageType);
        }

        return Task.FromResult(false);
    }
}
