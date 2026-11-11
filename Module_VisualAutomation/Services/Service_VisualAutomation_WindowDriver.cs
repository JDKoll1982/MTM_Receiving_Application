using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_VisualAutomation.Contracts;
using MTM_Receiving_Application.Module_VisualAutomation.Helpers;
using MTM_Receiving_Application.Module_VisualAutomation.Models;

namespace MTM_Receiving_Application.Module_VisualAutomation.Services;

/// <summary>
/// Drives the live Infor VISUAL Purchase Receipt Entry window with UI Automation reads and injected
/// mouse/keyboard writes, mirroring what a VISUAL macro would do to the same fields.
/// </summary>
public sealed class Service_VisualAutomation_WindowDriver : IService_VisualAutomation_WindowDriver
{
    /// <summary>Process name of the Purchase Receipt Entry module.</summary>
    public const string ModuleProcessName = "VMRCVENT";

    /// <summary>Text that marks the expected window title.</summary>
    public const string ExpectedTitleFragment = "Purchase Receipt";

    // A value control sits within this many pixels of its label's vertical centre.
    private const double LabelMatchTolerance = 8.0;

    // Value controls are taller than the static labels; labels are about 14px high.
    private const int MinimumValueHeight = 18;

    // Edit boxes are wide; this filters out the small lookup buttons between label and field.
    private const int MinimumValueWidth = 40;

    private readonly IService_LoggingUtility _logger;

    public Service_VisualAutomation_WindowDriver(IService_LoggingUtility logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Model_VisualWindowInfo DiscoverWindow()
    {
        Process? moduleProcess;

        try
        {
            moduleProcess = Process
                .GetProcessesByName(ModuleProcessName)
                .OrderByDescending(process => process.MainWindowHandle != IntPtr.Zero)
                .FirstOrDefault();
        }
        catch (Exception ex)
        {
            return new Model_VisualWindowInfo
            {
                IsRunning = false,
                Diagnostic = $"Could not enumerate {ModuleProcessName} processes: {ex.Message}",
            };
        }

        if (moduleProcess is null || moduleProcess.MainWindowHandle == IntPtr.Zero)
        {
            return new Model_VisualWindowInfo
            {
                IsRunning = false,
                Diagnostic =
                    $"No {ModuleProcessName}.EXE window is open. Open Purchase Receipt Entry in Infor VISUAL first.",
            };
        }

        var handle = moduleProcess.MainWindowHandle;
        var executablePath = string.Empty;
        string? diagnostic = null;

        try
        {
            executablePath = moduleProcess.MainModule?.FileName ?? string.Empty;
        }
        catch (Exception ex)
        {
            diagnostic =
                $"The window was found but its executable path could not be read ({ex.GetType().Name}). "
                + "Run both applications at the same elevation level.";
        }

        var elementCount = 0;

        try
        {
            elementCount = ReadElements(handle).Count;
        }
        catch (Exception ex)
        {
            diagnostic = $"The window was found but UI Automation could not read it: {ex.Message}";
        }

        var rect = ReadWindowRect(handle);

        return new Model_VisualWindowInfo
        {
            IsRunning = true,
            ProcessId = moduleProcess.Id,
            WindowHandle = handle,
            WindowTitle = moduleProcess.MainWindowTitle,
            ExecutablePath = executablePath,
            WindowRect = rect,
            IsForeground = Helper_VisualAutomation_NativeInput.GetForegroundWindowHandle() == handle,
            ElementCount = elementCount,
            Diagnostic = diagnostic,
        };
    }

    /// <inheritdoc />
    public Model_VisualFieldSnapshot CaptureSnapshot()
    {
        var window = DiscoverWindow();
        var fields = new Dictionary<string, Model_VisualFieldValue>(StringComparer.Ordinal);

        if (!window.IsRunning || window.WindowHandle == IntPtr.Zero)
        {
            return new Model_VisualFieldSnapshot { Window = window };
        }

        try
        {
            var elements = ReadElements(window.WindowHandle);
            var resolved = ResolveValueControls(elements);

            foreach (var variable in Helper_VisualAutomation_FieldMap.VisibleVariables)
            {
                if (resolved.TryGetValue(variable, out var control))
                {
                    fields[variable] = new Model_VisualFieldValue
                    {
                        Key = variable,
                        Label = control.Label,
                        Value = control.Value,
                        HasControl = true,
                        IsWritable = Helper_VisualAutomation_FieldMap.IsWritable(variable),
                        Rect = control.Rect,
                        ElementIndex = control.Index,
                    };
                }
                else
                {
                    fields[variable] = new Model_VisualFieldValue
                    {
                        Key = variable,
                        Label = Helper_VisualAutomation_FieldMap.LabelFor(variable) ?? string.Empty,
                        HasControl = false,
                        IsWritable = false,
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"Reading the VISUAL receipt window failed: {ex.Message}",
                nameof(Service_VisualAutomation_WindowDriver)
            );
        }

        return new Model_VisualFieldSnapshot
        {
            Window = window,
            Fields = fields,
            SaveControlPresent = HasSaveControl(),
        };
    }

    /// <inheritdoc />
    public Model_Dao_Result<string> ReadValue(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Model_Dao_Result_Factory.Failure<string>("A macro variable name is required.");
        }

        var snapshot = CaptureSnapshot();
        if (!snapshot.Fields.TryGetValue(key, out var field) || !field.HasControl)
        {
            return Model_Dao_Result_Factory.Failure<string>(
                $"Variable '{key}' has no control on the visible Purchase Receipt Entry window."
            );
        }

        return Model_Dao_Result_Factory.Success(field.Value);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_VisualWriteOutcome>> WriteValueAsync(
        string key,
        string value,
        bool restoreOriginal,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return Model_Dao_Result_Factory.Failure<Model_VisualWriteOutcome>(
                "A macro variable name is required."
            );
        }

        if (!Helper_VisualAutomation_FieldMap.IsWritable(key))
        {
            return Model_Dao_Result_Factory.Failure<Model_VisualWriteOutcome>(
                $"Variable '{key}' is not in the writable field list. Only free-text header fields may be typed into."
            );
        }

        var window = DiscoverWindow();
        if (!window.IsRunning || window.WindowHandle == IntPtr.Zero)
        {
            return Model_Dao_Result_Factory.Failure<Model_VisualWriteOutcome>(
                window.Diagnostic ?? "The Purchase Receipt Entry window is not available."
            );
        }

        var snapshot = CaptureSnapshot();
        if (!snapshot.Fields.TryGetValue(key, out var field) || !field.HasControl || !field.Rect.HasArea)
        {
            return Model_Dao_Result_Factory.Failure<Model_VisualWriteOutcome>(
                $"Variable '{key}' has no control on the visible window."
            );
        }

        if (!Helper_VisualAutomation_NativeInput.ActivateWindow(window.WindowHandle))
        {
            return Model_Dao_Result_Factory.Failure<Model_VisualWriteOutcome>(
                "The Purchase Receipt Entry window could not be activated, so no keystrokes were sent. "
                    + "Click the window once and run the probe again."
            );
        }

        if (Helper_VisualAutomation_NativeInput.GetForegroundWindowHandle() != window.WindowHandle)
        {
            return Model_Dao_Result_Factory.Failure<Model_VisualWriteOutcome>(
                "Another window took focus before typing. No keystrokes were sent."
            );
        }

        var original = field.Value;

        Helper_VisualAutomation_NativeInput.ClickLeft(field.Rect.CenterX, field.Rect.CenterY);
        Helper_VisualAutomation_NativeInput.SelectAll();

        if (value.Length == 0)
        {
            Helper_VisualAutomation_NativeInput.PressDelete();
        }
        else
        {
            Helper_VisualAutomation_NativeInput.TypeText(value);
        }

        Helper_VisualAutomation_NativeInput.PressTab();
        await Task.Delay(320, cancellationToken).ConfigureAwait(false);

        var observed = ReadValue(key).Data ?? string.Empty;
        var writeVerified = string.Equals(observed.Trim(), value.Trim(), StringComparison.Ordinal);

        var restored = false;
        var afterRestore = observed;

        if (restoreOriginal)
        {
            Helper_VisualAutomation_NativeInput.ActivateWindow(window.WindowHandle);
            Helper_VisualAutomation_NativeInput.ClickLeft(field.Rect.CenterX, field.Rect.CenterY);
            Helper_VisualAutomation_NativeInput.SelectAll();

            if (original.Length == 0)
            {
                Helper_VisualAutomation_NativeInput.PressDelete();
            }
            else
            {
                Helper_VisualAutomation_NativeInput.TypeText(original);
            }

            Helper_VisualAutomation_NativeInput.PressTab();
            await Task.Delay(320, cancellationToken).ConfigureAwait(false);

            afterRestore = ReadValue(key).Data ?? string.Empty;
            restored = string.Equals(afterRestore.Trim(), original.Trim(), StringComparison.Ordinal);
        }

        _logger.LogInfo(
            $"Typed '{value}' into VISUAL field {key} (was '{original}'); read back '{observed}', "
                + $"restore {(restoreOriginal ? (restored ? "verified" : "FAILED") : "not requested")}.",
            nameof(Service_VisualAutomation_WindowDriver)
        );

        return Model_Dao_Result_Factory.Success(
            new Model_VisualWriteOutcome
            {
                Key = key,
                OriginalValue = original,
                RequestedValue = value,
                ObservedValue = observed,
                WriteVerified = writeVerified,
                RestoreVerified = restored,
                ValueAfterRestore = afterRestore,
            }
        );
    }

    /// <inheritdoc />
    public bool HasSaveControl()
    {
        var window = DiscoverWindow();
        if (!window.IsRunning || window.WindowHandle == IntPtr.Zero)
        {
            return false;
        }

        try
        {
            return ReadElements(window.WindowHandle)
                .Any(element =>
                {
                    var name = element.Name ?? string.Empty;
                    return name.Equals("Save", StringComparison.OrdinalIgnoreCase)
                        || name.Equals("Save transaction", StringComparison.OrdinalIgnoreCase);
                });
        }
        catch
        {
            return false;
        }
    }

    private static List<Model_VisualElement> ReadElements(IntPtr windowHandle)
    {
        var root = AutomationElement.FromHandle(windowHandle);
        var found = root.FindAll(TreeScope.Descendants, Condition.TrueCondition);
        var elements = new List<Model_VisualElement>(found.Count);

        var index = 0;
        foreach (AutomationElement element in found)
        {
            AutomationElement.AutomationElementInformation current;
            try
            {
                current = element.Current;
            }
            catch (ElementNotAvailableException)
            {
                continue;
            }

            var rect = current.BoundingRectangle;
            elements.Add(
                new Model_VisualElement
                {
                    Index = index++,
                    Name = current.Name ?? string.Empty,
                    AutomationId = current.AutomationId ?? string.Empty,
                    ControlTypeName = current.ControlType?.ProgrammaticName ?? string.Empty,
                    Rect = new Model_VisualRect(
                        (int)rect.X,
                        (int)rect.Y,
                        (int)rect.Width,
                        (int)rect.Height
                    ),
                }
            );
        }

        return elements;
    }

    /// <summary>
    /// Pairs every mapped label with the value control to its right, on the same row.
    /// </summary>
    /// <param name="elements">Elements read from the window in tree order.</param>
    /// <returns>Resolved controls keyed by macro variable name.</returns>
    private static Dictionary<string, Model_VisualResolvedField> ResolveValueControls(
        IReadOnlyList<Model_VisualElement> elements
    )
    {
        var resolved = new Dictionary<string, Model_VisualResolvedField>(StringComparer.Ordinal);

        foreach (var label in elements)
        {
            if (string.IsNullOrWhiteSpace(label.Name))
            {
                continue;
            }

            var key = Helper_VisualAutomation_FieldMap.KeyForLabel(label.Name);
            if (key is null || resolved.ContainsKey(key))
            {
                continue;
            }

            var labelCenter = label.Rect.CenterYExact;

            var candidate = elements
                .Where(element =>
                    element.Rect.HasArea
                    && element.Rect.Height >= MinimumValueHeight
                    && element.Rect.Width >= MinimumValueWidth
                    && element.Rect.X >= label.Rect.Right - 2
                    && Math.Abs(element.Rect.CenterYExact - labelCenter) <= LabelMatchTolerance
                )
                .OrderBy(element =>
                    Math.Abs(element.Rect.CenterYExact - labelCenter)
                )
                .ThenBy(element => element.Rect.X)
                .FirstOrDefault();

            if (candidate is null)
            {
                continue;
            }

            resolved[key] = new Model_VisualResolvedField
            {
                Label = label.Name,
                Value = candidate.Name,
                Rect = candidate.Rect,
                Index = candidate.Index,
            };
        }

        return resolved;
    }

    private static Model_VisualRect ReadWindowRect(IntPtr handle)
    {
        if (!GetWindowRect(handle, out var rect))
        {
            return Model_VisualRect.Empty;
        }

        return new Model_VisualRect(
            rect.Left,
            rect.Top,
            rect.Right - rect.Left,
            rect.Bottom - rect.Top
        );
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr handle, out NativeRect rect);

    private sealed class Model_VisualElement
    {
        public int Index { get; init; }
        public string Name { get; init; } = string.Empty;
        public string AutomationId { get; init; } = string.Empty;
        public string ControlTypeName { get; init; } = string.Empty;
        public Model_VisualRect Rect { get; init; } = Model_VisualRect.Empty;
    }

    private sealed class Model_VisualResolvedField
    {
        public string Label { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public Model_VisualRect Rect { get; init; } = Model_VisualRect.Empty;
        public int Index { get; init; }
    }
}
