using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_VisualAutomation.Models;

/// <summary>
/// Snapshot of the running Infor VISUAL Purchase Receipt Entry window.
/// </summary>
public sealed class Model_VisualWindowInfo
{
    /// <summary>Gets a value indicating whether a VMRCVENT window was found.</summary>
    public bool IsRunning { get; init; }

    /// <summary>Gets the process id of the module, or 0 when it was not found.</summary>
    public int ProcessId { get; init; }

    /// <summary>Gets the window handle used as the root for UI Automation.</summary>
    public IntPtr WindowHandle { get; init; }

    /// <summary>Gets the main window title.</summary>
    public string WindowTitle { get; init; } = string.Empty;

    /// <summary>Gets the full path of VMRCVENT.EXE as reported by Windows.</summary>
    public string ExecutablePath { get; init; } = string.Empty;

    /// <summary>Gets the screen rectangle the window occupies, in physical pixels.</summary>
    public Model_VisualRect WindowRect { get; init; } = Model_VisualRect.Empty;

    /// <summary>Gets a value indicating whether the window is currently the foreground window.</summary>
    public bool IsForeground { get; init; }

    /// <summary>Gets the number of automation elements the window exposed.</summary>
    public int ElementCount { get; init; }

    /// <summary>Gets a diagnostic message describing why discovery failed, when applicable.</summary>
    public string? Diagnostic { get; init; }

    /// <summary>Gets the moment the snapshot was taken.</summary>
    public DateTimeOffset CapturedAt { get; init; } = DateTimeOffset.Now;
}

/// <summary>
/// Simple pixel rectangle used for automation targeting.
/// </summary>
public readonly record struct Model_VisualRect(int X, int Y, int Width, int Height)
{
    /// <summary>Gets an empty rectangle.</summary>
    public static Model_VisualRect Empty => new(0, 0, 0, 0);

    /// <summary>Gets a value indicating whether the rectangle has a usable size.</summary>
    public bool HasArea => Width > 0 && Height > 0;

    /// <summary>Gets the horizontal centre of the rectangle.</summary>
    public int CenterX => X + (Width / 2);

    /// <summary>Gets the vertical centre of the rectangle.</summary>
    public int CenterY => Y + (Height / 2);

    /// <summary>Gets the vertical centre as a double, for tolerance comparisons.</summary>
    public double CenterYExact => Y + (Height / 2.0);

    /// <summary>Gets the right edge of the rectangle.</summary>
    public int Right => X + Width;

    /// <summary>Renders the rectangle for logs.</summary>
    /// <returns>A compact description.</returns>
    public override string ToString() => $"({X},{Y}) {Width}x{Height}";
}

/// <summary>
/// One Infor VISUAL macro variable resolved against the live window.
/// </summary>
public sealed class Model_VisualFieldValue
{
    /// <summary>Gets the macro variable name, for example <c>ORDER_ID</c>.</summary>
    public required string Key { get; init; }

    /// <summary>Gets the on-screen label this variable was matched to, when one exists.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Gets the value currently shown on screen.</summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether a value control was located for this variable.</summary>
    public bool HasControl { get; init; }

    /// <summary>Gets a value indicating whether the harness is allowed to type into this control.</summary>
    public bool IsWritable { get; init; }

    /// <summary>Gets the screen rectangle of the value control.</summary>
    public Model_VisualRect Rect { get; init; } = Model_VisualRect.Empty;

    /// <summary>Gets the element index the value control was read from, for diagnostics.</summary>
    public int ElementIndex { get; init; } = -1;
}

/// <summary>
/// Full read of the window: every macro variable the harness could resolve, plus the window itself.
/// </summary>
public sealed class Model_VisualFieldSnapshot
{
    /// <summary>Gets the window snapshot this field set was read from.</summary>
    public required Model_VisualWindowInfo Window { get; init; }

    /// <summary>Gets the resolved variables, keyed by macro variable name.</summary>
    public IReadOnlyDictionary<string, Model_VisualFieldValue> Fields { get; init; } =
        new Dictionary<string, Model_VisualFieldValue>(StringComparer.Ordinal);

    /// <summary>Gets a value indicating whether a toolbar Save control was found in the window.</summary>
    public bool SaveControlPresent { get; init; }

    /// <summary>Gets the moment the snapshot was taken.</summary>
    public DateTimeOffset CapturedAt { get; init; } = DateTimeOffset.Now;

    /// <summary>
    /// Returns the value of a variable, or an empty string when it was not resolved.
    /// </summary>
    /// <param name="key">Macro variable name.</param>
    /// <returns>The value, or an empty string.</returns>
    public string ValueOf(string key) =>
        Fields.TryGetValue(key, out var field) ? field.Value : string.Empty;
}
