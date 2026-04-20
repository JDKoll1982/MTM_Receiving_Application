using System;
using System.IO;

namespace MTM_Receiving_Application.Module_Dunnage.Helpers;

/// <summary>
/// Provides a desktop file dialog for choosing Dunnage image files from the configured root folder.
/// </summary>
public static class Helper_DunnageImageFileDialog
{
    public static string? ChooseImageFile(IntPtr ownerHwnd, string? initialDirectory)
    {
        using var dialog = new System.Windows.Forms.OpenFileDialog
        {
            Title = "Choose Image",
            Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false,
            RestoreDirectory = false,
        };

        if (
            string.IsNullOrWhiteSpace(initialDirectory) is false
            && Directory.Exists(initialDirectory)
        )
        {
            dialog.InitialDirectory = initialDirectory;
        }

        var owner = ownerHwnd == IntPtr.Zero ? null : new Win32Window(ownerHwnd);
        var result = owner is null ? dialog.ShowDialog() : dialog.ShowDialog(owner);

        return result == System.Windows.Forms.DialogResult.OK ? dialog.FileName : null;
    }

    private sealed class Win32Window : System.Windows.Forms.IWin32Window
    {
        public Win32Window(IntPtr handle)
        {
            Handle = handle;
        }

        public IntPtr Handle { get; }
    }
}
