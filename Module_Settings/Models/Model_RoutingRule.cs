using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MTM_Receiving_Application.Module_Settings.Models;

/// <summary>
/// Represents an auto-routing rule with pattern matching
/// Database Table: routing_home_locations
/// </summary>
public partial class Model_RoutingRule : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _matchType = "Part Number";

    [ObservableProperty]
    private string _pattern = string.Empty;

    [ObservableProperty]
    private string _destinationLocation = string.Empty;

    [ObservableProperty]
    private int _priority = 50;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private DateTime _updatedAt;

    [ObservableProperty]
    private int? _createdBy;
}
