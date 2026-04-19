using System;
using Microsoft.UI.Xaml;

namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// Provides a shared host contract for settings navigation shells.
/// </summary>
public interface ISettingsNavigationHost
{
    bool NavigateToPage(Type pageType);
    void UpdateHeaderForPageType(Type pageType);
    FrameworkElement? GetContentRoot();
    Window? GetHostWindow();
}
