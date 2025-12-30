using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace MTM_Receiving_Application.Models.Dunnage;

/// <summary>
/// Represents an icon definition for the Add Type Dialog icon picker.
/// Contains Material Design Icons metadata.
/// </summary>
public partial class Model_IconDefinition : ObservableObject
{
    [ObservableProperty]
    private MaterialIconKind _kind;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private int _usageCount;

    [ObservableProperty]
    private bool _isRecentlyUsed;

    private static readonly Dictionary<string, MaterialIconKind> LegacyMapping = new()
    {
        { "Box", MaterialIconKind.PackageVariantClosed },
        { "Package", MaterialIconKind.PackageVariant },
        { "Cube", MaterialIconKind.CubeOutline },
        { "Folder", MaterialIconKind.Folder },
        { "Calendar", MaterialIconKind.Calendar },
        { "Mail", MaterialIconKind.Email },
        { "Flag", MaterialIconKind.Flag },
        { "Star", MaterialIconKind.Star },
        { "Heart", MaterialIconKind.Heart },
        { "Pin", MaterialIconKind.Pin },
        { "Tag", MaterialIconKind.Tag },
        { "Important", MaterialIconKind.AlertCircle },
        { "Warning", MaterialIconKind.Alert },
        { "Info", MaterialIconKind.Information },
        { "Document", MaterialIconKind.FileDocument },
        { "Image", MaterialIconKind.Image },
        { "Music", MaterialIconKind.MusicNote },
        { "Video", MaterialIconKind.Video }
    };

    // Alias for compatibility
    public string IconName
    {
        get => Kind.ToString();
        set
        {
            if (System.Enum.TryParse<MaterialIconKind>(value, out var result))
            {
                Kind = result;
                Name = value;
            }
            else if (LegacyMapping.TryGetValue(value, out var legacyResult))
            {
                Kind = legacyResult;
                Name = legacyResult.ToString();
            }
            else
            {
                Name = value;
            }
        }
    }
}
