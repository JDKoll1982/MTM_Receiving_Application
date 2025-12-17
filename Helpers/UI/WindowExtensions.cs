using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace MTM_Receiving_Application.Helpers.UI;

/// <summary>
/// Extension methods for WinUI 3 Window configuration
/// Provides simplified window sizing, positioning, and presenter configuration
/// </summary>
public static class WindowExtensions
{
    /// <summary>
    /// Sets the window size
    /// </summary>
    /// <param name="window">The window to resize</param>
    /// <param name="width">Desired width in pixels</param>
    /// <param name="height">Desired height in pixels</param>
    public static void SetWindowSize(this Window window, int width, int height)
    {
        var appWindow = window.GetAppWindow();
        appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
    }

    /// <summary>
    /// Centers the window on the primary display
    /// </summary>
    /// <param name="window">The window to center</param>
    public static void CenterOnScreen(this Window window)
    {
        var appWindow = window.GetAppWindow();
        var displayArea = DisplayArea.Primary;
        var workArea = displayArea.WorkArea;

        var x = (workArea.Width - appWindow.Size.Width) / 2;
        var y = (workArea.Height - appWindow.Size.Height) / 2;

        appWindow.Move(new Windows.Graphics.PointInt32(x, y));
    }

    /// <summary>
    /// Configures the window as non-resizable (fixed size)
    /// </summary>
    /// <param name="window">The window to configure</param>
    /// <param name="disableMaximize">Whether to disable maximize button</param>
    /// <param name="disableMinimize">Whether to disable minimize button</param>
    public static void SetFixedSize(this Window window, bool disableMaximize = true, bool disableMinimize = true)
    {
        var appWindow = window.GetAppWindow();
        
        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = !disableMaximize;
            presenter.IsMinimizable = !disableMinimize;
        }
    }

    /// <summary>
    /// Hides the title bar icon and system menu
    /// </summary>
    /// <param name="window">The window to configure</param>
    public static void HideTitleBarIcon(this Window window)
    {
        var appWindow = window.GetAppWindow();
        var titleBar = appWindow.TitleBar;
        titleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
    }

    /// <summary>
    /// Extends content into the title bar area (custom title bar)
    /// </summary>
    /// <param name="window">The window to configure</param>
    /// <param name="customTitleBarElement">Optional custom title bar element</param>
    public static void UseCustomTitleBar(this Window window, UIElement? customTitleBarElement = null)
    {
        window.ExtendsContentIntoTitleBar = true;
        window.SetTitleBar(customTitleBarElement);
    }

    /// <summary>
    /// Makes the title bar transparent (for splash screens)
    /// </summary>
    /// <param name="window">The window to configure</param>
    public static void MakeTitleBarTransparent(this Window window)
    {
        var appWindow = window.GetAppWindow();
        var titleBar = appWindow.TitleBar;
        
        titleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
        titleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
        titleBar.ButtonForegroundColor = Microsoft.UI.Colors.Transparent;
        titleBar.ButtonInactiveForegroundColor = Microsoft.UI.Colors.Transparent;
        titleBar.ButtonHoverBackgroundColor = Microsoft.UI.Colors.Transparent;
        titleBar.ButtonHoverForegroundColor = Microsoft.UI.Colors.Transparent;
        titleBar.ButtonPressedBackgroundColor = Microsoft.UI.Colors.Transparent;
        titleBar.ButtonPressedForegroundColor = Microsoft.UI.Colors.Transparent;
    }

    /// <summary>
    /// Gets the AppWindow from a WinUI 3 Window
    /// </summary>
    private static AppWindow GetAppWindow(this Window window)
    {
        var hWnd = WindowNative.GetWindowHandle(window);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(windowId);
    }
}
