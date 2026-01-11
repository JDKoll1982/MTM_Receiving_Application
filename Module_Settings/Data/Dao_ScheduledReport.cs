using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Models;

namespace MTM_Receiving_Application.Module_Settings.Data;

/// <summary>
/// Data Access Object for scheduled reports CRUD operations
/// Instance-based pattern with connection string injection
/// </summary>
public class Dao_ScheduledReport
{
    private readonly string _connectionString;

    public Dao_ScheduledReport(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Retrieves all active scheduled reports
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_ScheduledReport>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Settings_ScheduledReport_GetAll",
            MapFromReader
        );
    }

    /// <summary>
    /// Retrieves a scheduled report by ID
    /// </summary>
    /// <param name="id"></param>
    public async Task<Model_Dao_Result<Model_ScheduledReport>> GetByIdAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", id }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Settings_ScheduledReport_GetById",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Retrieves scheduled reports that are due for execution
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_ScheduledReport>>> GetDueAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Settings_ScheduledReport_GetDue",
            MapFromReader
        );
    }

    /// <summary>
    /// Retrieves active scheduled reports that have a next run date (spec compatibility)
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_ScheduledReport>>> GetActiveAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Settings_ScheduledReport_GetActive",
            MapFromReader
        );
    }

    /// <summary>
    /// Inserts a new scheduled report
    /// </summary>
    /// <param name="report"></param>
    /// <param name="createdBy"></param>
    public async Task<Model_Dao_Result<int>> InsertAsync(Model_ScheduledReport report, int createdBy)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_report_type", report.ReportType },
            { "p_schedule", report.Schedule },
            { "p_email_recipients", report.EmailRecipients ?? string.Empty },
            { "p_next_run_date", report.NextRunDate ?? (object)DBNull.Value },
            { "p_created_by", createdBy }
        };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Settings_ScheduledReport_Insert",
            reader => reader.GetInt32(reader.GetOrdinal("id")),
            parameters
        );
    }

    /// <summary>
    /// Updates an existing scheduled report
    /// </summary>
    /// <param name="report"></param>
    public async Task<Model_Dao_Result> UpdateAsync(Model_ScheduledReport report)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", report.Id },
            { "p_report_type", report.ReportType },
            { "p_schedule", report.Schedule },
            { "p_email_recipients", report.EmailRecipients ?? string.Empty },
            { "p_next_run_date", report.NextRunDate ?? (object)DBNull.Value }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_ScheduledReport_Update",
            parameters
        );
    }

    /// <summary>
    /// Updates the last run date and calculates next run date
    /// </summary>
    /// <param name="id"></param>
    /// <param name="lastRunDate"></param>
    /// <param name="nextRunDate"></param>
    public async Task<Model_Dao_Result> UpdateLastRunAsync(int id, DateTime lastRunDate, DateTime nextRunDate)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", id },
            { "p_last_run_date", lastRunDate },
            { "p_next_run_date", nextRunDate }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_ScheduledReport_UpdateLastRun",
            parameters
        );
    }

    /// <summary>
    /// Deletes a scheduled report (soft delete)
    /// </summary>
    /// <param name="id"></param>
    public async Task<Model_Dao_Result> DeleteAsync(int id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", id }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_ScheduledReport_Delete",
            parameters
        );
    }

    /// <summary>
    /// Toggles a scheduled report's active status
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isActive"></param>
    public async Task<Model_Dao_Result> ToggleActiveAsync(int id, bool isActive)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_id", id },
            { "p_is_active", isActive }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_ScheduledReport_ToggleActive",
            parameters
        );
    }

    /// <summary>
    /// Maps database reader to Model_ScheduledReport
    /// </summary>
    /// <param name="reader"></param>
    private static Model_ScheduledReport MapFromReader(IDataReader reader)
    {
        return new Model_ScheduledReport
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            ReportType = reader.GetString(reader.GetOrdinal("report_type")),
            Schedule = reader.GetString(reader.GetOrdinal("schedule")),
            EmailRecipients = reader.IsDBNull(reader.GetOrdinal("email_recipients")) ? null : reader.GetString(reader.GetOrdinal("email_recipients")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
            NextRunDate = reader.IsDBNull(reader.GetOrdinal("next_run_date")) ? null : reader.GetDateTime(reader.GetOrdinal("next_run_date")),
            LastRunDate = reader.IsDBNull(reader.GetOrdinal("last_run_date")) ? null : reader.GetDateTime(reader.GetOrdinal("last_run_date")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) ? null : reader.GetInt32(reader.GetOrdinal("created_by"))
        };
    }
}
