using System;
using System.Runtime.InteropServices;
using MTM_Receiving_Application.Module_Scanner.Contracts;

namespace MTM_Receiving_Application.Module_Scanner.Services;

/// <summary>
/// RegisterHotKey + WM_HOTKEY implementation for the scanner feature.
/// The application window is subclassed (SetWindowSubclass) so WM_HOTKEY messages are
/// observed without replacing the framework window procedure. MOD_NOREPEAT is used so a
/// held chord raises a single notification instead of auto-repeat spam.
/// </summary>
public sealed class Service_ScannerHotkey : IService_ScannerHotkey
{
	private const int WmHotkey = 0x0312;

	private const uint ModAlt = 0x0001;
	private const uint ModControl = 0x0002;
	private const uint ModShift = 0x0004;
	private const uint ModWin = 0x0008;
	private const uint ModNoRepeat = 0x4000;

	private const int HotkeySendId = 0x4D54; // "MT"
	private const int HotkeyStopId = 0x4D55; // "MU"

	private static readonly SubclassProcDelegate _subclassProcedure = SubclassProc;
	private static Service_ScannerHotkey? _activeInstance;

	private IntPtr _hwnd = IntPtr.Zero;
	private bool _subclassInstalled;
	private bool _isRegistered;

	public event EventHandler? SendShortcutPressed;
	public event EventHandler? StopShortcutPressed;

	public bool IsRegistered => _isRegistered;

	public bool TryRegister(IntPtr hwnd, string sendChord, string stopChord)
	{
		Unregister();

		if (hwnd == IntPtr.Zero)
		{
			return false;
		}

		if (!TryParseChord(sendChord, out var sendModifiers, out var sendKey))
		{
			return false;
		}

		if (!TryParseChord(stopChord, out var stopModifiers, out var stopKey))
		{
			return false;
		}

		_hwnd = hwnd;

		if (
			!RegisterHotKey(hwnd, HotkeySendId, sendModifiers | ModNoRepeat, sendKey)
			|| !RegisterHotKey(hwnd, HotkeyStopId, stopModifiers | ModNoRepeat, stopKey)
		)
		{
			// Roll back any partial registration so callers never observe a half-registered state.
			UnregisterHotKey(hwnd, HotkeySendId);
			UnregisterHotKey(hwnd, HotkeyStopId);
			_hwnd = IntPtr.Zero;
			return false;
		}

		// WM_HOTKEY is only observable if the window is subclassed. Without the subclass the
		// default window procedure discards the message, so treat a subclass failure as a
		// registration failure and roll back.
		_subclassInstalled = SetWindowSubclass(
			hwnd,
			_subclassProcedure,
			new UIntPtr(1),
			UIntPtr.Zero
		);
		if (!_subclassInstalled)
		{
			UnregisterHotKey(hwnd, HotkeySendId);
			UnregisterHotKey(hwnd, HotkeyStopId);
			_hwnd = IntPtr.Zero;
			return false;
		}

		_activeInstance = this;
		_isRegistered = true;
		return true;
	}

	public void Unregister()
	{
		if (_subclassInstalled)
		{
			RemoveWindowSubclass(_hwnd, _subclassProcedure, new UIntPtr(1));
			_subclassInstalled = false;
		}

		if (_hwnd != IntPtr.Zero)
		{
			UnregisterHotKey(_hwnd, HotkeySendId);
			UnregisterHotKey(_hwnd, HotkeyStopId);
			_hwnd = IntPtr.Zero;
		}

		_isRegistered = false;
		if (ReferenceEquals(_activeInstance, this))
		{
			_activeInstance = null;
		}
	}

	/// <summary>
	/// Parses a chord such as "Ctrl+Alt+M" into Win32 modifier flags and a virtual-key code.
	/// Supported modifiers: Ctrl/Control, Alt, Shift, Win/Windows. Pure logic so it is unit-testable.
	/// </summary>
	/// <param name="chord">Chord text, for example "Ctrl+Alt+M".</param>
	/// <param name="modifiers">When true is returned, the bitwise MOD_* flags.</param>
	/// <param name="virtualKey">When true is returned, the Win32 virtual-key code.</param>
	/// <returns>True when the chord is a valid combination that includes at least one modifier.</returns>
	public static bool TryParseChord(string chord, out uint modifiers, out ushort virtualKey)
	{
		modifiers = 0;
		virtualKey = 0;

		if (string.IsNullOrWhiteSpace(chord))
		{
			return false;
		}

		var parts = chord.Split(
			'+',
			StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
		);
		if (parts.Length == 0)
		{
			return false;
		}

		for (var index = 0; index < parts.Length - 1; index++)
		{
			switch (parts[index].ToLowerInvariant())
			{
				case "ctrl":
				case "control":
					modifiers |= ModControl;
					break;
				case "alt":
					modifiers |= ModAlt;
					break;
				case "shift":
					modifiers |= ModShift;
					break;
				case "win":
				case "windows":
					modifiers |= ModWin;
					break;
				default:
					return false;
			}
		}

		var key = parts[^1].ToUpperInvariant();
		if (TryMapKeyName(key, out virtualKey) && modifiers != 0)
		{
			return true;
		}

		virtualKey = 0;
		modifiers = 0;
		return false;
	}

	private static bool TryMapKeyName(string key, out ushort virtualKey)
	{
		switch (key)
		{
			case "A": virtualKey = 0x41; return true;
			case "B": virtualKey = 0x42; return true;
			case "C": virtualKey = 0x43; return true;
			case "D": virtualKey = 0x44; return true;
			case "E": virtualKey = 0x45; return true;
			case "F": virtualKey = 0x46; return true;
			case "G": virtualKey = 0x47; return true;
			case "H": virtualKey = 0x48; return true;
			case "I": virtualKey = 0x49; return true;
			case "J": virtualKey = 0x4A; return true;
			case "K": virtualKey = 0x4B; return true;
			case "L": virtualKey = 0x4C; return true;
			case "M": virtualKey = 0x4D; return true;
			case "N": virtualKey = 0x4E; return true;
			case "O": virtualKey = 0x4F; return true;
			case "P": virtualKey = 0x50; return true;
			case "Q": virtualKey = 0x51; return true;
			case "R": virtualKey = 0x52; return true;
			case "S": virtualKey = 0x53; return true;
			case "T": virtualKey = 0x54; return true;
			case "U": virtualKey = 0x55; return true;
			case "V": virtualKey = 0x56; return true;
			case "W": virtualKey = 0x57; return true;
			case "X": virtualKey = 0x58; return true;
			case "Y": virtualKey = 0x59; return true;
			case "Z": virtualKey = 0x5A; return true;
			case "F1": virtualKey = 0x70; return true;
			case "F2": virtualKey = 0x71; return true;
			case "F3": virtualKey = 0x72; return true;
			case "F4": virtualKey = 0x73; return true;
			case "F5": virtualKey = 0x74; return true;
			case "F6": virtualKey = 0x75; return true;
			case "F7": virtualKey = 0x76; return true;
			case "F8": virtualKey = 0x77; return true;
			case "F9": virtualKey = 0x78; return true;
			case "F10": virtualKey = 0x79; return true;
			case "F11": virtualKey = 0x7A; return true;
			case "F12": virtualKey = 0x7B; return true;
			case "TAB": virtualKey = 0x09; return true;
			case "ENTER": virtualKey = 0x0D; return true;
			case "ESC": virtualKey = 0x1B; return true;
			case "SPACE": virtualKey = 0x20; return true;
			default:
				virtualKey = 0;
				return false;
		}
	}

	private static IntPtr SubclassProc(
		IntPtr hWnd,
		uint uMsg,
		IntPtr wParam,
		IntPtr lParam,
		UIntPtr uIdSubclass,
		UIntPtr dwRefData
	)
	{
		if (uMsg == WmHotkey)
		{
			var instance = _activeInstance;
			if (instance is not null)
			{
				var id = wParam.ToInt32();
				if (id == HotkeySendId)
				{
					instance.SendShortcutPressed?.Invoke(instance, EventArgs.Empty);
				}
				else if (id == HotkeyStopId)
				{
					instance.StopShortcutPressed?.Invoke(instance, EventArgs.Empty);
				}
			}
		}

		return DefSubclassProc(hWnd, uMsg, wParam, lParam);
	}

	// ── Win32 interop ───────────────────────────────────────────────────────────

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

	[DllImport("comctl32.dll", SetLastError = true)]
	private static extern bool SetWindowSubclass(
		IntPtr hWnd,
		SubclassProcDelegate pfnSubclass,
		UIntPtr uIdSubclass,
		UIntPtr dwRefData
	);

	[DllImport("comctl32.dll", SetLastError = true)]
	private static extern bool RemoveWindowSubclass(
		IntPtr hWnd,
		SubclassProcDelegate pfnSubclass,
		UIntPtr uIdSubclass
	);

	[DllImport("comctl32.dll")]
	private static extern IntPtr DefSubclassProc(
		IntPtr hWnd,
		uint uMsg,
		IntPtr wParam,
		IntPtr lParam
	);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	private delegate IntPtr SubclassProcDelegate(
		IntPtr hWnd,
		uint uMsg,
		IntPtr wParam,
		IntPtr lParam,
		UIntPtr uIdSubclass,
		UIntPtr dwRefData
	);
}
