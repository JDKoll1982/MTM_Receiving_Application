using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Result of the SyncTool pre-flight requirement check.
/// </summary>
public sealed class Model_SyncToolReadiness
{
    /// <summary>Gets a value indicating whether every requirement is satisfied.</summary>
    public bool IsReady { get; init; }

    /// <summary>
    /// Gets a human-readable checklist, one line per requirement, each already
    /// prefixed with a check/cross glyph (e.g. "✓" / "✗").
    /// </summary>
    public IReadOnlyList<string> Checks { get; init; } = Array.Empty<string>();
}
