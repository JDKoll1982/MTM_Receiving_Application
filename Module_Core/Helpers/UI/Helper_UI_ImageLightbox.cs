using System.IO;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Dialogs;

namespace MTM_Receiving_Application.Module_Core.Helpers.UI;

/// <summary>
/// Shows an image at full size in a modal lightbox that covers the whole application window.
/// </summary>
public static class Helper_UI_ImageLightbox
{
    /// <summary>
    /// Shows the image at its original aspect ratio over a dimmed backdrop. Returns false when
    /// there is no window overlay to mount into or the image file is missing.
    /// </summary>
    public static bool Show(string? absoluteImagePath)
    {
        if (
            App.MainWindow is not MainWindow mainWindow
            || string.IsNullOrWhiteSpace(absoluteImagePath)
            || File.Exists(absoluteImagePath) is false
        )
        {
            return false;
        }

        var host = mainWindow.ModalOverlayHost;
        if (host is null)
        {
            return false;
        }

        var lightbox = new View_Shared_ImageLightbox();
        lightbox.Closed += (_, _) =>
        {
            host.Content = null;
            host.Visibility = Visibility.Collapsed;
        };

        host.Content = lightbox;
        host.Visibility = Visibility.Visible;
        lightbox.ShowImage(absoluteImagePath);
        return true;
    }
}
