using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using Windows.Graphics.Imaging;

namespace MTM_Receiving_Application.Module_Dunnage.Services;

/// <summary>
/// Stores Dunnage PNG files in local app data and exposes relative-path persistence.
/// </summary>
public class Service_DunnageImageStorage : IService_DunnageImageStorage
{
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

            if (
                string.Equals(
                    Path.GetExtension(absolutePath),
                    ".png",
                    StringComparison.OrdinalIgnoreCase
                )
                is false
            )
            {
                return Model_Dao_Result_Factory.Failure<string>("Only PNG images are supported.");
            }

            var tempDirectory = Path.Combine(Helper_DunnageImagePaths.RootFolder, "Temp");
            Directory.CreateDirectory(tempDirectory);

            targetFilePath = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}.png");

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
                BitmapEncoder.PngEncoderId,
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
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                return Model_Dao_Result_Factory.Failure<string>("Image file path is required.");
            }

            if (File.Exists(sourceFilePath) is false)
            {
                return Model_Dao_Result_Factory.Failure<string>(
                    "Selected image file was not found."
                );
            }

            var extension = Path.GetExtension(sourceFilePath);
            if (string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase) is false)
            {
                return Model_Dao_Result_Factory.Failure<string>("Only PNG images are supported.");
            }

            var safeFolderName = string.IsNullOrWhiteSpace(folderName)
                ? "Shared"
                : folderName.Trim();
            var targetDirectory = Path.Combine(Helper_DunnageImagePaths.RootFolder, safeFolderName);
            Directory.CreateDirectory(targetDirectory);

            var targetFileName = $"{Guid.NewGuid():N}{extension}";
            var targetFilePath = Path.Combine(targetDirectory, targetFileName);

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
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<string>(
                $"Failed to import image: {ex.Message}",
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
}
