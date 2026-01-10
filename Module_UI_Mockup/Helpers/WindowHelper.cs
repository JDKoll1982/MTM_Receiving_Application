using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace Module_UI_Mockup.Helpers;

/// <summary>
/// Utility class for window sizing, positioning, and management.
/// Standalone implementation with no external dependencies.
/// Provides methods for setting window size, centering windows, and accessing AppWindow APIs.
/// </summary>
public static class WindowHelper
{
    /// <summary>
    /// Sets the window size to specified dimensions.
    /// </summary>
    /// <param name="window">The window to resize.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <example>
    /// WindowHelper.SetWindowSize(window, 1400, 900);
    /// </example>
    public static void SetWindowSize(Window window, int width, int height)
    {
        var appWindow = GetAppWindowForCurrentWindow(window);
        appWindow?.Resize(new Windows.Graphics.SizeInt32(width, height));
    }

    /// <summary>
    /// Centers the window on the primary display.
    /// Calculates center position based on display work area and window size.
    /// </summary>
    /// <param name="window">The window to center.</param>
    /// <example>
    /// WindowHelper.CenterWindow(window);
    /// </example>
    public static void CenterWindow(Window window)
    {
        var appWindow = GetAppWindowForCurrentWindow(window);
        if (appWindow != null)
        {
            var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            if (displayArea != null)
            {
                var centerX = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
                var centerY = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
                appWindow.Move(new Windows.Graphics.PointInt32(centerX, centerY));
            }
        }
    }

    /// <summary>
    /// Gets the AppWindow for the current Window instance.
    /// AppWindow provides access to advanced window management APIs including
    /// sizing, positioning, title bar customization, and presenter modes.
    /// </summary>
    /// <param name="window">The Window instance to get the AppWindow for.</param>
    /// <returns>AppWindow instance or null if unable to retrieve.</returns>
    /// <example>
    /// var appWindow = WindowHelper.GetAppWindowForCurrentWindow(window);
    /// if (appWindow != null)
    /// {
    ///     appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
    /// }
    /// </example>
    public static AppWindow? GetAppWindowForCurrentWindow(Window window)
    {
        var windowHandle = WindowNative.GetWindowHandle(window);
        var windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
        return AppWindow.GetFromWindowId(windowId);
    }
}
