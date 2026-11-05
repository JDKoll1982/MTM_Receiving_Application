namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// One paper size a printer offers, with its edges reduced to inches so the chosen media can be matched
/// against the tag's measured layout.
/// </summary>
/// <param name="Name">The name the driver uses, e.g. "Letter".</param>
/// <param name="WidthInches">The media's short edge, in inches.</param>
/// <param name="HeightInches">The media's long edge, in inches.</param>
public sealed record Model_LabelOverlayPaperOption(
    string Name,
    float WidthInches,
    float HeightInches
)
{
    /// <summary>What the picker shows, so a size is recognisable without knowing the driver's naming.</summary>
    public string DisplayName => $"{Name} ({WidthInches:0.##}\" x {HeightInches:0.##}\")";

    public override string ToString() => DisplayName;
}
