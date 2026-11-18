using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using Windows.Graphics.Imaging;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

/// <summary>
/// Lets the user choose the part of an image to keep. The cropped result is written to a temp
/// working copy so the original photo is never modified.
/// </summary>
public sealed partial class View_Dunnage_Dialog_ImageCrop : ContentDialog
{
    private readonly IService_DunnageImageStorage _imageStorage;
    private readonly string _sourceImagePath;

    /// <summary>
    /// Gets the temp working copy path holding the cropped image after a successful apply.
    /// </summary>
    public string? CroppedImagePath { get; private set; }

    public View_Dunnage_Dialog_ImageCrop(string sourceImagePath)
    {
        _sourceImagePath = sourceImagePath;
        _imageStorage = App.GetService<IService_DunnageImageStorage>();

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
    }

    /// <summary>
    /// Shows the crop dialog for the supplied image and returns the temp working copy path of the
    /// cropped result, or null when the user cancels or the image cannot be loaded.
    /// </summary>
    public static async Task<string?> ShowAsync(string? imagePath, XamlRoot? xamlRoot)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || xamlRoot is null)
        {
            return null;
        }

        var dialog = new View_Dunnage_Dialog_ImageCrop(imagePath) { XamlRoot = xamlRoot };

        if (await dialog.LoadSourceImageAsync() is false)
        {
            dialog.IsPrimaryButtonEnabled = false;
        }

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? dialog.CroppedImagePath : null;
    }

    /// <summary>
    /// Loads the configured image into the cropper at its full pixel size.
    /// </summary>
    public async Task<bool> LoadSourceImageAsync()
    {
        try
        {
            var absolutePath = ResolveAbsoluteImagePath(_sourceImagePath);
            if (absolutePath is null || File.Exists(absolutePath) is false)
            {
                ShowStatus("The selected image file was not found.");
                return false;
            }

            CropperControl.Source = await CreateFullSizeBitmapAsync(absolutePath);
            return true;
        }
        catch (Exception ex)
        {
            ShowStatus($"Unable to load the image: {ex.Message}");
            return false;
        }
    }

    private static async Task<WriteableBitmap> CreateFullSizeBitmapAsync(string absolutePath)
    {
        await using var sourceStream = File.Open(
            absolutePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite
        );
        using var randomAccessStream = sourceStream.AsRandomAccessStream();

        var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
        var pixelData = await decoder.GetPixelDataAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            new BitmapTransform(),
            ExifOrientationMode.RespectExifOrientation,
            ColorManagementMode.DoNotColorManage
        );

        var bitmap = new WriteableBitmap(
            (int)decoder.OrientedPixelWidth,
            (int)decoder.OrientedPixelHeight
        );

        await using var pixelStream = bitmap.PixelBuffer.AsStream();
        await pixelStream.WriteAsync(pixelData.DetachPixelData());
        await pixelStream.FlushAsync();

        return bitmap;
    }

    private async void OnApplyCropClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (CropperControl.Source is null)
        {
            args.Cancel = true;
            ShowStatus("Choose an image before cropping.");
            return;
        }

        var deferral = args.GetDeferral();

        try
        {
            var targetPath = _imageStorage.CreateTempWorkingCopyPath(_sourceImagePath);
            var fileFormat = ResolveFileFormat(targetPath);

            await using (var targetStream = File.Open(
                targetPath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None
            ))
            using (var randomAccessStream = targetStream.AsRandomAccessStream())
            {
                await CropperControl.SaveAsync(
                    randomAccessStream,
                    fileFormat,
                    keepRectangularOutput: true
                );
                await randomAccessStream.FlushAsync();
            }

            if (new FileInfo(targetPath).Length == 0)
            {
                args.Cancel = true;
                TryDeleteFile(targetPath);
                ShowStatus("The crop area is too small to save.");
                return;
            }

            CroppedImagePath = targetPath;
        }
        catch (Exception ex)
        {
            args.Cancel = true;
            ShowStatus($"Unable to crop the image: {ex.Message}");
        }
        finally
        {
            deferral.Complete();
        }
    }

    private void OnAspectRatioChanged(object sender, RoutedEventArgs e)
    {
        // Checked also fires while InitializeComponent assigns the default, before the other
        // radio buttons exist. The default selection needs no work, so leave it alone.
        if (CropperControl is null || SquareAspectRadioButton is null)
        {
            return;
        }

        double? aspectRatio =
            SquareAspectRadioButton.IsChecked == true ? 1d
            : LandscapeAspectRadioButton.IsChecked == true ? 4d / 3d
            : WideAspectRadioButton.IsChecked == true ? 16d / 9d
            : null;

        CropperControl.AspectRatio = aspectRatio;
    }

    private void OnResetCropClick(object sender, RoutedEventArgs e)
    {
        CropperControl.Reset();
        ShowStatus(null);
    }

    private static BitmapFileFormat ResolveFileFormat(string imagePath)
    {
        var extension = Path.GetExtension(imagePath);

        return string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase)
            || string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase)
            ? BitmapFileFormat.Jpeg
            : BitmapFileFormat.Png;
    }

    private string? ResolveAbsoluteImagePath(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return null;
        }

        return Path.IsPathRooted(imagePath) ? imagePath : _imageStorage.GetAbsolutePath(imagePath);
    }

    private void ShowStatus(string? message)
    {
        CropStatusTextBlock.Text = message ?? string.Empty;
        CropStatusTextBlock.Visibility = string.IsNullOrWhiteSpace(message)
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
            // A leftover temp file is harmless; the temp folder is excluded from image sync.
        }
    }
}
