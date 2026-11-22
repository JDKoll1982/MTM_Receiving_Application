namespace MTM_Receiving_Application.Module_Core.Models.UI;

/// <summary>
/// The immutable snapshot a page hands the shell action bar.
/// </summary>
/// <remarks>
/// The snapshot is replaced whole whenever the active step or page changes, and the shell
/// re-reads it in one go rather than tracking loose per-control properties. A page that supplies
/// no provider is the same as <see cref="Empty"/>: the bar row collapses and the page takes the
/// full remaining height.
/// </remarks>
public sealed class Model_ShellActionBarState
{
    /// <summary>
    /// Gets a value indicating whether the bar has anything to show.
    /// </summary>
    public bool IsVisible { get; init; }

    /// <summary>
    /// Gets a value indicating whether the entry-mode group is shown. False on every page that is
    /// not part of Receiving.
    /// </summary>
    public bool ShowModeGroup { get; init; }

    /// <summary>
    /// Gets the Guided control.
    /// </summary>
    public Model_ShellModeAction Guided { get; init; } = Model_ShellModeAction.Hidden;

    /// <summary>
    /// Gets the Manual control.
    /// </summary>
    public Model_ShellModeAction Manual { get; init; } = Model_ShellModeAction.Hidden;

    /// <summary>
    /// Gets the Edit control.
    /// </summary>
    public Model_ShellModeAction Edit { get; init; } = Model_ShellModeAction.Hidden;

    /// <summary>
    /// Gets the Overlay control.
    /// </summary>
    public Model_ShellModeAction Overlay { get; init; } = Model_ShellModeAction.Hidden;

    /// <summary>
    /// Gets the Image Search control. Non-Inventoried Labels only.
    /// </summary>
    public Model_ShellModeAction ImageSearch { get; init; } = Model_ShellModeAction.Hidden;

    /// <summary>
    /// Gets the Back position, which may carry a page-specific verb instead.
    /// </summary>
    public Model_ShellActionSlot Back { get; init; } = Model_ShellActionSlot.Hidden;

    /// <summary>
    /// Gets the Next position, which may carry a page-specific verb instead.
    /// </summary>
    public Model_ShellActionSlot Next { get; init; } = Model_ShellActionSlot.Hidden;

    /// <summary>
    /// Gets the Clear Label Data position.
    /// </summary>
    public Model_ShellActionSlot ClearLabelData { get; init; } = Model_ShellActionSlot.Hidden;

    /// <summary>
    /// Gets the Help position.
    /// </summary>
    public Model_ShellActionSlot Help { get; init; } = Model_ShellActionSlot.Hidden;

    /// <summary>
    /// Gets a value indicating whether Guided is the stored default.
    /// </summary>
    public bool IsGuidedDefault { get; init; }

    /// <summary>
    /// Gets a value indicating whether Manual is the stored default.
    /// </summary>
    public bool IsManualDefault { get; init; }

    /// <summary>
    /// Gets a value indicating whether Edit is the stored default.
    /// </summary>
    public bool IsEditDefault { get; init; }

    /// <summary>
    /// Gets a value indicating whether Image Search is the stored default.
    /// </summary>
    public bool IsImageSearchDefault { get; init; }

    /// <summary>
    /// Gets the snapshot that offers nothing.
    /// </summary>
    public static Model_ShellActionBarState Empty { get; } = new();
}
