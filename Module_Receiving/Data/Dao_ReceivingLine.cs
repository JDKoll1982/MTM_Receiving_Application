using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data Access Object for receiving_label_data table
/// Provides CRUD operations using stored procedures
/// </summary>
public class Dao_ReceivingLine
{
    private readonly string _connectionString;

    public Dao_ReceivingLine(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Inserts a new receiving line record into the database
    /// </summary>
    /// <param name="line">ReceivingLine model to insert</param>
    /// <returns>Model_Dao_Result with success status and affected rows</returns>
    public async Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line)
    {
        try
        {
            // Prepare stored procedure parameters
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_Quantity", line.Quantity),
                new MySqlParameter("@p_PartID", line.PartID ?? string.Empty),
                new MySqlParameter("@p_PONumber", line.PONumber),
                new MySqlParameter("@p_EmployeeNumber", line.EmployeeNumber),
                new MySqlParameter("@p_Heat", line.Heat ?? string.Empty),
                new MySqlParameter("@p_Date", line.Date),
                new MySqlParameter("@p_InitialLocation", line.InitialLocation ?? string.Empty),
                new MySqlParameter("@p_CoilsOnSkid", (object?)line.CoilsOnSkid ?? DBNull.Value),
                new MySqlParameter("@p_VendorName", line.VendorName ?? "Unknown"),
                new MySqlParameter("@p_PartDescription", line.PartDescription ?? string.Empty),
                new MySqlParameter("@p_Status", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output },
                new MySqlParameter("@p_ErrorMsg", MySqlDbType.VarChar, 500) { Direction = System.Data.ParameterDirection.Output }
            };

            // Validate parameters
            if (!Helper_Database_StoredProcedure.ValidateParameters(parameters))
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = "Required parameters are missing or invalid",
                    Severity = Enum_ErrorSeverity.Warning
                };
            }

            // Execute stored procedure
            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "receiving_line_Insert",
                parameters,
                _connectionString
            );

            return result;
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Unexpected error inserting receiving line: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
            };
        }
    }
}
