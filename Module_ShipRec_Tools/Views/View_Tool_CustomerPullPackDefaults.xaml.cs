using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Lightweight defaults editor for Customer Pull n' Pack saved preferences.
/// </summary>
public sealed partial class View_Tool_CustomerPullPackDefaults : Page
{
    private readonly string _userId;

    public string DefaultCustomerId { get; set; }

    public string FavoriteCustomersText { get; set; }

    public DateTimeOffset LastGoodDateFrom { get; set; }

    public DateTimeOffset LastGoodDateTo { get; set; }

    public Enum_CustomerPullPackSortMode DefaultSortMode { get; set; }

    public Enum_CustomerPullPackPrintMode DefaultPrintPreset { get; set; }

    public bool DefaultShortagesOnly { get; set; }

    public bool DefaultUnpulledOnly { get; set; }

    public bool DefaultLateOrdersOnly { get; set; }

    public bool IncludeRequested { get; set; }

    public bool IncludeAccepted { get; set; }

    public bool IncludeProblem { get; set; }

    public bool IncludeCompleted { get; set; }

    public bool IncludeCancelled { get; set; }

    public IReadOnlyList<Enum_CustomerPullPackSortMode> SortModes { get; } =
        Enum.GetValues<Enum_CustomerPullPackSortMode>();

    public IReadOnlyList<Enum_CustomerPullPackPrintMode> PrintModes { get; } =
        Enum.GetValues<Enum_CustomerPullPackPrintMode>();

    public View_Tool_CustomerPullPackDefaults(Model_CustomerPullPack_UserDefaults defaults)
    {
        ArgumentNullException.ThrowIfNull(defaults);

        _userId = defaults.UserId;
        DefaultCustomerId = defaults.DefaultCustomerId;
        FavoriteCustomersText = string.Join(Environment.NewLine, defaults.FavoriteCustomerIds);
        LastGoodDateFrom = new DateTimeOffset(defaults.LastGoodDateFrom ?? DateTime.Today);
        LastGoodDateTo = new DateTimeOffset(defaults.LastGoodDateTo ?? DateTime.Today.AddDays(7));
        DefaultSortMode = defaults.DefaultSortMode;
        DefaultPrintPreset = defaults.DefaultPrintPreset;
        DefaultShortagesOnly = defaults.DefaultShortagesOnly;
        DefaultUnpulledOnly = defaults.DefaultUnpulledOnly;
        DefaultLateOrdersOnly = defaults.DefaultLateOrdersOnly;
        IncludeRequested = defaults.DefaultWaitlistStatusSet.Contains(
            Enum_CustomerPullPackWaitlistStatus.Requested
        );
        IncludeAccepted = defaults.DefaultWaitlistStatusSet.Contains(
            Enum_CustomerPullPackWaitlistStatus.Accepted
        );
        IncludeProblem = defaults.DefaultWaitlistStatusSet.Contains(
            Enum_CustomerPullPackWaitlistStatus.Problem
        );
        IncludeCompleted = defaults.DefaultWaitlistStatusSet.Contains(
            Enum_CustomerPullPackWaitlistStatus.Completed
        );
        IncludeCancelled = defaults.DefaultWaitlistStatusSet.Contains(
            Enum_CustomerPullPackWaitlistStatus.Cancelled
        );

        InitializeComponent();
    }

    public Model_CustomerPullPack_UserDefaults BuildDefaults()
    {
        var favoriteCustomers = FavoriteCustomersText
            .Split([',', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(static customerId => customerId.Trim().ToUpperInvariant())
            .Where(static customerId => string.IsNullOrWhiteSpace(customerId) is false)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var statuses = new List<Enum_CustomerPullPackWaitlistStatus>();

        if (IncludeRequested)
        {
            statuses.Add(Enum_CustomerPullPackWaitlistStatus.Requested);
        }

        if (IncludeAccepted)
        {
            statuses.Add(Enum_CustomerPullPackWaitlistStatus.Accepted);
        }

        if (IncludeProblem)
        {
            statuses.Add(Enum_CustomerPullPackWaitlistStatus.Problem);
        }

        if (IncludeCompleted)
        {
            statuses.Add(Enum_CustomerPullPackWaitlistStatus.Completed);
        }

        if (IncludeCancelled)
        {
            statuses.Add(Enum_CustomerPullPackWaitlistStatus.Cancelled);
        }

        return new Model_CustomerPullPack_UserDefaults
        {
            UserId = _userId,
            DefaultCustomerId = DefaultCustomerId?.Trim().ToUpperInvariant() ?? string.Empty,
            FavoriteCustomerIds = favoriteCustomers,
            LastGoodDateRangeType = "Custom",
            LastGoodDateFrom = LastGoodDateFrom.Date,
            LastGoodDateTo = LastGoodDateTo.Date,
            DefaultSortMode = DefaultSortMode,
            DefaultShortagesOnly = DefaultShortagesOnly,
            DefaultUnpulledOnly = DefaultUnpulledOnly,
            DefaultLateOrdersOnly = DefaultLateOrdersOnly,
            DefaultWaitlistStatusSet = statuses,
            DefaultPrintPreset = DefaultPrintPreset,
        };
    }
}
