using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Routing.Models;

/// <summary>
/// Model representing an internal routing label
/// Database Table: routing_labels
/// </summary>
public partial class Model_Routing_Label : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private int _labelNumber;

    [ObservableProperty]
    private string _deliverTo = string.Empty;

    [ObservableProperty]
    private string _department = string.Empty;

    [ObservableProperty]
    private string? _packageDescription;

    [ObservableProperty]
    private string? _poNumber;

    [ObservableProperty]
    private string? _workOrder;

    [ObservableProperty]
    private string _employeeNumber = string.Empty;

    [ObservableProperty]
    private DateTime _createdDate = DateTime.Today;

    [ObservableProperty]
    private bool _isArchived = false;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.Now;
}
