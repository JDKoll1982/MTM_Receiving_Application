using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Reconciliation.Models;

namespace MTM_Receiving_Application.Module_Reconciliation.ViewModels;

/// <summary>
/// The backlog sweep. It repeats the identical single-day pass one day at a time,
/// shows per-day progress, stops between days on request, stops on the first day
/// that fails and leaves the operator there, and never widens either window
/// (FR-011, FR-012, FR-014).
/// </summary>
public partial class ViewModel_ReconciliationMain
{
    private readonly HashSet<DateOnly> _completedDays = [];
    private bool _stopRequested;

    /// <summary>The first day of a sweep.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SweepStartDateOffset))]
    private DateOnly _sweepStartDate = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>The last day of a sweep.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SweepEndDateOffset))]
    private DateOnly _sweepEndDate = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>The sweep start as the date picker understands it.</summary>
    public DateTimeOffset SweepStartDateOffset =>
        new(SweepStartDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    /// <summary>The sweep end as the date picker understands it.</summary>
    public DateTimeOffset SweepEndDateOffset =>
        new(SweepEndDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    /// <summary>Per-day progress, or an empty string when no sweep has run.</summary>
    [ObservableProperty]
    private string _sweepProgressText = string.Empty;

    /// <summary>True while a sweep is running.</summary>
    [ObservableProperty]
    private bool _isSweepRunning;

    /// <summary>True when the day on the page completed in this session.</summary>
    [ObservableProperty]
    private bool _isSelectedDayCompleted;

    /// <summary>True when the given day completed earlier in this session.</summary>
    /// <param name="date">The day to test.</param>
    public bool IsDayCompleted(DateOnly date) => _completedDays.Contains(date);

    /// <summary>
    /// Runs the sweep day by day. Each day goes through the same single-day path the
    /// stepper uses, so nothing about a sweep can widen what a single day sees.
    /// </summary>
    [RelayCommand]
    private async Task StartSweepAsync()
    {
        if (IsSweepRunning || SweepEndDate < SweepStartDate)
        {
            return;
        }

        _stopRequested = false;
        IsSweepRunning = true;

        // Both windows are read once, so the sweep is fixing them for every day it
        // touches rather than letting anything drift mid-run (FR-014).
        await ReconciliationSettings.GetDiscoveryWindowDaysAsync();
        await ReconciliationSettings.GetStrictWindowDaysAsync();

        int dayCount = SweepEndDate.DayNumber - SweepStartDate.DayNumber + 1;
        int position = 0;

        try
        {
            for (
                DateOnly day = SweepStartDate;
                day <= SweepEndDate && _stopRequested is false;
                day = day.AddDays(1)
            )
            {
                position++;
                SweepProgressText = $"Day {position} of {dayCount} — {day:yyyy-MM-dd}";

                await LoadDayAsync(day);

                if (DayState is Enum_ReconciliationDayState.Failed)
                {
                    SweepProgressText =
                        $"Stopped on {day:yyyy-MM-dd} because the day failed. {FailureMessage}";
                    break;
                }

                _completedDays.Add(day);
                IsSelectedDayCompleted = true;
            }
        }
        finally
        {
            IsSweepRunning = false;
        }
    }

    /// <summary>Stops the sweep between days.</summary>
    [RelayCommand]
    private void StopSweep() => _stopRequested = true;

    partial void OnSelectedDateChanged(DateOnly value) =>
        IsSelectedDayCompleted = _completedDays.Contains(value);
}
