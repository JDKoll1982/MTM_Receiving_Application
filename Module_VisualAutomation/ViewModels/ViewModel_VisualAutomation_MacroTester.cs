using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_VisualAutomation.Contracts;
using MTM_Receiving_Application.Module_VisualAutomation.Enums;
using MTM_Receiving_Application.Module_VisualAutomation.Models;
using MTM_Receiving_Application.Module_VisualAutomation.Probes;

namespace MTM_Receiving_Application.Module_VisualAutomation.ViewModels;

/// <summary>
/// Drives the Visual Macro Tester page. Despite the page name, no macro is deployed or executed: the
/// app performs the same field reads and writes itself against the live Purchase Receipt Entry
/// window and reports one collapsible card per probe.
/// </summary>
public partial class ViewModel_VisualAutomation_MacroTester : ViewModel_Shared_Base
{
    private const string LogContext = nameof(ViewModel_VisualAutomation_MacroTester);

    private readonly IService_VisualAutomation_WindowDriver _driver;
    private readonly IService_VisualAutomation_ProbeRunner _runner;

    private CancellationTokenSource? _runCancellation;

    public ViewModel_VisualAutomation_MacroTester(
        IService_VisualAutomation_WindowDriver driver,
        IService_VisualAutomation_ProbeRunner runner,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));

        Probes = new ObservableCollection<Model_VisualProbeCard>(
            VisualProbeCatalog.All.Select(definition => new Model_VisualProbeCard(definition))
        );

        foreach (var card in Probes)
        {
            card.RunCommand = RunSingleCommand;
        }

        Title = "Visual Macro Tester";
    }

    /// <summary>Gets the probe cards shown on the page.</summary>
    public ObservableCollection<Model_VisualProbeCard> Probes { get; }

    /// <summary>Gets the activity log shown under the cards.</summary>
    public ObservableCollection<Model_VisualLogEntry> LogEntries { get; } = [];

    [ObservableProperty]
    private string _targetSummary = "Not checked yet.";

    [ObservableProperty]
    private bool _isTargetReady;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _instructionText =
        "Open Inventory > Purchase Receipt Entry in Infor VISUAL, load a PO, then run the probes.";

    [ObservableProperty]
    private string _snapshotSummary = string.Empty;

    /// <summary>Gets the number of probes that passed in the last run.</summary>
    public int PassedCount => Probes.Count(card => card.Status == Enum_VisualProbeStatus.Passed);

    /// <summary>Gets the number of probes that failed in the last run.</summary>
    public int FailedCount => Probes.Count(card => card.Status == Enum_VisualProbeStatus.Failed);

    /// <summary>
    /// Refreshes the target window snapshot and the summary line.
    /// </summary>
    public void Initialize()
    {
        try
        {
            var window = _driver.DiscoverWindow();
            ApplyWindow(window);

            AddLog(
                window.IsRunning ? "INFO" : "WARN",
                window.IsRunning
                    ? $"Found {window.WindowTitle} (PID {window.ProcessId}) with {window.ElementCount} elements at {window.WindowRect}."
                    : window.Diagnostic ?? "Purchase Receipt Entry is not running."
            );

            if (window.IsRunning)
            {
                RefreshSnapshotSummary();
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Error,
                nameof(Initialize),
                LogContext
            );
        }
    }

    [RelayCommand]
    private void RefreshTarget() => Initialize();

    [RelayCommand]
    private async Task RunSelectedAsync()
    {
        var selected = Probes.Where(card => card.IsSelected).ToList();
        if (selected.Count == 0)
        {
            ShowStatus("Select at least one probe card first.", InfoBarSeverity.Warning);
            return;
        }

        await RunProbesAsync(selected).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task RunSingleAsync(Model_VisualProbeCard card) =>
        await RunProbesAsync([card]).ConfigureAwait(true);

    [RelayCommand]
    private void CancelRun()
    {
        _runCancellation?.Cancel();
        IsRunning = false;
        AddLog("INFO", "Run cancelled.");
    }

    [RelayCommand]
    private void Reset()
    {
        _runCancellation?.Cancel();

        foreach (var card in Probes)
        {
            card.Reset();
        }

        LogEntries.Clear();
        RaiseRunCounts();
    }

    private async Task RunProbesAsync(IReadOnlyList<Model_VisualProbeCard> cards)
    {
        if (IsRunning)
        {
            return;
        }

        var window = _driver.DiscoverWindow();
        ApplyWindow(window);

        if (!window.IsRunning)
        {
            var message = window.Diagnostic ?? "Purchase Receipt Entry is not running.";
            ShowStatus(message, InfoBarSeverity.Warning);
            AddLog("WARN", message);
            return;
        }

        IsRunning = true;
        _runCancellation?.Dispose();
        _runCancellation = new CancellationTokenSource();

        try
        {
            foreach (var card in cards)
            {
                card.MarkRunning();
            }

            InstructionText =
                "Running. A write probe briefly takes focus of the VISUAL window - do not click or type until it finishes.";

            var definitions = cards.Select(card => card.Definition).ToList();

            await _runner
                .RunAllAsync(
                    definitions,
                    result =>
                    {
                        ApplyResult(result);
                        RaiseRunCounts();
                    },
                    _runCancellation.Token
                )
                .ConfigureAwait(true);

            InstructionText =
                $"Run finished - {PassedCount} passed, {FailedCount} failed. Nothing was saved; the VISUAL Save button was never pressed.";

            AddLog(
                FailedCount == 0 ? "PASS" : "WARN",
                $"Run finished: {PassedCount} passed, {FailedCount} failed."
            );

            RefreshSnapshotSummary();
        }
        catch (OperationCanceledException)
        {
            AddLog("INFO", "Run cancelled.");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Error,
                nameof(RunProbesAsync),
                LogContext
            );
        }
        finally
        {
            IsRunning = false;
        }
    }

    private void ApplyWindow(Model_VisualWindowInfo window)
    {
        IsTargetReady = window.IsRunning;

        TargetSummary = window.IsRunning
            ? $"{window.WindowTitle}  |  PID {window.ProcessId}  |  {window.ElementCount} elements  |  {window.WindowRect}"
            : window.Diagnostic ?? "Purchase Receipt Entry is not running.";
    }

    private void RefreshSnapshotSummary()
    {
        var snapshot = _driver.CaptureSnapshot();
        var resolved = snapshot.Fields.Values.Count(field => field.HasControl);

        SnapshotSummary =
            $"Order {snapshot.ValueOf("ORDER_ID")}  |  Receiver {snapshot.ValueOf("RECEIVER_ID")}  |  "
            + $"{resolved} of {snapshot.Fields.Count} mapped variables readable  |  Save control present: "
            + (snapshot.SaveControlPresent ? "yes" : "no");
    }

    private void ApplyResult(Model_VisualProbeResult result)
    {
        var card = Probes.FirstOrDefault(item => item.Id == result.ProbeId);
        if (card is null)
        {
            return;
        }

        card.ApplyResult(result);
        AddLog(result.IsSuccess ? "PASS" : "FAIL", $"{card.Title}: {result.Verdict}");
    }

    private void AddLog(string level, string message)
    {
        LogEntries.Add(new Model_VisualLogEntry { Level = level, Message = message });

        while (LogEntries.Count > 500)
        {
            LogEntries.RemoveAt(0);
        }

        _logger.LogInfo($"[{level}] {message}", LogContext);
    }

    private void RaiseRunCounts()
    {
        OnPropertyChanged(nameof(PassedCount));
        OnPropertyChanged(nameof(FailedCount));
    }
}
