using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using WinRT.Interop;

namespace MTM_Receiving_Application.Module_Shared.Views;

/// <summary>
/// Dedicated help window for the shared math quantity textbox. It explains, in short categorized
/// sections, what the box accepts, how the arithmetic and rounding work, and which keys do what.
/// </summary>
/// <remarks>
/// Shown as a modal window: the owning window is disabled while this page is open, so the
/// operator cannot keep editing quantities behind the instructions, and it is re-enabled as soon
/// as the help closes.
/// </remarks>
public sealed partial class View_Shared_MathQuantityHelpWindow : Window
{
    /// <summary>Win32 index used to give the help window an owner.</summary>
    private const int OwnerWindowIndex = -8;

    private readonly Window? _owner;
    private readonly IntPtr _ownerHandle;
    private readonly TaskCompletionSource _closed = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );

    public View_Shared_MathQuantityHelpWindow(Window? owner = null)
    {
        InitializeComponent();

        _owner = owner;

        this.ApplySharedIcon();
        this.SetWindowSize(620, 720);
        this.SetFixedSize();
        this.CenterOnScreen();

        if (owner is not null)
        {
            _ownerHandle = WindowNative.GetWindowHandle(owner);
            if (_ownerHandle != IntPtr.Zero)
            {
                // Own the window so it stays in front of the app, then make the app wait for it.
                SetWindowLongPtr(WindowNative.GetWindowHandle(this), OwnerWindowIndex, _ownerHandle);
                EnableWindow(_ownerHandle, false);
            }
        }

        Closed += OnClosed;
    }

    /// <summary>
    /// Opens the help window and completes when the operator closes it.
    /// </summary>
    /// <param name="owner">Window that should wait for the help to close.</param>
    public static async Task ShowAsync(Window? owner)
    {
        var window = new View_Shared_MathQuantityHelpWindow(owner);
        window.Activate();
        await window._closed.Task;
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        Closed -= OnClosed;

        if (_ownerHandle != IntPtr.Zero)
        {
            EnableWindow(_ownerHandle, true);
            _owner?.Activate();
        }

        _closed.TrySetResult();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnEscapeInvoked(
        KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args
    )
    {
        args.Handled = true;
        Close();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnableWindow(IntPtr windowHandle, bool enable);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(
        IntPtr windowHandle,
        int index,
        IntPtr newValue
    );
}
