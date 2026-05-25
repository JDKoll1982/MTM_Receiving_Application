using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.Views;

namespace MTM_Receiving_Application.Module_Core.Helpers;

public static class Helper_UI_ContentDialogTheme
{
    public static void ApplyTheme(ContentDialog dialog, XamlRoot? preferredXamlRoot = null)
    {
        System.ArgumentNullException.ThrowIfNull(dialog);

        var themeSource = ResolveThemeSource(preferredXamlRoot ?? dialog.XamlRoot);
        if (themeSource is not null)
        {
            dialog.RequestedTheme = themeSource.RequestedTheme;
        }
    }

    private static FrameworkElement? ResolveThemeSource(XamlRoot? xamlRoot)
    {
        if (
            App.MainWindow?.Content is FrameworkElement mainContent
            && mainContent.XamlRoot == xamlRoot
        )
        {
            return mainContent;
        }

        if (
            View_Settings_CoreWindow.GetInstance()?.Content is FrameworkElement settingsContent
            && settingsContent.XamlRoot == xamlRoot
        )
        {
            return settingsContent;
        }

        if (App.MainWindow?.Content is FrameworkElement mainFallback)
        {
            return mainFallback;
        }

        return View_Settings_CoreWindow.GetInstance()?.Content as FrameworkElement;
    }
}
