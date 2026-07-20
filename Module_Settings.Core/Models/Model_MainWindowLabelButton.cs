using System;
using Material.Icons;
using MTM_Receiving_Application.Module_Settings.Core.Enums;

namespace MTM_Receiving_Application.Module_Settings.Core.Models;

public class Model_MainWindowLabelButton
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string LabelPath { get; set; } = string.Empty;
    public string IconKey { get; set; } = "PackageVariantClosed";
    public Enum_MainWindowLabelButtonAccent Accent { get; set; } =
        Enum_MainWindowLabelButtonAccent.Neutral;
    public bool IsEnabled { get; set; } = true;
    public int SortOrder { get; set; }

    public MaterialIconKind IconKind =>
        Enum.TryParse<MaterialIconKind>(IconKey, true, out var iconKind)
            ? iconKind
            : MaterialIconKind.PackageVariantClosed;
}
