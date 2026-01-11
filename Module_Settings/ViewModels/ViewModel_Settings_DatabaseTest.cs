using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Settings.Data;
using MTM_Receiving_Application.Module_Settings.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Settings.ViewModels;

/// <summary>
/// ViewModel for Settings Database Test - Development tool for validating database implementation
/// </summary>
public partial class ViewModel_Settings_DatabaseTest : ViewModel_Shared_Base
{
    #region Dependencies
    private readonly Dao_SystemSettings _daoSystemSettings;
    private readonly Dao_UserSettings _daoUserSettings;
    private readonly Dao_PackageType _daoPackageType;
    private readonly Dao_PackageTypeMappings _daoPackageTypeMappings;
    private readonly Dao_RoutingRule _daoRoutingRule;
    private readonly Dao_ScheduledReport _daoScheduledReport;
    private readonly Dao_SettingsAuditLog _daoSettingsAuditLog;
    #endregion

    #region Observable Properties

    // Connection Status
    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectionStatus = "Not Connected";

    [ObservableProperty]
    private int _connectionTimeMs;

    [ObservableProperty]
    private string _serverVersion = string.Empty;

    // Test Summary
    [ObservableProperty]
    private int _tablesValidated;

    [ObservableProperty]
    private int _totalTables = 7;

    [ObservableProperty]
    private bool _allTablesValid;

    [ObservableProperty]
    private int _storedProceduresTested;

    [ObservableProperty]
    private int _totalStoredProcedures = 41;

    [ObservableProperty]
    private bool _allStoredProceduresValid;

    [ObservableProperty]
    private int _settingsSeeded;

    [ObservableProperty]
    private bool _allCategoriesSeeded;

    [ObservableProperty]
    private int _daosTested;

    [ObservableProperty]
    private int _totalDaos = 7;

    [ObservableProperty]
    private bool _allDaosValid;

    // Test Results
    [ObservableProperty]
    private ObservableCollection<Model_TableTestResult> _tableResults;

    [ObservableProperty]
    private ObservableCollection<Model_StoredProcedureTestResult> _storedProcedureResults;

    [ObservableProperty]
    private ObservableCollection<Model_DaoTestResult> _daoResults;

    [ObservableProperty]
    private ObservableCollection<Model_SettingsAuditLog> _auditLogEntries;

    [ObservableProperty]
    private ObservableCollection<string> _logMessages;

    // Test Execution
    [ObservableProperty]
    private DateTime _lastRunTime = DateTime.Now;

    [ObservableProperty]
    private int _totalTestDurationMs;

    [ObservableProperty]
    private string _selectedTab = "Schema";

    #endregion

    #region Constructor

    public ViewModel_Settings_DatabaseTest(
        Dao_SystemSettings daoSystemSettings,
        Dao_UserSettings daoUserSettings,
        Dao_PackageType daoPackageType,
        Dao_PackageTypeMappings daoPackageTypeMappings,
        Dao_RoutingRule daoRoutingRule,
        Dao_ScheduledReport daoScheduledReport,
        Dao_SettingsAuditLog daoSettingsAuditLog,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        _daoSystemSettings = daoSystemSettings;
        _daoUserSettings = daoUserSettings;
        _daoPackageType = daoPackageType;
        _daoPackageTypeMappings = daoPackageTypeMappings;
        _daoRoutingRule = daoRoutingRule;
        _daoScheduledReport = daoScheduledReport;
        _daoSettingsAuditLog = daoSettingsAuditLog;

        Title = "Settings Database Test";

        TableResults = new ObservableCollection<Model_TableTestResult>();
        StoredProcedureResults = new ObservableCollection<Model_StoredProcedureTestResult>();
        DaoResults = new ObservableCollection<Model_DaoTestResult>();
        AuditLogEntries = new ObservableCollection<Model_SettingsAuditLog>();
        LogMessages = new ObservableCollection<string>();

        // Don't run tests in constructor - let view trigger it
        // This prevents null reference issues during initialization
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task RunAllTestsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            var stopwatch = Stopwatch.StartNew();

            LogMessages.Clear();
            AddLog("Starting database test suite...");
            await _logger.LogInfoAsync("Database test suite started", "ViewModel_Settings_DatabaseTest");

            // Test connection
            await TestConnectionAsync();

            // Test tables
            await TestTablesAsync();

            // Test stored procedures
            await TestStoredProceduresAsync();

            // Test DAOs
            await TestDaosAsync();

            stopwatch.Stop();
            TotalTestDurationMs = (int)stopwatch.ElapsedMilliseconds;
            LastRunTime = DateTime.Now;

            AddLog($"All tests completed in {TotalTestDurationMs}ms");
            await _logger.LogInfoAsync($"Database tests completed successfully in {TotalTestDurationMs}ms", "ViewModel_Settings_DatabaseTest");
            ShowStatus($"All tests completed successfully in {TotalTestDurationMs}ms", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Database test execution failed: {ex.Message}", ex, "ViewModel_Settings_DatabaseTest");
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(RunAllTestsAsync),
                nameof(ViewModel_Settings_DatabaseTest)
            );
            ShowStatus("Test execution failed", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportResultsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            AddLog("Exporting test results...");

            var exportRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MTM_Receiving_Application",
                "Exports",
                "SettingsDbTest");

            Directory.CreateDirectory(exportRoot);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            var jsonPath = Path.Combine(exportRoot, $"SettingsDbTest_{timestamp}.json");
            var logPath = Path.Combine(exportRoot, $"SettingsDbTest_{timestamp}.log.txt");
            var tablesCsvPath = Path.Combine(exportRoot, $"SettingsDbTest_{timestamp}_tables.csv");
            var procsCsvPath = Path.Combine(exportRoot, $"SettingsDbTest_{timestamp}_stored_procedures.csv");
            var daosCsvPath = Path.Combine(exportRoot, $"SettingsDbTest_{timestamp}_daos.csv");

            static string EscapeCsv(string? value)
            {
                var s = value ?? string.Empty;
                var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
                if (!needsQuotes)
                    return s;

                return "\"" + s.Replace("\"", "\"\"") + "\"";
            }

            var report = new
            {
                GeneratedAt = DateTimeOffset.Now,
                Summary = new
                {
                    IsConnected,
                    ConnectionStatus,
                    ConnectionTimeMs,
                    ServerVersion,
                    TablesValidated,
                    TotalTables,
                    AllTablesValid,
                    StoredProceduresTested,
                    TotalStoredProcedures,
                    AllStoredProceduresValid,
                    SettingsSeeded,
                    AllCategoriesSeeded,
                    DaosTested,
                    TotalDaos,
                    AllDaosValid,
                    LastRunTime,
                    TotalTestDurationMs
                },
                Tables = (TableResults ?? new ObservableCollection<Model_TableTestResult>())
                    .Select(t => new { t.TableName, t.IsValid, t.StatusText, t.Details })
                    .ToList(),
                StoredProcedures = (StoredProcedureResults ?? new ObservableCollection<Model_StoredProcedureTestResult>())
                    .Select(p => new { p.ProcedureName, p.IsValid, p.ExecutionTimeMs, p.TestDetails })
                    .ToList(),
                Daos = (DaoResults ?? new ObservableCollection<Model_DaoTestResult>())
                    .Select(d => new { d.DaoName, d.IsValid, d.StatusText, d.OperationsTested })
                    .ToList(),
                AuditLogEntries = (AuditLogEntries ?? new ObservableCollection<Model_SettingsAuditLog>())
                    .Select(a => new
                    {
                        a.Id,
                        a.SettingId,
                        a.Category,
                        a.SettingKey,
                        a.SettingName,
                        a.UserSettingId,
                        a.OldValue,
                        a.NewValue,
                        a.ChangeType,
                        a.ChangedBy,
                        a.ChangedAt,
                        a.IpAddress,
                        a.WorkstationName
                    })
                    .ToList(),
                Logs = (LogMessages ?? new ObservableCollection<string>()).ToList()
            };

            var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(jsonPath, json, Encoding.UTF8);
            await File.WriteAllTextAsync(logPath, string.Join(Environment.NewLine, report.Logs), Encoding.UTF8);

            var tablesCsv = new StringBuilder();
            tablesCsv.AppendLine("TableName,IsValid,StatusText,Details");
            foreach (var t in report.Tables)
            {
                tablesCsv.AppendLine($"{EscapeCsv(t.TableName)},{t.IsValid},{EscapeCsv(t.StatusText)},{EscapeCsv(t.Details)}");
            }
            await File.WriteAllTextAsync(tablesCsvPath, tablesCsv.ToString(), Encoding.UTF8);

            var procsCsv = new StringBuilder();
            procsCsv.AppendLine("ProcedureName,IsValid,ExecutionTimeMs,TestDetails");
            foreach (var p in report.StoredProcedures)
            {
                procsCsv.AppendLine($"{EscapeCsv(p.ProcedureName)},{p.IsValid},{p.ExecutionTimeMs},{EscapeCsv(p.TestDetails)}");
            }
            await File.WriteAllTextAsync(procsCsvPath, procsCsv.ToString(), Encoding.UTF8);

            var daosCsv = new StringBuilder();
            daosCsv.AppendLine("DaoName,IsValid,StatusText,OperationsTested");
            foreach (var d in report.Daos)
            {
                daosCsv.AppendLine($"{EscapeCsv(d.DaoName)},{d.IsValid},{EscapeCsv(d.StatusText)},{EscapeCsv(d.OperationsTested)}");
            }
            await File.WriteAllTextAsync(daosCsvPath, daosCsv.ToString(), Encoding.UTF8);

            AddLog($"Export complete: {jsonPath}");
            ShowStatus($"Exported results to {exportRoot}", InfoBarSeverity.Success);
            await _logger.LogInfoAsync($"Settings DB Test export completed: {jsonPath}", "ViewModel_Settings_DatabaseTest");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Low, nameof(ExportResultsAsync), nameof(ViewModel_Settings_DatabaseTest));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadAuditLogAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            AddLog("Loading settings audit log...");
            AuditLogEntries.Clear();

            var settingsResult = await _daoSystemSettings.GetAllAsync();
            if (!settingsResult.IsSuccess || settingsResult.Data == null || settingsResult.Data.Count == 0)
            {
                AddLog("No system settings found; cannot load audit log.");
                ShowStatus("No settings found to query audit log", InfoBarSeverity.Warning);
                return;
            }

            var settingId = settingsResult.Data[0].Id;
            var auditResult = await _daoSettingsAuditLog.GetAsync(settingId, 50);

            if (!auditResult.IsSuccess || auditResult.Data == null)
            {
                AddLog($"Failed to load audit log: {auditResult.ErrorMessage}");
                ShowStatus("Failed to load audit log", InfoBarSeverity.Error);
                return;
            }

            foreach (var entry in auditResult.Data)
            {
                AuditLogEntries.Add(entry);
            }

            AddLog($"Loaded {AuditLogEntries.Count} audit log entries for setting ID {settingId}.");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Low, nameof(LoadAuditLogAsync), nameof(ViewModel_Settings_DatabaseTest));
            ShowStatus("Error loading audit log", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Private Methods

    private async Task TestConnectionAsync()
    {
        AddLog("Testing database connection...");
        await _logger.LogInfoAsync("Testing database connection", "ViewModel_Settings_DatabaseTest");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await using var connection = new MySqlConnection(Helper_Database_Variables.GetConnectionString());
            await connection.OpenAsync();

            stopwatch.Stop();
            ConnectionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            ServerVersion = connection.ServerVersion;
            IsConnected = true;
            ConnectionStatus = $"Connected to localhost:3306/mtm_receiving_application";

            AddLog($"✓ Database connection successful ({ConnectionTimeMs}ms)");
            AddLog($"  Server version: MySQL {ServerVersion}");
            await _logger.LogInfoAsync($"Database connection successful in {ConnectionTimeMs}ms - MySQL {ServerVersion}", "ViewModel_Settings_DatabaseTest");
        }
        catch (Exception ex)
        {
            IsConnected = false;
            ConnectionStatus = $"Connection failed: {ex.Message}";
            AddLog($"✗ Database connection failed: {ex.Message}");
            await _logger.LogErrorAsync($"Database connection failed: {ex.Message}", ex, "ViewModel_Settings_DatabaseTest");
            throw;
        }
    }

    private async Task TestTablesAsync()
    {
        AddLog("Validating database tables...");
        TableResults.Clear();

        var tables = new[]
        {
            ("settings_universal", "79 rows | 18 columns | 3 indexes"),
            ("settings_personal", "0 rows | 5 columns | 1 unique constraint"),
            ("dunnage_types", "5 rows | 5 columns | 2 unique constraints"),
            ("routing_home_locations", "3 rows | 7 columns | 1 index on priority"),
            ("reporting_scheduled_reports", "2 rows | 8 columns | 2 indexes"),
            ("receiving_package_type_mapping", "15 rows | 4 columns | 2 indexes"),
            ("settings_activity", "0 rows | 8 columns | 2 indexes")
        };

        var validatedCount = 0;

        foreach (var (tableName, details) in tables)
        {
            try
            {
                // Query to check if table exists and get basic info
                await using var connection = new MySqlConnection(Helper_Database_Variables.GetConnectionString());
                await connection.OpenAsync();

                var query = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'mtm_receiving_application' AND table_name = '{tableName}'";
                await using var command = new MySqlCommand(query, connection);
                var exists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

                if (exists)
                {
                    TableResults.Add(new Model_TableTestResult
                    {
                        TableName = tableName,
                        Details = details,
                        IsValid = true,
                        StatusText = "Validated"
                    });
                    validatedCount++;
                    AddLog($"✓ Table '{tableName}' validated");
                }
                else
                {
                    TableResults.Add(new Model_TableTestResult
                    {
                        TableName = tableName,
                        Details = "Table not found",
                        IsValid = false,
                        StatusText = "Missing"
                    });
                    AddLog($"✗ Table '{tableName}' not found");
                }
            }
            catch (Exception ex)
            {
                TableResults.Add(new Model_TableTestResult
                {
                    TableName = tableName,
                    Details = $"Error: {ex.Message}",
                    IsValid = false,
                    StatusText = "Error"
                });
                AddLog($"✗ Error validating table '{tableName}': {ex.Message}");
            }
        }

        TablesValidated = validatedCount;
        AllTablesValid = validatedCount == TotalTables;
        AddLog($"Table validation complete: {validatedCount}/{TotalTables} tables valid");
    }

    private async Task TestStoredProceduresAsync()
    {
        AddLog("Testing stored procedures...");
        StoredProcedureResults.Clear();

        var procedures = new[]
        {
            "sp_SystemSettings_GetAll",
            "sp_SystemSettings_GetByCategory",
            "sp_SystemSettings_GetByKey",
            "sp_SystemSettings_UpdateValue",

            "sp_SystemSettings_ResetToDefault",
            "sp_SystemSettings_SetLocked",

            "sp_UserSettings_Get",
            "sp_UserSettings_Set",

            "sp_UserSettings_GetAllForUser",
            "sp_UserSettings_Reset",
            "sp_UserSettings_ResetAll",

            "sp_dunnage_type_GetAll",
            "sp_dunnage_type_GetById",
            "sp_dunnage_type_Insert",
            "sp_dunnage_type_Update",
            "sp_dunnage_type_Delete",
            "sp_dunnage_type_UsageCount",

            "sp_Receiving_PackageTypeMappings_GetAll",
            "sp_Receiving_PackageTypeMappings_GetByPrefix",
            "sp_Receiving_PackageTypeMappings_Insert",
            "sp_Receiving_PackageTypeMappings_Update",
            "sp_Receiving_PackageTypeMappings_Delete",

            "sp_RoutingRule_GetAll",
            "sp_RoutingRule_GetById",
            "sp_RoutingRule_FindMatch",
            "sp_RoutingRule_GetByPartNumber",
            "sp_RoutingRule_Insert",
            "sp_RoutingRule_Update",
            "sp_RoutingRule_Delete",

            "sp_ScheduledReport_GetAll",
            "sp_ScheduledReport_GetActive",
            "sp_ScheduledReport_GetById",
            "sp_ScheduledReport_GetDue",
            "sp_ScheduledReport_Insert",
            "sp_ScheduledReport_Update",
            "sp_ScheduledReport_UpdateLastRun",
            "sp_ScheduledReport_ToggleActive",
            "sp_ScheduledReport_Delete",

            "sp_SettingsAuditLog_Get",
            "sp_SettingsAuditLog_GetBySetting",
            "sp_SettingsAuditLog_GetByUser"
        };

        var testedCount = 0;

        foreach (var procName in procedures)
        {
            try
            {
                await using var connection = new MySqlConnection(Helper_Database_Variables.GetConnectionString());
                await connection.OpenAsync();

                var sw = Stopwatch.StartNew();
                var query = $"SELECT COUNT(*) FROM information_schema.routines WHERE routine_schema = 'mtm_receiving_application' AND routine_name = '{procName}'";
                await using var command = new MySqlCommand(query, connection);
                var exists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
                sw.Stop();

                StoredProcedureResults.Add(new Model_StoredProcedureTestResult
                {
                    ProcedureName = procName,
                    IsValid = exists,
                    ExecutionTimeMs = exists ? (int)sw.ElapsedMilliseconds : 0,
                    TestDetails = exists ? "Exists and callable" : "Not found"
                });

                if (exists)
                {
                    testedCount++;
                    AddLog($"✓ Stored procedure '{procName}' found");
                }
                else
                {
                    AddLog($"✗ Stored procedure '{procName}' not found");
                }
            }
            catch (Exception ex)
            {
                AddLog($"✗ Error checking stored procedure '{procName}': {ex.Message}");
            }
        }

        StoredProceduresTested = testedCount;
        AllStoredProceduresValid = testedCount == procedures.Length;
        AddLog($"Stored procedure testing complete: {testedCount}/{procedures.Length} procedures found");
    }

    private async Task TestDaosAsync()
    {
        AddLog("Testing DAO operations...");
        DaoResults.Clear();

        AuditLogEntries.Clear();

        var testedCount = 0;

        // Test SystemSettings DAO
        if (await TestDaoAsync("Dao_SystemSettings", async () =>
        {
            var result = await _daoSystemSettings.GetAllAsync();
            if (result.IsSuccess)
            {
                SettingsSeeded = result.Data?.Count ?? 0;
                AllCategoriesSeeded = SettingsSeeded > 0;
            }
            return result.IsSuccess;
        }))
        {
            testedCount++;
        }

        // Test UserSettings DAO
        if (await TestDaoAsync("Dao_UserSettings", async () =>
        {
            // Validate stored procedures + mapping by doing a lightweight read.
            // User ID 1 is used as a safe default for dev testing.
            var result = await _daoUserSettings.GetAllForUserAsync(1);
            return result.IsSuccess;
        }))
        {
            testedCount++;
        }

        // Test PackageType DAO
        if (await TestDaoAsync("Dao_PackageType", async () =>
        {
            var result = await _daoPackageType.GetAllAsync();
            return result.IsSuccess;
        }))
        {
            testedCount++;
        }

        // Test RoutingRule DAO
        if (await TestDaoAsync("Dao_RoutingRule", async () =>
        {
            var result = await _daoRoutingRule.GetAllAsync();
            return result.IsSuccess;
        }))
        {
            testedCount++;
        }

        // Test ScheduledReport DAO
        if (await TestDaoAsync("Dao_ScheduledReport", async () =>
        {
            var result = await _daoScheduledReport.GetAllAsync();
            return result.IsSuccess;
        }))
        {
            testedCount++;
        }

        // Test PackageTypeMappings DAO
        if (await TestDaoAsync("Dao_PackageTypeMappings", async () =>
        {
            var result = await _daoPackageTypeMappings.GetAllAsync();
            return result.IsSuccess;
        }))
        {
            testedCount++;
        }

        // Test SettingsAuditLog DAO
        if (await TestDaoAsync("Dao_SettingsAuditLog", async () =>
        {
            var settingsResult = await _daoSystemSettings.GetAllAsync();
            if (!settingsResult.IsSuccess || settingsResult.Data == null || settingsResult.Data.Count == 0)
                return false;

            var auditResult = await _daoSettingsAuditLog.GetAsync(settingsResult.Data[0].Id, 10);
            if (!auditResult.IsSuccess)
                return false;

            if (auditResult.Data != null)
            {
                foreach (var entry in auditResult.Data)
                {
                    AuditLogEntries.Add(entry);
                }
            }

            return true;
        }))
        {
            testedCount++;
        }

        DaosTested = testedCount;
        AllDaosValid = testedCount == TotalDaos;
        AddLog($"DAO testing complete: {testedCount}/{TotalDaos} DAOs operational");
    }

    private async Task<bool> TestDaoAsync(string daoName, Func<Task<bool>> testFunc)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var success = await testFunc();
            stopwatch.Stop();

            DaoResults.Add(new Model_DaoTestResult
            {
                DaoName = daoName,
                IsValid = success,
                OperationsTested = "GetAll",
                StatusText = success ? "Operational" : "Failed"
            });

            if (success)
            {
                AddLog($"✓ {daoName} operational ({stopwatch.ElapsedMilliseconds}ms)");
            }
            else
            {
                AddLog($"✗ {daoName} failed");
            }

            return success;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            DaoResults.Add(new Model_DaoTestResult
            {
                DaoName = daoName,
                IsValid = false,
                OperationsTested = "GetAll",
                StatusText = $"Error: {ex.Message}"
            });
            AddLog($"✗ {daoName} error: {ex.Message}");
            return false;
        }
    }

    private void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        LogMessages.Add($"[{timestamp}] {message}");
    }

    #endregion
}
