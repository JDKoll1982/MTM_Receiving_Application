using Microsoft.UI.Xaml;

namespace MTM_Receiving_Application.Module_Core.Helpers.UI;

/// <summary>
/// Helper for consistent window sizing and startup positioning.
/// </summary>
public static class WindowHelper_WindowSizeAndStartupLocation
{
    public static void SetWindowSize(Window window, int width, int height)
    {
        window.SetWindowSize(width, height);
        window.CenterOnScreen();
    }
}
