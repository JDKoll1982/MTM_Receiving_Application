using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using Windows.Graphics.Imaging;

namespace MTM_Receiving_Application.Module_Dunnage.Services;

/// <summary>
/// Stores Dunnage image files under the configured Dunnage image root and exposes relative-path persistence.
/// </summary>
public class Service_DunnageImageStorage : IService_DunnageImageStorage
{
    private const string SettingsCategory = "Dunnage";
    private const string TypeImagePrefix = "DunnageType";
    private static readonly string[] SupportedExtensions = [".png", ".jpg", ".jpeg"];

    private readonly IService_SettingsCoreFacade _settingsCore;

    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    private static extern int WNetGetConnection(
        string localName,
        StringBuilder remoteName,
        ref int length
    );

    public Service_DunnageImageStorage(IService_SettingsCoreFacade settingsCore)
    {
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
    }

    public async Task RefreshConfiguredRootFolderAsync()
    {
        try
        {
            var result = await _settingsCore.GetSettingAsync(
                SettingsCategory,
                DunnageSettingsKeys.Application.DefaultImageLocation
            );

            var configuredRootFolder =
                result.IsSuccess && result.Data is not null
                    ? result.Data.Value?.Trim()
                    : string.Empty;

            Helper_DunnageImagePaths.SetRootFolder(configuredRootFolder);
        }
        catch
        {
            Helper_DunnageImagePaths.SetRootFolder(null);
        }
    }

    public async Task<string?> GetConfiguredRootFolderAsync()
    {
        await RefreshConfiguredRootFolderAsync();

        return Directory.Exists(Helper_DunnageImagePaths.RootFolder)
            ? Helper_DunnageImagePaths.RootFolder
            : null;
    }

    public async Task<Model_Dao_Result<string>> CreateRotatedWorkingCopyAsync(string imagePath)
    {
        string? targetFilePath = null;

        try
        {
            var absolutePath = ResolveAbsoluteImagePath(imagePath);
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return Model_Dao_Result_Factory.Failure<string>("Image file path is required.");
            }

            if (File.Exists(absolutePath) is false)
            {
                return Model_Dao_Result_Factory.Failure<string>(
                    "Selected image file was not found."
                );
            }

            var extension = Path.GetExtension(absolutePath);
            if (IsSupportedExtension(extension) is false)
            {
                return Model_Dao_Result_Factory.Failure<string>(
                    "Only PNG and JPG images are supported."
                );
            }

            var tempDirectory = Path.Combine(Helper_DunnageImagePaths.RootFolder, "Temp");
            Directory.CreateDirectory(tempDirectory);

            targetFilePath = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}{extension}");

            await using var sourceStream = File.Open(
                absolutePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );
            await using var targetStream = File.Open(
                targetFilePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None
            );

            var decoder = await BitmapDecoder.CreateAsync(sourceStream.AsRandomAccessStream());
            var transform = new BitmapTransform { Rotation = BitmapRotation.Clockwise90Degrees };
            var pixelData = await decoder.GetPixelDataAsync(
                decoder.BitmapPixelFormat,
                BitmapAlphaMode.Premultiplied,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage
            );

            var encoder = await BitmapEncoder.CreateAsync(
                GetEncoderId(extension),
                targetStream.AsRandomAccessStream()
            );
            encoder.SetPixelData(
                decoder.BitmapPixelFormat,
                BitmapAlphaMode.Premultiplied,
                decoder.PixelHeight,
                decoder.PixelWidth,
                decoder.DpiX,
                decoder.DpiY,
                pixelData.DetachPixelData()
            );

            await encoder.FlushAsync();
            return Model_Dao_Result_Factory.Success(targetFilePath);
        }
        catch (Exception ex)
        {
            if (string.IsNullOrWhiteSpace(targetFilePath) is false && File.Exists(targetFilePath))
            {
                File.Delete(targetFilePath);
            }

            return Model_Dao_Result_Factory.Failure<string>(
                $"Failed to rotate image: {ex.Message}",
                ex
            );
        }
    }

    public async Task<Model_Dao_Result<string>> ImportImageAsync(
        string sourceFilePath,
        string folderName
    )
    {
        try
        {
            return await ImportImageInternalAsync(
                sourceFilePath,
                folderName,
                preferredFileName: null
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<string>(
                $"Failed to import image: {ex.Message}",
                ex
            );
        }
    }

    public async Task<Model_Dao_Result<string>> ImportTypeImageAsync(
        string sourceFilePath,
        string typeName
    )
    {
        try
        {
            var sanitizedTypeName = SanitizeFileNameSegment(typeName);
            var preferredFileName = string.IsNullOrWhiteSpace(sanitizedTypeName)
                ? null
                : $"{TypeImagePrefix}-{sanitizedTypeName}";

            return await ImportImageInternalAsync(sourceFilePath, "Types", preferredFileName);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<string>(
                $"Failed to import type image: {ex.Message}",
                ex
            );
        }
    }

    public async Task<Model_Dao_Result<string>> ImportPartImageAsync(
        string sourceFilePath,
        string typeName,
        string partId
    )
    {
        try
        {
            var sanitizedTypeName = SanitizeFileNameSegment(typeName);
            var sanitizedPartId = SanitizeFileNameSegment(partId);
            var preferredFileName =
                string.IsNullOrWhiteSpace(sanitizedTypeName) ? sanitizedPartId
                : string.IsNullOrWhiteSpace(sanitizedPartId) ? sanitizedTypeName
                : $"{sanitizedTypeName}-{sanitizedPartId}";

            return await ImportImageInternalAsync(sourceFilePath, "Parts", preferredFileName);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<string>(
                $"Failed to import part image: {ex.Message}",
                ex
            );
        }
    }

    public Task<Model_Dao_Result> DeleteImageAsync(string? relativeImagePath)
    {
        try
        {
            var absolutePath = GetAbsolutePath(relativeImagePath);
            if (string.IsNullOrWhiteSpace(absolutePath) || File.Exists(absolutePath) is false)
            {
                return Task.FromResult(Model_Dao_Result_Factory.Success());
            }

            File.Delete(absolutePath);
            return Task.FromResult(Model_Dao_Result_Factory.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                Model_Dao_Result_Factory.Failure($"Failed to delete image: {ex.Message}", ex)
            );
        }
    }

    public string? GetAbsolutePath(string? relativeImagePath)
    {
        return Helper_DunnageImagePaths.GetAbsolutePath(relativeImagePath);
    }

    public string? GetNormalizedFullPath(string? imagePath)
    {
        var absolutePath = ResolveAbsoluteImagePath(imagePath);
        if (string.IsNullOrWhiteSpace(absolutePath))
        {
            return null;
        }

        return TryConvertMappedDriveToUnc(absolutePath, out var uncPath) ? uncPath : absolutePath;
    }

    private string? ResolveAbsoluteImagePath(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return null;
        }

        return Path.IsPathRooted(imagePath)
            ? imagePath
            : Helper_DunnageImagePaths.GetAbsolutePath(imagePath);
    }

    private async Task<Model_Dao_Result<string>> ImportImageInternalAsync(
        string sourceFilePath,
        string folderName,
        string? preferredFileName
    )
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            return Model_Dao_Result_Factory.Failure<string>("Image file path is required.");
        }

        if (File.Exists(sourceFilePath) is false)
        {
            return Model_Dao_Result_Factory.Failure<string>("Selected image file was not found.");
        }

        var extension = Path.GetExtension(sourceFilePath);
        if (IsSupportedExtension(extension) is false)
        {
            return Model_Dao_Result_Factory.Failure<string>(
                "Only PNG and JPG images are supported."
            );
        }

        await RefreshConfiguredRootFolderAsync();

        var safeFolderName = string.IsNullOrWhiteSpace(folderName) ? "Shared" : folderName.Trim();
        var targetDirectory = Path.Combine(Helper_DunnageImagePaths.RootFolder, safeFolderName);
        Directory.CreateDirectory(targetDirectory);

        var targetFileName = string.IsNullOrWhiteSpace(preferredFileName)
            ? $"{Guid.NewGuid():N}{extension}"
            : $"{preferredFileName}{extension}";
        var targetFilePath = Path.Combine(targetDirectory, targetFileName);

        if (ShouldRenameFromConfiguredRoot(sourceFilePath, preferredFileName))
        {
            var moveResult = MoveImageIntoManagedFolder(
                sourceFilePath,
                targetFilePath,
                safeFolderName,
                targetFileName
            );
            if (moveResult.IsSuccess)
            {
                return moveResult;
            }
        }

        await using var sourceStream = File.Open(
            sourceFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );
        await using var targetStream = File.Create(targetFilePath);
        await sourceStream.CopyToAsync(targetStream);

        var relativePath = Path.Combine(safeFolderName, targetFileName).Replace('\\', '/');
        return Model_Dao_Result_Factory.Success(relativePath);
    }

    private static Model_Dao_Result<string> MoveImageIntoManagedFolder(
        string sourceFilePath,
        string targetFilePath,
        string safeFolderName,
        string targetFileName
    )
    {
        var sourceFullPath = Path.GetFullPath(sourceFilePath);
        var targetFullPath = Path.GetFullPath(targetFilePath);

        if (string.Equals(sourceFullPath, targetFullPath, StringComparison.OrdinalIgnoreCase))
        {
            var unchangedRelativePath = Path.Combine(safeFolderName, targetFileName)
                .Replace('\\', '/');
            return Model_Dao_Result_Factory.Success(unchangedRelativePath);
        }

        File.Move(sourceFullPath, targetFullPath, true);
        var relativePath = Path.Combine(safeFolderName, targetFileName).Replace('\\', '/');
        return Model_Dao_Result_Factory.Success(relativePath);
    }

    private static bool ShouldRenameFromConfiguredRoot(
        string sourceFilePath,
        string? preferredFileName
    )
    {
        if (string.IsNullOrWhiteSpace(preferredFileName))
        {
            return false;
        }

        var sourceFullPath = Path.GetFullPath(sourceFilePath);
        var configuredRoot = Path.GetFullPath(Helper_DunnageImagePaths.RootFolder);
        var rootWithSeparator =
            configuredRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        return sourceFullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)
            || string.Equals(sourceFullPath, configuredRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static string SanitizeFileNameSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var invalidCharacters = Path.GetInvalidFileNameChars();
        var sanitizedCharacters = value
            .Trim()
            .Select(character => invalidCharacters.Contains(character) ? '-' : character)
            .ToArray();

        return new string(sanitizedCharacters).Trim().TrimEnd('.');
    }

    private static bool IsSupportedExtension(string? extension)
    {
        return string.IsNullOrWhiteSpace(extension) is false
            && SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    private static Guid GetEncoderId(string extension)
    {
        return string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase)
            ? BitmapEncoder.PngEncoderId
            : BitmapEncoder.JpegEncoderId;
    }

    private static bool TryConvertMappedDriveToUnc(string absolutePath, out string uncPath)
    {
        uncPath = absolutePath;

        if (absolutePath.StartsWith("\\\\", StringComparison.Ordinal))
        {
            return true;
        }

        var root = Path.GetPathRoot(absolutePath);
        if (string.IsNullOrWhiteSpace(root) || root.Length < 2 || root[1] != ':')
        {
            return false;
        }

        var drive = root[..2];
        var length = 512;
        var remoteName = new StringBuilder(length);
        var result = WNetGetConnection(drive, remoteName, ref length);
        if (result != 0 || remoteName.Length == 0)
        {
            return false;
        }

        var remainder = absolutePath[root.Length..].TrimStart(Path.DirectorySeparatorChar);
        uncPath = string.IsNullOrWhiteSpace(remainder)
            ? remoteName.ToString()
            : Path.Combine(remoteName.ToString(), remainder);
        return true;
    }
}
