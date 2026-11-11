using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_VisualAutomation.Models;

/// <summary>
/// Defines one automation probe: what it proves, what it does on the VISUAL window, and what a pass
/// looks like.
/// </summary>
public sealed class Model_VisualProbeDefinition
{
    /// <summary>Gets the stable identifier used for the card, logs, and results.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the card heading.</summary>
    public required string Title { get; init; }

    /// <summary>Gets the capability group shown on the card.</summary>
    public required string Category { get; init; }

    /// <summary>Gets the description of the capability being verified.</summary>
    public required string Purpose { get; init; }

    /// <summary>Gets the plain-language description of what the probe does on the VISUAL window.</summary>
    public required string UiSteps { get; init; }

    /// <summary>Gets the description of a successful outcome.</summary>
    public required string Expected { get; init; }

    /// <summary>Gets a value indicating whether the probe types into the window.</summary>
    public bool WritesToWindow { get; init; }

    /// <summary>Gets the macro variables this probe reports on.</summary>
    public IReadOnlyList<string> ReportedKeys { get; init; } = [];
}
