using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for the Database Config page. Lets an admin/developer choose which
/// MySQL database the application connects to on its next launch, and run the
/// reference-data SyncTool with explicit, checkbox-driven options.
/// </summary>
public partial class ViewModel_Settings_DatabaseConfig : ViewModel_Shared_Base
{
    private static readonly (Enum_SyncToolAction Action, string Label)[] SyncActions =
    {
        (Enum_SyncToolAction.WhatIf, "What-if plan (read-only)"),
        (Enum_SyncToolAction.Inspect, "Inspect drift report (read-only)"),
        (Enum_SyncToolAction.GenerateSql, "Generate reviewable SQL (no writes)"),
        (Enum_SyncToolAction.Execute, "Execute sync (writes into the chosen target)"),
        (Enum_SyncToolAction.VerifyCopy, "Verify on disposable copy (make + sync + verify)"),
    };

    private readonly IService_DatabaseTargetManager _targetManager;
    private readonly IService_SyncToolRunner _syncRunner;
    private readonly IService_Dispatcher _dispatcher;
    private readonly StringBuilder _outputBuffer = new();
    private CancellationTokenSource? _runCts;

    /// <summary>Password for the SyncTool run; populated by the page, never logged.</summary>
    public string? SyncPassword { get; set; }

    public ObservableCollection<Model_DatabaseTarget> ConnectionTargets { get; } = new();

    public IReadOnlyList<Model_SyncTargetOption> SyncTargetOptions { get; } =
        new List<Model_SyncTargetOption>
        {
            new("core", "Core (live)", "mtm_receiving_application"),
            new("backup_test", "Disposable copy", "mtm_receiving_application_backup_test"),
        };

    public IReadOnlyList<string> SyncActionChoices { get; } = SyncActions
        .Select(action => action.Label)
        .ToArray();

    [ObservableProperty]
    private Model_DatabaseTarget? _selectedConnectionTarget;

    [ObservableProperty]
    private string _currentDatabaseDisplay = "(loading...)";

    [ObservableProperty]
    private string _currentServerDisplay = string.Empty;

    [ObservableProperty]
    private string _selectedConnectionTargetSummary = string.Empty;

    [ObservableProperty]
    private string _pendingStateText = string.Empty;

    [ObservableProperty]
    private bool _isRestartPending;

    [ObservableProperty]
    private int _syncActionIndex;

    [ObservableProperty]
    private Model_SyncTargetOption? _selectedSyncTarget;

    [ObservableProperty]
    private bool _confirmCoreWrite;

    [ObservableProperty]
    private bool _skipValidation;

    [ObservableProperty]
    private bool _keepCopy;

    [ObservableProperty]
    private string _generateSqlOutputPath = string.Empty;

    [ObservableProperty]
    private string _syncToolPath = string.Empty;

    [ObservableProperty]
    private string _syncToolPythonPath = string.Empty;

    [ObservableProperty]
    private bool _isSyncToolAvailable;

    [ObservableProperty]
    private bool _isBundledExeAvailable;

    [ObservableProperty]
    private bool _isSyncToolBlocked = true;

    [ObservableProperty]
    private string _databaseListStatusText = string.Empty;

    [ObservableProperty]
    private string _syncToolReadinessTitle = "Checking SyncTool requirements...";

    [ObservableProperty]
    private string _syncToolRequirementText = string.Empty;

    [ObservableProperty]
    private string _syncActionCaption = string.Empty;

    [ObservableProperty]
    private string _syncOutputText = string.Empty;

    [ObservableProperty]
    private bool _isSyncRunning;

    public ViewModel_Settings_DatabaseConfig(
        IService_DatabaseTargetManager targetManager,
        IService_SyncToolRunner syncRunner,
        IService_Dispatcher dispatcher,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _targetManager =
            targetManager ?? throw new ArgumentNullException(nameof(targetManager));
        _syncRunner = syncRunner ?? throw new ArgumentNullException(nameof(syncRunner));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

        _ = LoadAsync();
    }

    /// <summary>Gets the SyncTool action selected by the current combo index.</summary>
    public bool IsGenerateSqlAction =>
        SelectedSyncAction == Enum_SyncToolAction.GenerateSql;

    /// <summary>Gets a text describing the SyncTool availability based on the configured path.</summary>
    public string SyncToolAvailabilityText => IsSyncToolBlocked
        ? "The SyncTool is not ready - resolve the requirements below, then click 'Re-check requirements'."
        : "Ready - the bundled executable will be used (no Python required).";

    /// <summary>Gets a value indicating whether the SyncTool run controls should be enabled.</summary>
    public bool IsSyncToolReady => !IsSyncToolBlocked;

    /// <summary>Gets a value indicating whether the Execute action is selected.</summary>
    public bool IsExecuteAction => SelectedSyncAction == Enum_SyncToolAction.Execute;

    /// <summary>Gets a value indicating whether the verify-copy action is selected.</summary>
    public bool IsVerifyCopyAction => SelectedSyncAction == Enum_SyncToolAction.VerifyCopy;

    /// <summary>Gets a value indicating whether an explicit confirmation checkbox is required.</summary>
    public bool IsConfirmCoreRequired =>
        IsExecuteAction && SelectedSyncTarget?.Id == "core";

    /// <summary>Gets a value indicating whether the target picker applies to the selected action.</summary>
    public bool IsSyncTargetRelevant => !IsVerifyCopyAction;

    private Enum_SyncToolAction SelectedSyncAction
    {
        get
        {
            var index = Math.Clamp(SyncActionIndex, 0, SyncActions.Length - 1);
            return SyncActions[index].Action;
        }
    }

    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading database configuration...";

            CurrentDatabaseDisplay = _targetManager.CurrentDatabaseName;
            CurrentServerDisplay = _targetManager.CurrentServerDisplay;

            await RefreshDatabaseTargetsAsync();

            await _syncRunner.RefreshConfiguredPathsAsync();
            SyncToolPath = _syncRunner.EffectiveScriptPath;
            SyncToolPythonPath = _syncRunner.EffectivePythonPath;
            IsBundledExeAvailable = _syncRunner.IsBundledExeAvailable;
            IsSyncToolAvailable = _syncRunner.IsScriptAvailable;
            RefreshSyncToolReadiness();

            var pendingDatabase = await _targetManager.GetPendingDatabaseNameAsync();
            IsRestartPending = await _targetManager.GetIsRestartPendingAsync();
            UpdatePendingState(pendingDatabase);

            SelectedSyncTarget =
                SyncTargetOptions.FirstOrDefault(target => target.Id == "core")
                ?? SyncTargetOptions.FirstOrDefault();

            SyncActionIndex = 0;
            GenerateSqlOutputPath = Path.Combine(
                AppContext.BaseDirectory,
                "Database",
                "SyncTool",
                "review_sync.sql"
            );

            StatusMessage = "Database configuration loaded.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadAsync),
                nameof(ViewModel_Settings_DatabaseConfig)
            );
            StatusMessage = "Failed to load database configuration.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshDatabaseTargetsAsync()
    {
        var enumeration = await _targetManager.EnumerateDatabasesAsync();
        var currentDatabase = _targetManager.CurrentDatabaseName;

        ConnectionTargets.Clear();
        if (enumeration.Success)
        {
            var existing = new HashSet<string>(
                enumeration.Databases,
                StringComparer.OrdinalIgnoreCase
            );
            foreach (var target in _targetManager.Targets)
            {
                if (existing.Contains(target.DatabaseName))
                {
                    ConnectionTargets.Add(target);
                }
            }

            DatabaseListStatusText =
                $"Databases present on {_targetManager.CurrentServerDisplay}: "
                + string.Join(
                    ", ",
                    enumeration.Databases.OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                );
        }
        else
        {
            foreach (var target in _targetManager.Targets)
            {
                ConnectionTargets.Add(target);
            }

            DatabaseListStatusText =
                $"Could not reach the server to list databases ({enumeration.Message}). "
                + "Showing all known targets - use Test Connection to confirm which exist.";
        }

        SelectedConnectionTarget =
            ConnectionTargets.FirstOrDefault(target =>
                string.Equals(
                    target.DatabaseName,
                    currentDatabase,
                    StringComparison.OrdinalIgnoreCase
                )
            ) ?? ConnectionTargets.FirstOrDefault();
    }

    [RelayCommand]
    private async Task RefreshDatabasesAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            await RefreshDatabaseTargetsAsync();
            SetStatus("Database list refreshed.", InfoBarSeverity.Informational);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(RefreshDatabasesAsync),
                nameof(ViewModel_Settings_DatabaseConfig)
            );
            SetStatus("Failed to refresh the database list.", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RefreshSyncToolReadiness()
    {
        var readiness = _syncRunner.GetReadiness();

        SyncToolRequirementText = string.Join(Environment.NewLine, readiness.Checks);
        IsSyncToolBlocked = !readiness.IsReady;
        SyncToolReadinessTitle = readiness.IsReady
            ? "All requirements are in place - the SyncTool is ready to run."
            : "The SyncTool cannot run yet. Resolve the items below, then click 'Re-check requirements'.";
    }

    [RelayCommand]
    private async Task RecheckRequirementsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            await _syncRunner.SaveToolPathsAsync(SyncToolPath, SyncToolPythonPath);
            await _syncRunner.RefreshConfiguredPathsAsync();
            SyncToolPath = _syncRunner.EffectiveScriptPath;
            SyncToolPythonPath = _syncRunner.EffectivePythonPath;
            RefreshSyncToolReadiness();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(RecheckRequirementsAsync),
                nameof(ViewModel_Settings_DatabaseConfig)
            );
            SetStatus("Failed to re-check SyncTool requirements.", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (SelectedConnectionTarget is null)
        {
            SetStatus("Select a database target first.", InfoBarSeverity.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Testing connection...";

            var result = await _targetManager.TestConnectionAsync(
                _targetManager.BuildConnectionString(SelectedConnectionTarget)
            );

            SetStatus(
                result.Message,
                result.Success ? InfoBarSeverity.Success : InfoBarSeverity.Error
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(TestConnectionAsync),
                nameof(ViewModel_Settings_DatabaseConfig)
            );
            SetStatus("Connection test failed unexpectedly.", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveSelectionAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (SelectedConnectionTarget is null)
        {
            SetStatus("Select a database target first.", InfoBarSeverity.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Saving database target...";

            await _targetManager.SaveTargetAsync(SelectedConnectionTarget);

            var pendingDatabase = await _targetManager.GetPendingDatabaseNameAsync();
            IsRestartPending = await _targetManager.GetIsRestartPendingAsync();
            UpdatePendingState(pendingDatabase);

            SetStatus(
                IsRestartPending
                    ? $"Saved. Restart the application to connect to '{pendingDatabase}'."
                    : $"Saved. '{pendingDatabase}' is already the active database.",
                IsRestartPending ? InfoBarSeverity.Success : InfoBarSeverity.Informational
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SaveSelectionAsync),
                nameof(ViewModel_Settings_DatabaseConfig)
            );
            SetStatus("Failed to save the database target.", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void RestartApplication()
    {
        if (IsSyncRunning)
        {
            SetStatus(
                "A SyncTool run is in progress. Wait for it to finish or cancel it before restarting.",
                InfoBarSeverity.Warning
            );
            return;
        }

        if (!IsRestartPending)
        {
            SetStatus("No database change is waiting to be applied.", InfoBarSeverity.Informational);
            return;
        }

        try
        {
            var executable = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(executable))
            {
                SetStatus(
                    "Could not determine the application path to restart. Please close and reopen the application.",
                    InfoBarSeverity.Error
                );
                return;
            }

            // Launch the new instance first so the restart survives the graceful shutdown.
            Process.Start(new ProcessStartInfo(executable) { UseShellExecute = true });

            // Closing the main window triggers the application's single shutdown path,
            // which cleans up and exits the current process.
            App.MainWindow?.Close();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(RestartApplication),
                nameof(ViewModel_Settings_DatabaseConfig)
            );
            SetStatus(
                "Restart failed. Please close and reopen the application manually.",
                InfoBarSeverity.Error
            );
        }
    }

    [RelayCommand]
    private async Task RunSyncToolAsync()
    {
        if (IsSyncRunning)
        {
            return;
        }

        if (IsSyncToolBlocked)
        {
            SetStatus(
                "The SyncTool requirements are not satisfied. Resolve the items on this page, then click 'Re-check requirements'.",
                InfoBarSeverity.Warning
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(SyncPassword))
        {
            SetStatus(
                "Enter the MySQL password used by the SyncTool (used only for this run).",
                InfoBarSeverity.Warning
            );
            return;
        }

        var action = SelectedSyncAction;
        var target = SelectedSyncTarget?.Id ?? "core";

        if (action == Enum_SyncToolAction.Execute && target == "core" && !ConfirmCoreWrite)
        {
            SetStatus(
                "Checking the 'I understand this writes to the LIVE core database' box is required before executing a sync into core.",
                InfoBarSeverity.Warning
            );
            return;
        }

        if (
            action == Enum_SyncToolAction.GenerateSql
            && string.IsNullOrWhiteSpace(GenerateSqlOutputPath)
        )
        {
            SetStatus("Provide an output path for the generated SQL file.", InfoBarSeverity.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Preparing SyncTool run...";

            await _syncRunner.SaveToolPathsAsync(SyncToolPath, SyncToolPythonPath);
            await _syncRunner.RefreshConfiguredPathsAsync();
            SyncToolPath = _syncRunner.EffectiveScriptPath;
            SyncToolPythonPath = _syncRunner.EffectivePythonPath;
            RefreshSyncToolReadiness();
            if (IsSyncToolBlocked)
            {
                IsBusy = false;
                SetStatus(
                    "The SyncTool requirements are still not satisfied - see the checklist on this page.",
                    InfoBarSeverity.Warning
                );
                return;
            }

            var request = new Model_SyncToolRunRequest
            {
                Action = action,
                Target = target,
                ConfirmCoreWrite = ConfirmCoreWrite,
                SkipValidation = SkipValidation,
                KeepCopy = KeepCopy,
                GenerateSqlOutputPath =
                    action == Enum_SyncToolAction.GenerateSql ? GenerateSqlOutputPath : null,
                SyncToolScriptPath = SyncToolPath,
            };

            SyncOutputText = string.Empty;
            _outputBuffer.Clear();
            IsSyncRunning = true;
            _runCts = new CancellationTokenSource();
            _syncRunner.OutputReceived += OnSyncOutput;

            try
            {
                var exitCode = await _syncRunner.RunAsync(
                    request,
                    SyncPassword ?? string.Empty,
                    _runCts.Token
                );

                SetStatus(
                    exitCode == 0
                        ? "SyncTool completed successfully."
                        : $"SyncTool finished with exit code {exitCode}. Review the output above.",
                    exitCode == 0 ? InfoBarSeverity.Success : InfoBarSeverity.Warning
                );
            }
            catch (OperationCanceledException)
            {
                SetStatus("SyncTool run cancelled.", InfoBarSeverity.Warning);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(
                    ex,
                    Enum_ErrorSeverity.Medium,
                    nameof(RunSyncToolAsync),
                    nameof(ViewModel_Settings_DatabaseConfig)
                );
                SetStatus("SyncTool failed to run.", InfoBarSeverity.Error);
            }
            finally
            {
                _syncRunner.OutputReceived -= OnSyncOutput;
                _runCts?.Dispose();
                _runCts = null;
                SyncPassword = null;
                IsSyncRunning = false;
                IsBusy = false;
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(RunSyncToolAsync),
                nameof(ViewModel_Settings_DatabaseConfig)
            );
            SetStatus("Failed to start the SyncTool.", InfoBarSeverity.Error);
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CancelSyncTool()
    {
        _runCts?.Cancel();
    }

    private void OnSyncOutput(string text)
    {
        _ = _dispatcher.TryEnqueue(() => AppendOutput(text));
    }

    private void AppendOutput(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _outputBuffer.AppendLine(text);
        }

        SyncOutputText = _outputBuffer.ToString();
    }

    partial void OnSelectedConnectionTargetChanged(Model_DatabaseTarget? value)
    {
        SelectedConnectionTargetSummary = value is null
            ? "No target selected."
            : $"{value.DatabaseName} — {value.Description}";
    }

    partial void OnSyncToolPathChanged(string value)
    {
        var executable = Path.Combine(
            Path.GetDirectoryName(value) ?? string.Empty,
            "sync_reference_data.exe"
        );
        IsBundledExeAvailable = File.Exists(executable);
        IsSyncToolAvailable = IsBundledExeAvailable || File.Exists(value);
    }

    partial void OnIsSyncToolAvailableChanged(bool value)
    {
        OnPropertyChanged(nameof(SyncToolAvailabilityText));
    }

    partial void OnIsBundledExeAvailableChanged(bool value)
    {
        OnPropertyChanged(nameof(SyncToolAvailabilityText));
    }

    partial void OnIsSyncToolBlockedChanged(bool value)
    {
        OnPropertyChanged(nameof(IsSyncToolReady));
        OnPropertyChanged(nameof(SyncToolAvailabilityText));
    }

    partial void OnSyncActionIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsGenerateSqlAction));
        OnPropertyChanged(nameof(IsExecuteAction));
        OnPropertyChanged(nameof(IsVerifyCopyAction));
        OnPropertyChanged(nameof(IsSyncTargetRelevant));
        OnPropertyChanged(nameof(IsConfirmCoreRequired));
        UpdateSyncActionCaption();
    }

    partial void OnSelectedSyncTargetChanged(Model_SyncTargetOption? value)
    {
        OnPropertyChanged(nameof(IsConfirmCoreRequired));
        OnPropertyChanged(nameof(IsSyncTargetRelevant));
        UpdateSyncActionCaption();
    }

    partial void OnConfirmCoreWriteChanged(bool value)
    {
        OnPropertyChanged(nameof(IsConfirmCoreRequired));
        UpdateSyncActionCaption();
    }

    private void UpdateSyncActionCaption()
    {
        var index = Math.Clamp(SyncActionIndex, 0, SyncActions.Length - 1);
        var caption = SyncActions[index].Label;
        if (IsConfirmCoreRequired && !ConfirmCoreWrite)
        {
            caption += " - confirmation required before it will run into core.";
        }

        SyncActionCaption = caption;
    }

    private void UpdatePendingState(string pendingDatabase)
    {
        PendingStateText = IsRestartPending
            ? $"A database change is saved but not yet applied — connect to '{pendingDatabase}' after restarting."
            : $"The application is connected to '{pendingDatabase}'.";
    }

    /// <summary>
    /// Updates the page-local status bar only. Settings pages deliberately avoid
    /// <c>ShowStatus</c> so the message is not duplicated by the application-wide
    /// notification banner hosted by the main window.
    /// </summary>
    private void SetStatus(
        string message,
        InfoBarSeverity severity = InfoBarSeverity.Informational
    )
    {
        StatusMessage = message;
        StatusSeverity = severity;
        IsStatusOpen = true;
    }
}
