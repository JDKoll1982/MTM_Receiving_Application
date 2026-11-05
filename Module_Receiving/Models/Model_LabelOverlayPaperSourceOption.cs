namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// One tray a printer offers, kept by the name the operator recognises and the number the driver uses.
/// </summary>
/// <param name="Name">The name the driver uses, e.g. "Auto" or "Bypass Tray".</param>
/// <param name="RawKind">The driver's own number for the tray, which is what a print job asks for.</param>
public sealed record Model_LabelOverlayPaperSourceOption(string Name, short RawKind)
{
    /// <summary>What the picker shows.</summary>
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? $"Tray {RawKind}" : Name;

    public override string ToString() => DisplayName;
}
