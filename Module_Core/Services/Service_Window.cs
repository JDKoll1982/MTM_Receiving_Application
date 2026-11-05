using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Core.Services;

public class Service_Window : IService_Window
{
    public XamlRoot? GetXamlRoot()
    {
        try
        {
            return App.MainWindow?.Content?.XamlRoot;
        }
        catch (Exception)
        {
            // Reading a window's content throws a COMException until the window has finished coming up,
            // which is exactly when startup is asking — during the splash, before the main window has
            // been activated. Reporting "no root yet" is the honest answer there; letting the exception
            // out turned every startup error report into a second, unrelated failure.
            return null;
        }
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
