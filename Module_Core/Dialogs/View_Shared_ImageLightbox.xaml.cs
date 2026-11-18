using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MTM_Receiving_Application.Module_Core.Dialogs;

/// <summary>
/// Full-window lightbox that shows one image at its original aspect ratio over a dimmed
/// backdrop. It has no title bar: tapping outside the image, or pressing any key, closes it.
/// </summary>
public sealed partial class View_Shared_ImageLightbox : UserControl
{
    /// <summary>
    /// Raised once the lightbox has closed so the host can remove it from the overlay.
    /// </summary>
    public event EventHandler? Closed;

    public View_Shared_ImageLightbox()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Loads the image at full resolution and takes focus so key presses close the lightbox.
    /// </summary>
    public void ShowImage(string? absoluteImagePath)
    {
        if (
            string.IsNullOrWhiteSpace(absoluteImagePath)
            || File.Exists(absoluteImagePath) is false
        )
        {
            Close();
            return;
        }

        LightboxImage.Source = new BitmapImage(new Uri(absoluteImagePath));
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Focus(FocusState.Programmatic);
    }

    private void OnImageTapped(object sender, TappedRoutedEventArgs e)
    {
        // A tap on the picture itself keeps the lightbox open.
        e.Handled = true;
    }

    private void OnScrimTapped(object sender, TappedRoutedEventArgs e)
    {
        Close();
    }

    private void OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        e.Handled = true;
        Close();
    }

    private void Close()
    {
        var handler = Closed;
        Closed = null;
        handler?.Invoke(this, EventArgs.Empty);
    }
}
