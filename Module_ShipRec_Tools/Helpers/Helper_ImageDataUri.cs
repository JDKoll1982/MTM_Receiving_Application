using System;
using System.IO;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;

/// <summary>
/// Converts image files into base64 data URIs so generated book HTML is self-contained.
/// </summary>
public static class Helper_ImageDataUri
{
    /// <summary>
    /// Returns a base64 data URI for the file at <paramref name="absolutePath"/>, or null
    /// when the path is blank, the file does not exist, or the file cannot be read.
    /// </summary>
    public static string? TryGetDataUri(string? absolutePath)
    {
        if (string.IsNullOrWhiteSpace(absolutePath) || File.Exists(absolutePath) is false)
        {
            return null;
        }

        try
        {
            var bytes = File.ReadAllBytes(absolutePath);
            return $"data:{GetMimeType(absolutePath)};base64,{Convert.ToBase64String(bytes)}";
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static string GetMimeType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream",
        };
    }
}
