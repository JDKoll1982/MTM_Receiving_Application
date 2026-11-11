using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace MTM_Receiving_Application.Module_VisualAutomation.Helpers;

/// <summary>
/// Raw Win32 mouse and keyboard injection used to drive the Infor VISUAL window.
/// </summary>
/// <remarks>
/// The VISUAL field controls are Gupta panes that expose no UI Automation patterns and are not
/// keyboard focusable, so the only way to change a field is to click it and type - which is what a
/// person, and what a macro, effectively does.
/// </remarks>
public static class Helper_VisualAutomation_NativeInput
{
    private const uint InputMouse = 0;
    private const uint InputKeyboard = 1;

    private const uint KeyEventKeyUp = 0x0002;
    private const uint KeyEventUnicode = 0x0004;

    private const uint MouseEventLeftDown = 0x0002;
    private const uint MouseEventLeftUp = 0x0004;

    private const ushort VkControl = 0x11;
    private const ushort VkA = 0x41;
    private const ushort VkDelete = 0x2E;
    private const ushort VkTab = 0x09;

    /// <summary>Gets a value indicating whether the process is allowed to inject input.</summary>
    public static bool IsSupported => OperatingSystem.IsWindows();

    /// <summary>
    /// Moves the cursor to the given screen point and clicks the left button once.
    /// </summary>
    /// <param name="x">Screen x in physical pixels.</param>
    /// <param name="y">Screen y in physical pixels.</param>
    public static void ClickLeft(int x, int y)
    {
        SetCursorPos(x, y);
        Thread.Sleep(120);

        var inputs = new[]
        {
            CreateMouseInput(MouseEventLeftDown),
            CreateMouseInput(MouseEventLeftUp),
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
        Thread.Sleep(120);
    }

    /// <summary>
    /// Sends Ctrl+A to select the contents of the focused control.
    /// </summary>
    public static void SelectAll()
    {
        SendKey(VkControl, keyUp: false);
        SendKey(VkA, keyUp: false);
        SendKey(VkA, keyUp: true);
        SendKey(VkControl, keyUp: true);
        Thread.Sleep(80);
    }

    /// <summary>
    /// Sends Delete to clear the selection.
    /// </summary>
    public static void PressDelete()
    {
        SendKey(VkDelete, keyUp: false);
        SendKey(VkDelete, keyUp: true);
        Thread.Sleep(80);
    }

    /// <summary>
    /// Sends Tab to commit the field and move focus away from it.
    /// </summary>
    public static void PressTab()
    {
        SendKey(VkTab, keyUp: false);
        SendKey(VkTab, keyUp: true);
        Thread.Sleep(120);
    }

    /// <summary>
    /// Types text into the focused control using Unicode key events.
    /// </summary>
    /// <param name="text">Text to type.</param>
    public static void TypeText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var inputs = new List<Input>(text.Length * 2);
        foreach (var character in text)
        {
            inputs.Add(UnicodeKey(character, keyUp: false));
            inputs.Add(UnicodeKey(character, keyUp: true));
        }

        SendInput((uint)inputs.Count, [.. inputs], Marshal.SizeOf<Input>());
        Thread.Sleep(120);
    }

    /// <summary>
    /// Brings a window to the foreground.
    /// </summary>
    /// <param name="windowHandle">Handle of the window to activate.</param>
    /// <returns>True when the window is the foreground window afterwards.</returns>
    public static bool ActivateWindow(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            return false;
        }

        if (GetForegroundWindow() == windowHandle)
        {
            return true;
        }

        ShowWindow(windowHandle, 9);
        BringWindowToTop(windowHandle);
        SetForegroundWindow(windowHandle);
        Thread.Sleep(350);

        return GetForegroundWindow() == windowHandle;
    }

    /// <summary>
    /// Returns the handle of the window that currently has input focus.
    /// </summary>
    /// <returns>The foreground window handle.</returns>
    public static IntPtr GetForegroundWindowHandle() => GetForegroundWindow();

    private static void SendKey(ushort virtualKey, bool keyUp)
    {
        var input = new Input
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    VirtualKey = virtualKey,
                    ScanCode = 0,
                    Flags = keyUp ? KeyEventKeyUp : 0,
                    Time = 0,
                    ExtraInfo = IntPtr.Zero,
                },
            },
        };

        SendInput(1, [input], Marshal.SizeOf<Input>());
        Thread.Sleep(40);
    }

    private static Input UnicodeKey(char character, bool keyUp) =>
        new()
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    VirtualKey = 0,
                    ScanCode = character,
                    Flags = KeyEventUnicode | (keyUp ? KeyEventKeyUp : 0),
                    Time = 0,
                    ExtraInfo = IntPtr.Zero,
                },
            },
        };

    private static Input CreateMouseInput(uint flags) =>
        new()
        {
            Type = InputMouse,
            Data = new InputUnion
            {
                Mouse = new MouseInput
                {
                    Dx = 0,
                    Dy = 0,
                    MouseData = 0,
                    Flags = flags,
                    Time = 0,
                    ExtraInfo = IntPtr.Zero,
                },
            },
        };

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MouseInput Mouse;

        [FieldOffset(0)]
        public KeyboardInput Keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, Input[] inputs, int size);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr windowHandle);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr windowHandle);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr windowHandle, int command);
}
