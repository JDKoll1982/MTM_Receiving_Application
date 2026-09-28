using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Scanner.Contracts;

/// <summary>
/// Scanner-native input emission service.
/// Encapsulates Win32 SendInput injection and foreground-window verification for the
/// scanner feature. Scanner- and Visual-specific behavior lives here rather than on the
/// generic <c>IService_UIAutomation</c> contract in Module_Core.
/// All methods are synchronous because SendInput is a synchronous Win32 call; pacing
/// between fields is orchestrated by <c>IService_ScannerExecution</c>.
/// </summary>
public interface IService_ScannerInputEngine
{
	/// <summary>
	/// Injects the given text as an ordered array of Unicode key events via SendInput.
	/// Returns true only if every event was inserted (false when blocked by UIPI or another thread).
	/// </summary>
	/// <param name="text">Text to type. Each character becomes a VK_PACKET down/up pair.</param>
	bool SendText(string text);

	/// <summary>
	/// Injects a single virtual-key press (key down followed by key up).
	/// </summary>
	/// <param name="virtualKey">Win32 virtual-key code (for example 0x09 for Tab).</param>
	/// <param name="extended">True for extended keys (Insert, Delete, arrow keys, and so on).</param>
	bool SendKeyPress(ushort virtualKey, bool extended = false);

	/// <summary>
	/// Injects a modifier chord (for example Ctrl+Alt+M). Modifiers are pressed in order,
	/// the key is pressed and released, then the modifiers are released in reverse order.
	/// </summary>
	/// <param name="modifiers">Bitwise combination of MOD_* flags.</param>
	/// <param name="virtualKey">Win32 virtual-key code of the main key.</param>
	bool SendChord(uint modifiers, ushort virtualKey);

	/// <summary>
	/// Returns true if the given virtual key is currently held down (via GetAsyncKeyState).
	/// Used to detect interference from keys the operator is already pressing.
	/// </summary>
	/// <param name="virtualKey">Win32 virtual-key code to test.</param>
	bool IsKeyPressed(ushort virtualKey);

	/// <summary>Gets the currently focused top-level window handle, or IntPtr.Zero.</summary>
	bool TryGetForegroundWindow(out IntPtr hwnd);

	/// <summary>Gets the process name (without .exe) that owns the given window handle.</summary>
	bool TryGetWindowProcessName(IntPtr hwnd, out string processName);

	/// <summary>Gets the window title text for the given window handle.</summary>
	bool TryGetWindowTitle(IntPtr hwnd, out string windowTitle);

	/// <summary>
	/// Finds a top-level window by optional class name and title. Returns true if a
	/// matching window was found.
	/// </summary>
	/// <param name="className">Optional window class name (null/empty to skip).</param>
	/// <param name="windowTitle">Optional window title (null/empty to skip).</param>
	/// <param name="hwnd">The first matching window handle, or IntPtr.Zero.</param>
	bool TryFindWindow(string? className, string? windowTitle, out IntPtr hwnd);

	/// <summary>
	/// Finds a child window (by optional class and title) below the given parent handle.
	/// Used to resolve the Visual Inventory Transfers child window.
	/// </summary>
	bool TryFindChildWindow(IntPtr parentHwnd, string? childClassName, string? childTitle, out IntPtr hwnd);

	/// <summary>
	/// Brings the given window to the foreground and verifies success via GetForegroundWindow.
	/// </summary>
	/// <param name="hwnd">Window handle to activate.</param>
	bool TrySetForeground(IntPtr hwnd);
}
