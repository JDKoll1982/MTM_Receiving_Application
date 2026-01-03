using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.DunnageModule.Models;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.Data.Receiving;

/// <summary>
/// Data Access Object for label_table_dunnage table
/// Provides CRUD operations using stored procedures
/// </summary>
public static class Dao_DunnageLine
{
    private static IService_ErrorHandler? _errorHandler;

    /// <summary>
    /// Sets the error handler service (dependency injection)
    /// </summary>
    /// <param name="errorHandler"></param>
    public static void SetErrorHandler(IService_ErrorHandler errorHandler)
    {
        _errorHandler = errorHandler;
    }

    /// <summary>
    /// Inserts a new dunnage line record into the database
    /// </summary>
    /// <param name="line">DunnageLine model to insert</param>
    /// <returns>Model_Dao_Result with success status and affected rows</returns>
    public static async Task<Model_Dao_Result> InsertDunnageLineAsync(Model_DunnageLine line)
    {
        try
        {
            // Get connection string
            string connectionString = Helper_Database_Variables.GetConnectionString(useProduction: true);

            // Prepare stored procedure parameters
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_Line1", line.Line1 ?? string.Empty),
                new MySqlParameter("@p_Line2", line.Line2 ?? string.Empty),
                new MySqlParameter("@p_PONumber", line.PONumber),
                new MySqlParameter("@p_Date", line.Date),
                new MySqlParameter("@p_EmployeeNumber", line.EmployeeNumber),
                new MySqlParameter("@p_VendorName", line.VendorName ?? "Unknown"),
                new MySqlParameter("@p_Location", line.Location ?? string.Empty),
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
                    Severity = Models.Enums.Enum_ErrorSeverity.Warning
                };
            }

            // Execute stored procedure
            var result = await Helper_Database_StoredProcedure.ExecuteAsync(
                "dunnage_line_Insert",
                parameters,
                connectionString
            );

            return result;
        }
        catch (Exception ex)
        {
            var errorResult = new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Unexpected error inserting dunnage line: {ex.Message}",
                Severity = Models.Enums.Enum_ErrorSeverity.Error
            };

            // Log the error using error handler if available
            if (_errorHandler != null)
            {
                await _errorHandler.HandleErrorAsync(
                    errorResult.ErrorMessage,
                    errorResult.Severity,
                    ex,
                    showDialog: false
                );
            }

            return errorResult;
        }
    }
}
