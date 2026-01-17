using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.Models;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.ViewModels;

/// <summary>
/// ViewModel for the Settings Developer Tools database test.
/// </summary>
public partial class ViewModel_SettingsDeveloperTools_DatabaseTest : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectionStatus = "Not Connected";

    [ObservableProperty]
    private int _connectionTimeMs;

    [ObservableProperty]
    private string _serverVersion = string.Empty;

    [ObservableProperty]
    private int _tablesValidated;

    [ObservableProperty]
    private int _totalTables;

    [ObservableProperty]
    private int _proceduresValidated;

    [ObservableProperty]
    private int _totalProcedures;

    [ObservableProperty]
    private int _daosValidated;

    [ObservableProperty]
    private int _totalDaos;

    [ObservableProperty]
    private ObservableCollection<Model_SettingsDbTableResult> _tableResults = new();

    [ObservableProperty]
    private ObservableCollection<Model_SettingsDbProcedureResult> _procedureResults = new();

    [ObservableProperty]
    private ObservableCollection<Model_SettingsDbDaoResult> _daoResults = new();

    public ViewModel_SettingsDeveloperTools_DatabaseTest(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _mediator = mediator;
        Title = "Settings DB Test";
    }

    [RelayCommand]
    private async Task RunAllTestsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _mediator.Send(new RunSettingsDbTestCommand());
            if (!result.Success || result.Data == null)
            {
                ConnectionStatus = result.ErrorMessage ?? "Failed";
                return;
            }

            var report = result.Data;
            IsConnected = report.IsConnected;
            ConnectionStatus = report.ConnectionStatus;
            ConnectionTimeMs = report.ConnectionTimeMs;
            ServerVersion = report.ServerVersion;
            TablesValidated = report.TablesValidated;
            TotalTables = report.TotalTables;
            ProceduresValidated = report.ProceduresValidated;
            TotalProcedures = report.TotalProcedures;
            DaosValidated = report.DaosValidated;
            TotalDaos = report.TotalDaos;

            TableResults = new ObservableCollection<Model_SettingsDbTableResult>(report.TableResults);
            ProcedureResults = new ObservableCollection<Model_SettingsDbProcedureResult>(report.ProcedureResults);
            DaoResults = new ObservableCollection<Model_SettingsDbDaoResult>(report.DaoResults);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(RunAllTestsAsync), nameof(ViewModel_SettingsDeveloperTools_DatabaseTest));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
