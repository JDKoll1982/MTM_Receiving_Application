using System;
using System.IO;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MTM_Receiving_Application.Module_Dunnage.Helpers;

/// <summary>
/// Centralizes resolution of Dunnage image files stored under the configured Dunnage image root.
/// </summary>
public static class Helper_DunnageImagePaths
{
    private static readonly string DefaultRootFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MTM_Receiving_Application",
        "DunnageImages"
    );

    private static string _rootFolder = DefaultRootFolder;

    public static string RootFolder => _rootFolder;

    public static void SetRootFolder(string? configuredRootFolder)
    {
        _rootFolder = string.IsNullOrWhiteSpace(configuredRootFolder)
            ? DefaultRootFolder
            : configuredRootFolder.Trim();
    }

    public static string? GetAbsolutePath(string? relativeImagePath)
    {
        if (string.IsNullOrWhiteSpace(relativeImagePath))
        {
            return null;
        }

        var normalizedPath = relativeImagePath.Replace('/', Path.DirectorySeparatorChar);

        if (Path.IsPathRooted(normalizedPath))
        {
            return normalizedPath;
        }

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
