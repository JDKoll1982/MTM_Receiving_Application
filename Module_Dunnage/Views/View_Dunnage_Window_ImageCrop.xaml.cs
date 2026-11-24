using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using Windows.Graphics.Imaging;
using WinRT.Interop;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

/// <summary>
/// Lets the user choose the part of an image to keep. The cropped result is written to a temp
/// working copy so the original photo is never modified.
/// </summary>
/// <remarks>
/// This is a Window rather than a ContentDialog because every caller is already an open
/// ContentDialog, and WinUI allows only one open ContentDialog per XamlRoot.
/// </remarks>
public sealed partial class View_Dunnage_Window_ImageCrop : Window
{
    private const int WindowWidth = 900;
    private const int WindowHeight = 720;

    private const int GWLP_HWNDPARENT = -8;

    private readonly IService_DunnageImageStorage _imageStorage;
    private readonly string _sourceImagePath;
    private readonly UIElement? _ownerElement;
    private readonly TaskCompletionSource<bool> _contentLoadedSource = new();
    private readonly TaskCompletionSource<string?> _resultSource = new();

    private bool _isSaving;
    private string? _croppedImagePath;

    public View_Dunnage_Window_ImageCrop(string sourceImagePath, FrameworkElement? owner)
    {
        _sourceImagePath = sourceImagePath;
        _ownerElement = owner as UIElement;
        _imageStorage = App.GetService<IService_DunnageImageStorage>();

        InitializeComponent();
        ApplyOwnerTheme();

        this.ApplySharedIcon();
        this.SetWindowSize(WindowWidth, WindowHeight);
        this.SetFixedSize();
        this.CenterOnScreen();

        RootGrid.Loaded += OnRootGridLoaded;
        Closed += OnWindowClosed;
    }

    /// <summary>
    /// Shows the crop window for the supplied image and returns the temp working copy path of the
    /// cropped result, or null when the user cancels or the image cannot be loaded.
    /// </summary>
    public static async Task<string?> ShowAsync(string? imagePath, FrameworkElement? owner)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return null;
        }

        View_Dunnage_Window_ImageCrop window;
        try
        {
            window = new View_Dunnage_Window_ImageCrop(imagePath, owner);
        }
        catch
        {
            return null;
        }

        return await window.ShowWindowAsync();
    }

    private async Task<string?> ShowWindowAsync()
    {
        if (_ownerElement is not null)
        {
            _ownerElement.IsHitTestVisible = false;
        }

        try
        {
            Activate();
            KeepAboveOwnerWindow();

            if (await _contentLoadedSource.Task is false)
            {
                return null;
            }

            await LoadSourceImageAsync();
        }
        catch (Exception ex)
        {
            ShowStatus($"Unable to open the crop window: {ex.Message}");
            Close();
        }

        return await _resultSource.Task;
    }

    private async Task LoadSourceImageAsync()
    {
        try
        {
            var absolutePath = ResolveAbsoluteImagePath(_sourceImagePath);
            if (absolutePath is null || File.Exists(absolutePath) is false)
            {
                ShowStatus("The selected image file was not found.");
                ApplyButton.IsEnabled = false;
                return;
            }

            CropperControl.Source = await CreateFullSizeBitmapAsync(absolutePath);
        }
        catch (Exception ex)
        {
            ShowStatus($"Unable to load the image: {ex.Message}");
            ApplyButton.IsEnabled = false;
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

    private async void OnApplyClick(object sender, RoutedEventArgs e)
    {
        if (_isSaving)
        {
            return;
        }

        if (CropperControl.Source is null)
        {
            ShowStatus("Choose an image before cropping.");
            return;
        }

        _isSaving = true;
        ApplyButton.IsEnabled = false;

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
                TryDeleteFile(targetPath);
                ShowStatus("The crop area is too small to save.");
                return;
            }

            _croppedImagePath = targetPath;
            Close();
        }
        catch (Exception ex)
        {
            ShowStatus($"Unable to crop the image: {ex.Message}");
        }
        finally
        {
            _isSaving = false;
            ApplyButton.IsEnabled = true;
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close();
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

    private void OnRootGridLoaded(object sender, RoutedEventArgs e)
    {
        RootGrid.Loaded -= OnRootGridLoaded;
        _contentLoadedSource.TrySetResult(true);
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        RootGrid.Loaded -= OnRootGridLoaded;
        _contentLoadedSource.TrySetResult(false);

        if (_ownerElement is not null)
        {
            _ownerElement.IsHitTestVisible = true;
        }

        _resultSource.TrySetResult(_croppedImagePath);
    }

    private void ApplyOwnerTheme()
    {
        if (App.MainWindow?.Content is FrameworkElement mainContent)
        {
            RootGrid.RequestedTheme = mainContent.RequestedTheme;
        }
    }

    /// <summary>
    /// Makes this window an owned window of the main window so it stays above the dialog that
    /// opened it instead of dropping behind the parent window.
    /// </summary>
    private void KeepAboveOwnerWindow()
    {
        if (App.MainWindow is null)
        {
            return;
        }

        var ownerHandle = WindowNative.GetWindowHandle(App.MainWindow);
        var windowHandle = WindowNative.GetWindowHandle(this);
        SetWindowLongPtr(windowHandle, GWLP_HWNDPARENT, ownerHandle);
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

    private static IntPtr SetWindowLongPtr(IntPtr windowHandle, int index, IntPtr value)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(windowHandle, index, value)
            : SetWindowLong32(windowHandle, index, value.ToInt32());
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(
        IntPtr windowHandle,
        int index,
        IntPtr value
    );

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern IntPtr SetWindowLong32(IntPtr windowHandle, int index, int value);
}
