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
    private static readonly string DefaultSharedRootFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MTM_Receiving_Application",
        "DunnageImages"
    );
    private static readonly string DefaultLocalCacheRootFolder = Path.Combine(
        AppContext.BaseDirectory,
        "Assets",
        "DunnageImages"
    );

    private static string _sharedRootFolder = DefaultSharedRootFolder;
    private static string _localCacheRootFolder = DefaultLocalCacheRootFolder;

    public static string RootFolder => _sharedRootFolder;

    public static string SharedRootFolder => _sharedRootFolder;

    public static string LocalCacheRootFolder => _localCacheRootFolder;

    public static void SetRootFolder(string? configuredRootFolder)
    {
        _sharedRootFolder = string.IsNullOrWhiteSpace(configuredRootFolder)
            ? DefaultSharedRootFolder
            : configuredRootFolder.Trim();
    }

    public static void SetLocalCacheRootFolder(string? localCacheRootFolder)
    {
        _localCacheRootFolder = string.IsNullOrWhiteSpace(localCacheRootFolder)
            ? DefaultLocalCacheRootFolder
            : localCacheRootFolder.Trim();
    }

    public static void EnsureLocalCacheFolder()
    {
        Directory.CreateDirectory(_localCacheRootFolder);
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

        return Path.Combine(SharedRootFolder, normalizedPath);
    }

    public static string? GetCachedAbsolutePath(string? relativeImagePath)
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

        return Path.Combine(LocalCacheRootFolder, normalizedPath);
    }

    public static string? GetDisplayAbsolutePath(string? relativeImagePath)
    {
        if (string.IsNullOrWhiteSpace(relativeImagePath))
        {
            return null;
        }

        var cachedPath = GetCachedAbsolutePath(relativeImagePath);
        if (string.IsNullOrWhiteSpace(cachedPath) is false && File.Exists(cachedPath))
        {
            return cachedPath;
        }

        return GetAbsolutePath(relativeImagePath);
    }

    public static ImageSource? CreateImageSource(string? relativeImagePath)
    {
        var absolutePath = GetDisplayAbsolutePath(relativeImagePath);
        if (string.IsNullOrWhiteSpace(absolutePath) || File.Exists(absolutePath) is false)
        {
            return null;
        }

        return new BitmapImage(new Uri(absolutePath));
    }
}
