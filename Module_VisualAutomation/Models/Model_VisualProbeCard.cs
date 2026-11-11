using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_VisualAutomation.Enums;

namespace MTM_Receiving_Application.Module_VisualAutomation.Models;

/// <summary>
/// Observable projection of a <see cref="Model_VisualProbeDefinition"/> shown as a collapsible card.
/// </summary>
public sealed partial class Model_VisualProbeCard : ObservableObject
{
    public Model_VisualProbeCard(Model_VisualProbeDefinition definition)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
    }

    /// <summary>Gets the immutable probe definition behind this card.</summary>
    public Model_VisualProbeDefinition Definition { get; }

    public string Id => Definition.Id;

    public string Title => Definition.Title;

    public string Category => Definition.Category;

    public string Purpose => Definition.Purpose;

    public string UiSteps => Definition.UiSteps;

    public string Expected => Definition.Expected;

    /// <summary>Gets the label describing whether the probe types into the window.</summary>
    public string WindowImpactLabel => Definition.WritesToWindow ? "Types + restores" : "Read only";

    [ObservableProperty]
    private Enum_VisualProbeStatus _status = Enum_VisualProbeStatus.NotRun;

    [ObservableProperty]
    private string _statusText = "Not run";

    [ObservableProperty]
    private string _detailText = string.Empty;

    [ObservableProperty]
    private string _capturedValues = string.Empty;

    [ObservableProperty]
    private bool _hasCapturedValues;

    [ObservableProperty]
    private bool _isSelected = true;

    /// <summary>
    /// Gets or sets the command the card's Run button binds to. Assigned by the page view model so
    /// the item template can stay on compiled bindings.
    /// </summary>
    [ObservableProperty]
    private CommunityToolkit.Mvvm.Input.IAsyncRelayCommand<Model_VisualProbeCard>? _runCommand;

    /// <summary>
    /// Applies a probe result to the card.
    /// </summary>
    /// <param name="result">The probe result.</param>
    public void ApplyResult(Model_VisualProbeResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        Status = result.IsSuccess ? Enum_VisualProbeStatus.Passed : Enum_VisualProbeStatus.Failed;
        StatusText = result.IsSuccess ? "Passed" : "Failed";
        DetailText = result.Verdict;
        CapturedValues = result.Values.Count == 0
            ? string.Empty
            : string.Join(
                Environment.NewLine,
                result.Values.Select(pair => $"{pair.Key} = {pair.Value}")
            );
        HasCapturedValues = CapturedValues.Length > 0;
    }

    /// <summary>
    /// Marks the card as running.
    /// </summary>
    public void MarkRunning()
    {
        Status = Enum_VisualProbeStatus.Running;
        StatusText = "Running";
        DetailText = string.Empty;
    }

    /// <summary>
    /// Resets the card to its initial state.
    /// </summary>
    public void Reset()
    {
        Status = Enum_VisualProbeStatus.NotRun;
        StatusText = "Not run";
        DetailText = string.Empty;
        CapturedValues = string.Empty;
        HasCapturedValues = false;
    }
}
