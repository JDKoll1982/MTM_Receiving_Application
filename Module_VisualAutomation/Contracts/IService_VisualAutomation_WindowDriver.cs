using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_VisualAutomation.Models;

namespace MTM_Receiving_Application.Module_VisualAutomation.Contracts;

/// <summary>
/// Reads and drives the live Infor VISUAL Purchase Receipt Entry window (VMRCVENT.EXE) the way a
/// macro would, without deploying or running any macro script.
/// </summary>
/// <remarks>
/// Reads use UI Automation. Field controls are Gupta panes that expose no patterns and are not
/// keyboard focusable, so writes are performed with real mouse and keyboard input: click the field,
/// select all, type, then verify by reading the field back.
/// Saving is never performed - no toolbar button is clicked and no save key sequence is sent.
/// </remarks>
public interface IService_VisualAutomation_WindowDriver
{
    /// <summary>
    /// Locates the running Purchase Receipt Entry window.
    /// </summary>
    /// <returns>The window snapshot, with <see cref="Model_VisualWindowInfo.IsRunning"/> set accordingly.</returns>
    Model_VisualWindowInfo DiscoverWindow();

    /// <summary>
    /// Reads every macro variable the harness can resolve on the visible window.
    /// </summary>
    /// <returns>The field snapshot.</returns>
    Model_VisualFieldSnapshot CaptureSnapshot();

    /// <summary>
    /// Reads a single macro variable from the window.
    /// </summary>
    /// <param name="key">Macro variable name, for example <c>ORDER_ID</c>.</param>
    /// <returns>The value, or a failure when the variable has no visible control.</returns>
    Model_Dao_Result<string> ReadValue(string key);

    /// <summary>
    /// Types a value into a field and verifies the result, then optionally restores the prior value.
    /// </summary>
    /// <param name="key">Macro variable name; must be a writable field.</param>
    /// <param name="value">Value to type.</param>
    /// <param name="restoreOriginal">When true the original value is typed back afterwards.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The observed value after typing, and whether the restore succeeded.</returns>
    Task<Model_Dao_Result<Model_VisualWriteOutcome>> WriteValueAsync(
        string key,
        string value,
        bool restoreOriginal,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns whether the window exposes a toolbar control whose name suggests Save.
    /// </summary>
    /// <returns>True when a save-like control exists in the window.</returns>
    bool HasSaveControl();
}
