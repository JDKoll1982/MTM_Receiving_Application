using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.Models.Core;

/// <summary>
/// Model representing help content for the application
/// Used by IService_Help to provide contextual help, tips, tooltips, and tutorials
/// </summary>
public partial class Model_HelpContent : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private Enum_HelpType _helpType = Enum_HelpType.Info;

    [ObservableProperty]
    private string _icon = "HelpCircle";

    [ObservableProperty]
    private Enum_HelpSeverity _severity = Enum_HelpSeverity.Info;

    [ObservableProperty]
    private List<string> _relatedKeys = new();

    [ObservableProperty]
    private DateTime _lastUpdated = DateTime.Now;

    /// <summary>
    /// Gets the MaterialIconKind enum for displaying the icon
    /// </summary>
    public MaterialIconKind IconKind
    {
        get
        {
            if (!string.IsNullOrEmpty(Icon) && Enum.TryParse<MaterialIconKind>(Icon, true, out var kind))
            {
                return kind;
            }
            return MaterialIconKind.HelpCircle;
        }
    }
}
