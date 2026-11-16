namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// One chip on the guided receiving walk's step trail.
/// </summary>
/// <remarks>
/// The trail only reports state; it never drives the workflow. A clerk can read where they are and
/// what is left without the walk being able to jump by clicking a chip, which is the same reason the
/// Next button is the only way forward.
/// </remarks>
public sealed class Model_ReceivingWorkflowTrailStep
{
    /// <summary>Position in the walk, counting from 1.</summary>
    public required int Number { get; init; }

    /// <summary>The short name shown on the chip.</summary>
    public required string Label { get; init; }

    /// <summary>True for the step the clerk is on right now.</summary>
    public required bool IsCurrent { get; init; }

    /// <summary>True for a step that has already been completed.</summary>
    public required bool IsComplete { get; init; }

    /// <summary>What a screen reader says for this chip.</summary>
    public string AccessibleName =>
        IsCurrent ? $"Step {Number}, {Label}, current step"
        : IsComplete ? $"Step {Number}, {Label}, done"
        : $"Step {Number}, {Label}";

    /// <summary>The label with the state appended, so the trail reads correctly as one line of text.</summary>
    public override string ToString() => AccessibleName;
}
