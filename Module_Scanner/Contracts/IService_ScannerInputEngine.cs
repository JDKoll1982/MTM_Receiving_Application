using System;

namespace MTM_Receiving_Application.Module_Scanner.Contracts;

/// <summary>
/// Scanner-native input emission service.
/// Encapsulates Win32 SendInput injection and foreground-window verification for the
/// scanner feature. All methods are synchronous because SendInput is a synchronous Win32
/// call; pacing between fields is orchestrated by <c>IService_ScannerExecution</c>.
/// </summary>
public interface IService_ScannerInputEngine
{
	/// <summary>
	/// Injects the given text as an ordered array of Unicode key events via SendInput.
	/// Returns true only if every event was inserted.
	/// </summary>
	bool SendText(string text);

	/// <summary>
	/// Injects a single virtual-key press (key down followed by key up).
	/// </summary>
	bool SendKeyPress(ushort virtualKey, bool extended = false);

	/// <summary>
	/// Injects a modifier chord (for example Ctrl+Alt+M).
	/// </summary>
	bool SendChord(uint modifiers, ushort virtualKey);

	/// <summary>
	/// Returns true if the given virtual key is currently held down (via GetAsyncKeyState).
	/// </summary>
	bool IsKeyPressed(ushort virtualKey);

	/// <summary>Gets the currently focused top-level window handle, or IntPtr.Zero.</summary>
	bool TryGetForegroundWindow(out IntPtr hwnd);

	/// <summary>Gets the process name (without .exe) that owns the given window handle.</summary>
	bool TryGetWindowProcessName(IntPtr hwnd, out string processName);

	/// <summary>Gets the window title text for the given window handle.</summary>
	bool TryGetWindowTitle(IntPtr hwnd, out string windowTitle);

	/// <summary>
	/// Finds a top-level window by optional class name and title.
	/// </summary>
	bool TryFindWindow(string? className, string? windowTitle, out IntPtr hwnd);

	/// <summary>
	/// Finds a child window (by optional class and title) below the given parent handle.
	/// </summary>
	bool TryFindChildWindow(IntPtr parentHwnd, string? childClassName, string? childTitle, out IntPtr hwnd);

	/// <summary>
	/// Brings the given window to the foreground and verifies success via GetForegroundWindow.
	/// </summary>
	bool TrySetForeground(IntPtr hwnd);
}
