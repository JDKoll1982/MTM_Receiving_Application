using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

/// <summary>
/// Generic, window-agnostic UI Automation service.
/// Provides window discovery, popup lifecycle management, field filling, and focus control.
/// Visual-specific logic (fill sequences, AutomationIds, popup titles) belongs in
/// <c>Service_VisualInventoryAutomation</c>, not here.
/// </summary>
public interface IService_UIAutomation
{
    // ── Window discovery ──────────────────────────────────────────────────────

    /// <summary>Returns the AutomationElement for a top-level window by title, or null if not found within timeout.</summary>
    /// <param name="windowTitle">Title of the window to search for.</param>
    /// <param name="timeout">Maximum time to wait for the window to appear.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task<AutomationElement?> FindWindowAsync(string windowTitle, TimeSpan timeout, CancellationToken ct = default);

    /// <summary>Returns the hwnd for a top-level window matching both class name and window title, or IntPtr.Zero.</summary>
    /// <param name="className">Window class name to match.</param>
    /// <param name="windowTitle">Window title to match.</param>
    IntPtr FindWindowByClassAndTitle(string className, string windowTitle);

    /// <summary>Returns true if a window matching both class name and title currently exists.</summary>
    /// <param name="className">Window class name to match.</param>
    /// <param name="windowTitle">Window title to match.</param>
    bool WindowExists(string className, string windowTitle);

    /// <summary>
    /// Polls for a popup (matched by class + title) to appear within settleTimeout.
    /// Uses Task.Delay(pollMs) between checks — never spins.
    /// Returns the hwnd if the popup appeared, IntPtr.Zero if it did not appear within the timeout.
    /// Use this for every popup detection point instead of a one-shot WindowExists check,
    /// because Visual can be delayed by hundreds of milliseconds to over a second on loaded networks.
    /// </summary>
    /// <param name="className">Window class name to poll for.</param>
    /// <param name="windowTitle">Window title to poll for.</param>
    /// <param name="settleTimeout">Maximum time to wait for the popup to appear.</param>
    /// <param name="pollMs">Polling interval in milliseconds.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task<IntPtr> WaitForPopupAsync(string className, string windowTitle, TimeSpan settleTimeout, int pollMs = 100, CancellationToken ct = default);

    // ── Popup lifecycle ───────────────────────────────────────────────────────

    /// <summary>
    /// Polls until the popup (matched by class + title) disappears or the timeout elapses.
    /// Uses Task.Delay(pollMs) between checks — never spins. Throws OperationCanceledException on ct.
    /// </summary>
    /// <param name="className">Window class name to monitor.</param>
    /// <param name="windowTitle">Window title to monitor.</param>
    /// <param name="timeout">Maximum time to wait for the window to close.</param>
    /// <param name="pollMs">Polling interval in milliseconds.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task WaitForWindowToCloseAsync(string className, string windowTitle, TimeSpan timeout, int pollMs = 100, CancellationToken ct = default);

    /// <summary>
    /// If a popup matching className/windowTitle is present, brings it to foreground (verified
    /// via GetForegroundWindow), sends the provided key sequence, then waits for it to close.
    /// Returns true if the popup was found and dismissed, false if it was not present.
    /// </summary>
    /// <param name="className">Window class name of the popup.</param>
    /// <param name="windowTitle">Window title of the popup.</param>
    /// <param name="keySequence">Key sequence to send to dismiss the popup.</param>
    /// <param name="timeout">Maximum time to wait for the popup to close.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task<bool> DismissPopupIfPresentAsync(string className, string windowTitle, string keySequence, TimeSpan timeout, CancellationToken ct = default);

    // ── Field filling ─────────────────────────────────────────────────────────

    /// <summary>Fills a text field identified by AutomationId; optionally sends Tab after.</summary>
    /// <param name="window">The parent automation element (window).</param>
    /// <param name="automationId">AutomationId of the field to fill.</param>
    /// <param name="value">Value to type into the field.</param>
    /// <param name="sendTab">If true, sends Tab after filling.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task FillFieldAsync(AutomationElement window, string automationId, string value, bool sendTab = false, CancellationToken ct = default);

    /// <summary>Clears existing content then types the new value.</summary>
    /// <param name="window">The parent automation element (window).</param>
    /// <param name="automationId">AutomationId of the field to fill.</param>
    /// <param name="value">Value to type into the field.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task ClearAndFillFieldAsync(AutomationElement window, string automationId, string value, CancellationToken ct = default);

    // ── Input ─────────────────────────────────────────────────────────────────

    /// <summary>Sends a key sequence to the currently focused window.</summary>
    /// <param name="keys">The key sequence to send.</param>
    void SendKeys(string keys);

    /// <summary>
    /// Brings a window handle to the foreground and verifies success via GetForegroundWindow.
    /// Returns false if focus could not be acquired (e.g., another process holds the lock).
    /// </summary>
    /// <param name="hwnd">Window handle to bring to foreground.</param>
    bool SetForegroundVerified(IntPtr hwnd);
}
