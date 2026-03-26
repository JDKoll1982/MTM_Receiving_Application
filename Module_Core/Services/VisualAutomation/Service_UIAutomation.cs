using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using WinForms = System.Windows.Forms;

namespace MTM_Receiving_Application.Module_Core.Services.VisualAutomation;

/// <summary>
/// Generic UI Automation service backed by <see cref="System.Windows.Automation"/> and
/// Win32 user32.dll. Entirely window-agnostic — all Visual-specific logic lives in
/// <c>Service_VisualInventoryAutomation</c>.
/// </summary>
public class Service_UIAutomation : IService_UIAutomation
{
    private readonly IService_ApplicationShutdown _applicationShutdown;

    public Service_UIAutomation(IService_ApplicationShutdown applicationShutdown)
    {
        _applicationShutdown = applicationShutdown;
    }

    // ── Win32 P/Invoke ────────────────────────────────────────────────────────

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindowEx(
        IntPtr hwndParent,
        IntPtr hwndChildAfter,
        string? lpszClass,
        string? lpszWindow
    );

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    // ── Window discovery ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<AutomationElement?> FindWindowAsync(
        string windowTitle,
        TimeSpan timeout,
        CancellationToken ct = default
    )
    {
        using var linkedCts = CreateLinkedTokenSource(ct);
        var linkedToken = linkedCts.Token;
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            linkedToken.ThrowIfCancellationRequested();

            var element = AutomationElement.RootElement.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, windowTitle)
            );

            if (element is not null)
            {
                return element;
            }

            await Task.Delay(100, linkedToken).ConfigureAwait(false);
        }

        return null;
    }

    /// <inheritdoc/>
    public IntPtr FindWindowByClassAndTitle(string className, string windowTitle) =>
        FindWindow(className, windowTitle);

    /// <inheritdoc/>
    public bool WindowExists(string className, string windowTitle) =>
        FindWindow(className, windowTitle) != IntPtr.Zero;

    // ── Popup lifecycle ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IntPtr> WaitForPopupAsync(
        string className,
        string windowTitle,
        TimeSpan settleTimeout,
        int pollMs = 100,
        CancellationToken ct = default
    )
    {
        using var linkedCts = CreateLinkedTokenSource(ct);
        var linkedToken = linkedCts.Token;
        var deadline = DateTime.UtcNow + settleTimeout;
        while (DateTime.UtcNow < deadline)
        {
            linkedToken.ThrowIfCancellationRequested();

            var hwnd = FindWindow(className, windowTitle);
            if (hwnd != IntPtr.Zero)
            {
                return hwnd;
            }

            await Task.Delay(pollMs, linkedToken).ConfigureAwait(false);
        }

        return IntPtr.Zero;
    }

    /// <inheritdoc/>
    public async Task WaitForWindowToCloseAsync(
        string className,
        string windowTitle,
        TimeSpan timeout,
        int pollMs = 100,
        CancellationToken ct = default
    )
    {
        using var linkedCts = CreateLinkedTokenSource(ct);
        var linkedToken = linkedCts.Token;
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            linkedToken.ThrowIfCancellationRequested();

            if (!WindowExists(className, windowTitle))
            {
                return;
            }

            await Task.Delay(pollMs, linkedToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DismissPopupIfPresentAsync(
        string className,
        string windowTitle,
        string keySequence,
        TimeSpan timeout,
        CancellationToken ct = default
    )
    {
        using var linkedCts = CreateLinkedTokenSource(ct);
        var linkedToken = linkedCts.Token;
        var hwnd = FindWindow(className, windowTitle);
        if (hwnd == IntPtr.Zero)
        {
            return false;
        }

        if (!SetForegroundVerified(hwnd))
        {
            return false;
        }

        WinForms.SendKeys.Send(keySequence);

        await WaitForWindowToCloseAsync(className, windowTitle, timeout, ct: linkedToken)
            .ConfigureAwait(false);
        return true;
    }

    // ── Field filling ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task FillFieldAsync(
        AutomationElement window,
        string automationId,
        string value,
        bool sendTab = false,
        CancellationToken ct = default
    )
    {
        using var linkedCts = CreateLinkedTokenSource(ct);
        var linkedToken = linkedCts.Token;
        linkedToken.ThrowIfCancellationRequested();

        var element = FindElementByAutomationId(window, automationId);

        if (!element.Current.IsEnabled || !element.Current.IsKeyboardFocusable)
        {
            throw new InvalidOperationException(
                $"Field '{automationId}' is not enabled or keyboard-focusable."
            );
        }

        var hwnd = new IntPtr(element.Current.NativeWindowHandle);
        SetForegroundVerified(hwnd);

        await Task.Delay(50, linkedToken).ConfigureAwait(false);

        WinForms.SendKeys.Send(value);

        if (sendTab)
        {
            WinForms.SendKeys.Send("{TAB}");
        }
    }

    /// <inheritdoc/>
    public async Task ClearAndFillFieldAsync(
        AutomationElement window,
        string automationId,
        string value,
        CancellationToken ct = default
    )
    {
        using var linkedCts = CreateLinkedTokenSource(ct);
        var linkedToken = linkedCts.Token;
        linkedToken.ThrowIfCancellationRequested();

        var element = FindElementByAutomationId(window, automationId);

        if (!element.Current.IsEnabled || !element.Current.IsKeyboardFocusable)
        {
            throw new InvalidOperationException(
                $"Field '{automationId}' is not enabled or keyboard-focusable."
            );
        }

        var hwnd = new IntPtr(element.Current.NativeWindowHandle);
        SetForegroundVerified(hwnd);

        await Task.Delay(50, linkedToken).ConfigureAwait(false);

        // Select all → delete existing content, then type the new value
        WinForms.SendKeys.Send("^a");
        WinForms.SendKeys.Send("{DEL}");
        WinForms.SendKeys.Send(value);
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    /// <remarks>
    /// ⚠️ PF-08: Never pass credentials through this method — use Process.StartInfo for launch args.
    /// SendKeys is only for non-sensitive field values and navigation keystrokes.
    /// </remarks>
    public void SendKeys(string keys) => WinForms.SendKeys.Send(keys);

    /// <inheritdoc/>
    public bool SetForegroundVerified(IntPtr hwnd)
    {
        SetForegroundWindow(hwnd);
        return GetForegroundWindow() == hwnd;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static AutomationElement FindElementByAutomationId(
        AutomationElement window,
        string automationId
    )
    {
        var element = window.FindFirst(
            TreeScope.Descendants,
            new PropertyCondition(AutomationElement.AutomationIdProperty, automationId)
        );

        if (element is null)
        {
            throw new InvalidOperationException(
                $"No UI element with AutomationId '{automationId}' found in the target window."
            );
        }

        return element;
    }

    private CancellationTokenSource CreateLinkedTokenSource(CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(
                _applicationShutdown.ShutdownToken
            );
        }

        return CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _applicationShutdown.ShutdownToken
        );
    }
}
