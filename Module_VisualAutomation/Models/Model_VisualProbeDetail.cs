namespace MTM_Receiving_Application.Module_VisualAutomation.Models;

/// <summary>
/// One row in the probe details modal: what the probe was, how it ended, and what it captured.
/// </summary>
public sealed class Model_VisualProbeDetail
{
    /// <summary>Gets the probe identifier.</summary>
    public required string ProbeId { get; init; }

    /// <summary>Gets the probe heading.</summary>
    public required string Title { get; init; }

    /// <summary>Gets the short status word, for example Pass or Fail.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the verdict explanation, or an empty string when the probe has not run.</summary>
    public string Verdict { get; init; } = string.Empty;

    /// <summary>Gets the captured values as formatted text, or an empty string when nothing was captured.</summary>
    public string CapturedValues { get; init; } = string.Empty;
}
