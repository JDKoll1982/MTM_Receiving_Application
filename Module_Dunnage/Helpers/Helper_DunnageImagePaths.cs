using System;
using System.IO;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MTM_Receiving_Application.Module_Dunnage.Helpers;

/// <summary>
/// Centralizes resolution of Dunnage image files stored in local app data.
/// </summary>
public static class Helper_DunnageImagePaths
{
    public static string RootFolder { get; } =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MTM_Receiving_Application",
            "DunnageImages"
        );

    public static string? GetAbsolutePath(string? relativeImagePath)
    {
        if (string.IsNullOrWhiteSpace(relativeImagePath))
        {
            return null;
        }

        var normalizedPath = relativeImagePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(RootFolder, normalizedPath);
    }

    public static ImageSource? CreateImageSource(string? relativeImagePath)
    {
        var absolutePath = GetAbsolutePath(relativeImagePath);
        if (string.IsNullOrWhiteSpace(absolutePath) || File.Exists(absolutePath) is false)
        {
            return null;
        }

        return new BitmapImage(new Uri(absolutePath));
    }
}
