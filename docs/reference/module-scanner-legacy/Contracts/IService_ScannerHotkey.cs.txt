using System;

namespace MTM_Receiving_Application.Module_Scanner.Contracts;

/// <summary>
/// Global scanner hotkey service.
/// Registers the operator send shortcut against the application window using the
/// Win32 RegisterHotKey model and raises an event when the WM_HOTKEY message arrives.
/// Registration is isolated from business execution logic so testing and alternate
/// invocation paths remain straightforward.
/// </summary>
public interface IService_ScannerHotkey
{
	/// <summary>Raised when the operator send shortcut (Ctrl+Alt+M by default) is pressed.</summary>
	event EventHandler? SendShortcutPressed;

	/// <summary>True while the shortcut is registered against a live window.</summary>
	bool IsRegistered { get; }

	/// <summary>
	/// Registers the send shortcut against the given window handle. Any previously registered
	/// shortcut is unregistered first. Returns false (and leaves nothing registered) if the
	/// chord cannot be parsed or RegisterHotKey fails, so callers can surface guidance.
	/// </summary>
	/// <param name="hwnd">Window handle that should receive WM_HOTKEY messages.</param>
	/// <param name="sendChord">Send chord, for example "Ctrl+Alt+M".</param>
	bool TryRegister(IntPtr hwnd, string sendChord);

	/// <summary>Unregisters the shortcut and removes the window subclass. Safe to call multiple times.</summary>
	void Unregister();
}
