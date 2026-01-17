using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Data;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.Data;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.Services;

/// <summary>
/// Handler to execute the settings DB diagnostics.
/// </summary>
public class RunSettingsDbTestCommandHandler : IRequestHandler<RunSettingsDbTestCommand, Model_Dao_Result<Model_SettingsDbTestReport>>
{
    private readonly Dao_SettingsDiagnostics _diagnosticsDao;
    private readonly Dao_SettingsCoreSystem _systemDao;
    private readonly Dao_SettingsCoreUser _userDao;
    private readonly Dao_SettingsCoreAudit _auditDao;

    public RunSettingsDbTestCommandHandler(
        Dao_SettingsDiagnostics diagnosticsDao,
        Dao_SettingsCoreSystem systemDao,
        Dao_SettingsCoreUser userDao,
        Dao_SettingsCoreAudit auditDao)
    {
        _diagnosticsDao = diagnosticsDao;
        _systemDao = systemDao;
        _userDao = userDao;
        _auditDao = auditDao;
    }

    public async Task<Model_Dao_Result<Model_SettingsDbTestReport>> Handle(RunSettingsDbTestCommand request, CancellationToken cancellationToken)
    {
        var report = new Model_SettingsDbTestReport
        {
            TotalTables = 5,
            TotalProcedures = 8,
            TotalDaos = 3
        };

        try
        {
            var stopwatch = Stopwatch.StartNew();
            await using var connection = new MySqlConnection(Helper_Database_Variables.GetConnectionString());
            await connection.OpenAsync(cancellationToken);
            stopwatch.Stop();

            report.IsConnected = true;
            report.ConnectionStatus = "Connected";
            report.ConnectionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            report.ServerVersion = connection.ServerVersion;
        }
        catch (Exception ex)
        {
            report.IsConnected = false;
            report.ConnectionStatus = $"Connection failed: {ex.Message}";
        }

        var tablesResult = await _diagnosticsDao.GetTablesAsync();
        var proceduresResult = await _diagnosticsDao.GetStoredProceduresAsync();

        var expectedTables = new[]
        {
            "settings_universal",
            "settings_personal",
            "settings_activity",
            "settings_roles",
            "settings_user_roles"
        };

        var expectedProcedures = new[]
        {
            "sp_SettingsCore_System_GetByKey",
            "sp_SettingsCore_System_GetByCategory",
            "sp_SettingsCore_System_Upsert",
            "sp_SettingsCore_System_Reset",
            "sp_SettingsCore_User_GetByKey",
            "sp_SettingsCore_User_GetByCategory",
            "sp_SettingsCore_User_Upsert",
            "sp_SettingsCore_User_Reset"
        };

        var tableSet = tablesResult.Data?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new();
        foreach (var table in expectedTables)
        {
            var exists = tableSet.Contains(table);
            report.TableResults.Add(new Model_SettingsDbTableResult
            {
                TableName = table,
                IsValid = exists,
                Details = exists ? "Found" : "Missing"
            });
        }
        report.TablesValidated = report.TableResults.Count(r => r.IsValid);

        var procSet = proceduresResult.Data?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new();
        foreach (var proc in expectedProcedures)
        {
            var exists = procSet.Contains(proc);
            report.ProcedureResults.Add(new Model_SettingsDbProcedureResult
            {
                ProcedureName = proc,
                IsValid = exists,
                Details = exists ? "Found" : "Missing"
            });
        }
        report.ProceduresValidated = report.ProcedureResults.Count(r => r.IsValid);

        var systemDaoResult = await _systemDao.GetByCategoryAsync("System");
        report.DaoResults.Add(new Model_SettingsDbDaoResult
        {
            DaoName = nameof(Dao_SettingsCoreSystem),
            IsValid = systemDaoResult.Success,
            Details = systemDaoResult.Success ? "Operational" : systemDaoResult.ErrorMessage
        });

        var userDaoResult = await _userDao.GetByCategoryAsync(1, "User");
        report.DaoResults.Add(new Model_SettingsDbDaoResult
        {
            DaoName = nameof(Dao_SettingsCoreUser),
            IsValid = userDaoResult.Success,
            Details = userDaoResult.Success ? "Operational" : userDaoResult.ErrorMessage
        });

        var auditDaoResult = await _auditDao.GetByUserAsync(1);
        report.DaoResults.Add(new Model_SettingsDbDaoResult
        {
            DaoName = nameof(Dao_SettingsCoreAudit),
            IsValid = auditDaoResult.Success,
            Details = auditDaoResult.Success ? "Operational" : auditDaoResult.ErrorMessage
        });

        report.DaosValidated = report.DaoResults.Count(r => r.IsValid);

        return Model_Dao_Result_Factory.Success(report);
    }
}
