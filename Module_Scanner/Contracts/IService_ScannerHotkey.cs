using System;

namespace MTM_Receiving_Application.Module_Scanner.Contracts;

/// <summary>
/// Global scanner hotkey service.
/// Registers the operator send/stop shortcuts against the application window using the
/// Win32 RegisterHotKey model and raises events when WM_HOTKEY messages arrive.
/// Registration is isolated from business execution logic so testing and alternate
/// invocation paths remain straightforward.
/// </summary>
public interface IService_ScannerHotkey
{
	/// <summary>Raised when the operator send shortcut (Ctrl+Alt+M by default) is pressed.</summary>
	event EventHandler? SendShortcutPressed;

	/// <summary>Raised when the operator stop shortcut (Ctrl+Alt+N by default) is pressed.</summary>
	event EventHandler? StopShortcutPressed;

	/// <summary>True while the shortcuts are registered against a live window.</summary>
	bool IsRegistered { get; }

	/// <summary>
	/// Registers both shortcuts against the given window handle. Any previously registered
	/// shortcuts are unregistered first. Returns false (and leaves nothing registered) if the
	/// chords cannot be parsed or RegisterHotKey fails, so callers can surface guidance.
	/// </summary>
	/// <param name="hwnd">Window handle that should receive WM_HOTKEY messages.</param>
	/// <param name="sendChord">Send chord, for example "Ctrl+Alt+M".</param>
	/// <param name="stopChord">Stop chord, for example "Ctrl+Alt+N".</param>
	bool TryRegister(IntPtr hwnd, string sendChord, string stopChord);

	/// <summary>Unregisters both shortcuts and removes the window subclass. Safe to call multiple times.</summary>
	void Unregister();
}
