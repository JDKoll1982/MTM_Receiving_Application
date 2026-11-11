using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_VisualAutomation.Models;

/// <summary>
/// Outcome of one automation probe.
/// </summary>
public sealed class Model_VisualProbeResult
{
    /// <summary>Gets the probe identifier that produced this result.</summary>
    public required string ProbeId { get; init; }

    /// <summary>Gets a value indicating whether the probe satisfied every requirement.</summary>
    public required bool IsSuccess { get; init; }

    /// <summary>Gets the key/value pairs captured while the probe ran.</summary>
    public IReadOnlyDictionary<string, string> Values { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>Gets a short explanation of the verdict.</summary>
    public string Verdict { get; init; } = string.Empty;

    /// <summary>Gets the moment the probe finished.</summary>
    public DateTimeOffset CompletedAt { get; init; } = DateTimeOffset.Now;

    /// <summary>
    /// Creates a passing result.
    /// </summary>
    /// <param name="probeId">Probe identifier.</param>
    /// <param name="verdict">Explanation shown on the card.</param>
    /// <param name="values">Captured values.</param>
    /// <returns>The result.</returns>
    public static Model_VisualProbeResult Pass(
        string probeId,
        string verdict,
        IReadOnlyDictionary<string, string>? values = null
    ) =>
        new()
        {
            ProbeId = probeId,
            IsSuccess = true,
            Verdict = verdict,
            Values = values ?? new Dictionary<string, string>(StringComparer.Ordinal),
        };

    /// <summary>
    /// Creates a failing result.
    /// </summary>
    /// <param name="probeId">Probe identifier.</param>
    /// <param name="verdict">Explanation shown on the card.</param>
    /// <param name="values">Captured values.</param>
    /// <returns>The result.</returns>
    public static Model_VisualProbeResult Fail(
        string probeId,
        string verdict,
        IReadOnlyDictionary<string, string>? values = null
    ) =>
        new()
        {
            ProbeId = probeId,
            IsSuccess = false,
            Verdict = verdict,
            Values = values ?? new Dictionary<string, string>(StringComparer.Ordinal),
        };
}
