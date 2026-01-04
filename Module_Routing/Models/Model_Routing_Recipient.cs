using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Routing.Models;

/// <summary>
/// Model representing a routing recipient with default department for auto-fill
/// Database Table: routing_recipients
/// </summary>
public partial class Model_Routing_Recipient : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _defaultDepartment;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private DateTime _createdDate = DateTime.Now;
}
