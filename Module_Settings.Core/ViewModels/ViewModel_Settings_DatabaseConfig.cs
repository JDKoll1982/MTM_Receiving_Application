using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for the Database Config page. Lets a developer point this PC at another MySQL
/// database on the next launch. The choice is written to the machine-local
/// <c>appsettings.local.json</c> override, so it never changes the shared configuration or
/// another computer.
/// </summary>
public partial class ViewModel_Settings_DatabaseConfig : ViewModel_Shared_Base
{
    private readonly IService_DatabaseTargetManager _targetManager;

    public ObservableCollection<Model_DatabaseTarget> ConnectionTargets { get; } = new();

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
    private string _databaseListStatusText = string.Empty;

    public ViewModel_Settings_DatabaseConfig(
        IService_DatabaseTargetManager targetManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _targetManager =
            targetManager ?? throw new ArgumentNullException(nameof(targetManager));

        _ = LoadAsync();
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

            await RefreshDatabaseTargetsAsync();

            var pendingDatabase = await _targetManager.GetPendingDatabaseNameAsync();
            IsRestartPending = await _targetManager.GetIsRestartPendingAsync();
            UpdatePendingState(pendingDatabase);

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

    /// <summary>
    /// Fills the picker with the targets that actually exist on the configured server, and selects
    /// the database this PC is running against so the picker never misrepresents the current state.
    /// </summary>
    private async Task RefreshDatabaseTargetsAsync()
    {
        var earlierSelection = SelectedConnectionTarget;
        var enumeration = await _targetManager.EnumerateDatabasesAsync();

        CurrentDatabaseDisplay = _targetManager.CurrentDatabaseName;
        CurrentServerDisplay = _targetManager.CurrentServerDisplay;

        ConnectionTargets.Clear();
        if (enumeration.Success)
        {
            var present = new HashSet<string>(
                enumeration.Databases,
                StringComparer.OrdinalIgnoreCase
            );
            var missing = new List<string>();

            foreach (var target in _targetManager.Targets)
            {
                if (present.Contains(target.DatabaseName))
                {
                    ConnectionTargets.Add(target);
                }
                else
                {
                    missing.Add(target.DatabaseName);
                }
            }

            DatabaseListStatusText = ConnectionTargets.Count == 0
                ? $"Neither {string.Join(" nor ", _targetManager.Targets.Select(target => target.DatabaseName))} "
                  + $"was found on {CurrentServerDisplay}. Create it, then press Refresh DB List."
                : missing.Count == 0
                    ? $"Both databases are present on {CurrentServerDisplay}."
                    : $"{string.Join(", ", missing)} is not on {CurrentServerDisplay}, so it is not offered.";
        }
        else
        {
            foreach (var target in _targetManager.Targets)
            {
                ConnectionTargets.Add(target);
            }

            DatabaseListStatusText =
                $"Could not reach {CurrentServerDisplay} to list databases ({enumeration.Message}). "
                + "Showing both known databases - use Test Connection to confirm which one answers.";
        }

        SelectedConnectionTarget = ResolveSelection(earlierSelection);
    }

    /// <summary>
    /// Keeps whatever the operator changed in the picker, otherwise selects the database this PC is
    /// currently connected to.
    /// </summary>
    private Model_DatabaseTarget? ResolveSelection(Model_DatabaseTarget? earlierSelection)
    {
        if (earlierSelection is not null)
        {
            var stillOffered = ConnectionTargets.FirstOrDefault(target =>
                string.Equals(target.Id, earlierSelection.Id, StringComparison.Ordinal)
            );
            if (stillOffered is not null)
            {
                return stillOffered;
            }
        }

        var currentDatabase = _targetManager.CurrentDatabaseName;
        return ConnectionTargets.FirstOrDefault(target =>
                   string.Equals(
                       target.DatabaseName,
                       currentDatabase,
                       StringComparison.OrdinalIgnoreCase
                   )
               )
               ?? ConnectionTargets.FirstOrDefault();
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
                    ? $"Saved on this PC only. Restart the application to connect to '{pendingDatabase}'."
                    : $"Saved. '{pendingDatabase}' is already the active database on this PC.",
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

    partial void OnSelectedConnectionTargetChanged(Model_DatabaseTarget? value)
    {
        SelectedConnectionTargetSummary = value is null
            ? "No target selected."
            : $"{value.DatabaseName} — {value.Description}";
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
