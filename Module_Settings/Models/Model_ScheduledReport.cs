using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MTM_Receiving_Application.Module_Settings.Models;

/// <summary>
/// Represents a scheduled report configuration
/// Database Table: reporting_scheduled_reports
/// </summary>
public partial class Model_ScheduledReport : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _reportType = string.Empty;

    [ObservableProperty]
    private string _schedule = string.Empty;

    [ObservableProperty]
    private string? _emailRecipients;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private DateTime? _nextRunDate;

    [ObservableProperty]
    private DateTime? _lastRunDate;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private DateTime _updatedAt;

    [ObservableProperty]
    private int? _createdBy;
}
