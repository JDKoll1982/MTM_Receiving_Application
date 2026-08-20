using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using MTM_Receiving_Application.Module_Scanner.Contracts;

namespace MTM_Receiving_Application.Module_Scanner.Services;

/// <summary>
/// Win32 SendInput-based input engine for the scanner feature.
/// Constructs ordered INPUT arrays and injects them serially, per the platform guarantee
/// documented for SendInput. Text is injected with KEYEVENTF_UNICODE (VK_PACKET) so field
/// values are layout-independent; navigation keys use virtual-key down/up pairs.
/// </summary>
public sealed class Service_ScannerInputEngine : IService_ScannerInputEngine
{
	private const uint InputKeyboard = 0x0001;

	private const uint KeyEventfKeyUp = 0x0002;
	private const uint KeyEventfUnicode = 0x0004;
	private const uint KeyEventfExtendedKey = 0x0001;

	private const uint ModAlt = 0x0001;
	private const uint ModControl = 0x0002;
	private const uint ModShift = 0x0004;

	public bool SendText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return true;
		}

		// Build one ordered array of down/up Unicode events for the whole value so the
		// injected events are inserted serially with nothing intermingled.
		var inputs = new INPUT[text.Length * 2];
		for (var index = 0; index < text.Length; index++)
		{
			inputs[index * 2] = BuildUnicodeKeyInput(text[index], keyUp: false);
			inputs[(index * 2) + 1] = BuildUnicodeKeyInput(text[index], keyUp: true);
		}

		return InjectInputs(inputs) == inputs.Length;
	}

	public bool SendKeyPress(ushort virtualKey, bool extended = false)
	{
		var inputs = new[]
		{
			BuildVirtualKeyInput(virtualKey, extended, keyUp: false),
			BuildVirtualKeyInput(virtualKey, extended, keyUp: true),
		};

		return InjectInputs(inputs) == inputs.Length;
	}

	public bool SendChord(uint modifiers, ushort virtualKey)
	{
		var pressed = new System.Collections.Generic.List<ushort>();
		if ((modifiers & ModAlt) != 0)
		{
			pressed.Add(0x12); // VK_MENU
		}

		if ((modifiers & ModControl) != 0)
		{
			pressed.Add(0x11); // VK_CONTROL
		}

		if ((modifiers & ModShift) != 0)
		{
			pressed.Add(0x10); // VK_SHIFT
		}

		var inputs = new INPUT[(pressed.Count * 2) + 2];
		var position = 0;
		foreach (var key in pressed)
		{
			inputs[position++] = BuildVirtualKeyInput(key, extended: false, keyUp: false);
		}

		inputs[position++] = BuildVirtualKeyInput(virtualKey, extended: false, keyUp: false);
		inputs[position++] = BuildVirtualKeyInput(virtualKey, extended: false, keyUp: true);

		for (var index = pressed.Count - 1; index >= 0; index--)
		{
			inputs[position++] = BuildVirtualKeyInput(pressed[index], extended: false, keyUp: true);
		}

		return InjectInputs(inputs) == inputs.Length;
	}

	public bool IsKeyPressed(ushort virtualKey)
	{
		// GetAsyncKeyState returns a short whose high bit indicates the key is down.
		return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
	}

	public bool TryGetForegroundWindow(out IntPtr hwnd)
	{
		hwnd = GetForegroundWindow();
		return hwnd != IntPtr.Zero;
	}

	public bool TryGetWindowProcessName(IntPtr hwnd, out string processName)
	{
		processName = string.Empty;
		if (hwnd == IntPtr.Zero || !GetWindowThreadProcessId(hwnd, out var processId))
		{
			return false;
		}

		try
		{
			var process = Process.GetProcessById((int)processId);
			processName = process.ProcessName;
			return !string.IsNullOrWhiteSpace(processName);
		}
		catch (ArgumentException)
		{
			// Process already exited.
			return false;
		}
	}

	public bool TryGetWindowTitle(IntPtr hwnd, out string windowTitle)
	{
		windowTitle = string.Empty;
		if (hwnd == IntPtr.Zero)
		{
			return false;
		}

		var buffer = new StringBuilder(512);
		var length = GetWindowText(hwnd, buffer, buffer.Capacity);
		if (length <= 0)
		{
			return false;
		}

		windowTitle = buffer.ToString();
		return true;
	}

	public bool TryFindWindow(string? className, string? windowTitle, out IntPtr hwnd)
	{
		hwnd = FindWindow(
			string.IsNullOrWhiteSpace(className) ? null : className,
			string.IsNullOrWhiteSpace(windowTitle) ? null : windowTitle
		);
		return hwnd != IntPtr.Zero;
	}

	public bool TryFindChildWindow(
		IntPtr parentHwnd,
		string? childClassName,
		string? childTitle,
		out IntPtr hwnd
	)
	{
		hwnd = FindWindowEx(
			parentHwnd,
			IntPtr.Zero,
			string.IsNullOrWhiteSpace(childClassName) ? null : childClassName,
			string.IsNullOrWhiteSpace(childTitle) ? null : childTitle
		);
		return hwnd != IntPtr.Zero;
	}

	public bool TrySetForeground(IntPtr hwnd)
	{
		if (hwnd == IntPtr.Zero)
		{
			return false;
		}

		_ = SetForegroundWindow(hwnd);

		// Verify ownership so we never inject into the wrong window after a failed activation.
		return GetForegroundWindow() == hwnd;
	}

	private static INPUT BuildUnicodeKeyInput(char character, bool keyUp)
	{
		return new INPUT
		{
			type = InputKeyboard,
			U = new InputUnion
			{
				ki = new KEYBDINPUT
				{
					wVk = 0,
					wScan = character,
					dwFlags = KeyEventfUnicode | (keyUp ? KeyEventfKeyUp : 0),
					time = 0,
					dwExtraInfo = IntPtr.Zero,
				},
			},
		};
	}

	private static INPUT BuildVirtualKeyInput(ushort virtualKey, bool extended, bool keyUp)
	{
		return new INPUT
		{
			type = InputKeyboard,
			U = new InputUnion
			{
				ki = new KEYBDINPUT
				{
					wVk = virtualKey,
					wScan = 0,
					dwFlags = (extended ? KeyEventfExtendedKey : 0) | (keyUp ? KeyEventfKeyUp : 0),
					time = 0,
					dwExtraInfo = IntPtr.Zero,
				},
			},
		};
	}

	private static uint InjectInputs(INPUT[] inputs)
	{
		if (inputs.Length == 0)
		{
			return 0;
		}

		return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
	}

	// ── Win32 interop ───────────────────────────────────────────────────────────

	[DllImport("user32.dll", SetLastError = true)]
	private static extern uint SendInput(uint cInputs, INPUT[] pInputs, int cbSize);

	[DllImport("user32.dll")]
	private static extern short GetAsyncKeyState(int vKey);

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern IntPtr FindWindowEx(
		IntPtr hWndParent,
		IntPtr hWndChildAfter,
		string? lpszClass,
		string? lpszWindow
	);

	[DllImport("user32.dll")]
	private static extern bool SetForegroundWindow(IntPtr hWnd);

	[DllImport("kernel32.dll")]
	private static extern uint GetCurrentProcessId();

	[StructLayout(LayoutKind.Sequential)]
	private struct INPUT
	{
		public uint type;
		public InputUnion U;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct InputUnion
	{
		[FieldOffset(0)]
		public MOUSEINPUT mi;

		[FieldOffset(0)]
		public KEYBDINPUT ki;

		[FieldOffset(0)]
		public HARDWAREINPUT hi;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct KEYBDINPUT
	{
		public ushort wVk;
		public ushort wScan;
		public uint dwFlags;
		public uint time;
		public IntPtr dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct MOUSEINPUT
	{
		public int dx;
		public int dy;
		public uint mouseData;
		public uint dwFlags;
		public uint time;
		public IntPtr dwExtraInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct HARDWAREINPUT
	{
		public uint uMsg;
		public ushort wParamL;
		public ushort wParamH;
	}
}
