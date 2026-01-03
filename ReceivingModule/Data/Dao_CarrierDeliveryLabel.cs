using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ReceivingModule.Models;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.ReceivingModule.Data;

/// <summary>
/// Data Access Object for label_table_parcel table
/// Handles carrier delivery label records (UPS/FedEx/USPS shipping info)
/// Provides CRUD operations using stored procedures
/// </summary>
public static class Dao_CarrierDeliveryLabel
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
    /// Inserts a new carrier delivery label record into the database
    /// </summary>
    /// <param name="label">CarrierDeliveryLabel model to insert</param>
    /// <returns>Model_Dao_Result with success status and affected rows</returns>
    public static async Task<Model_Dao_Result> InsertCarrierDeliveryLabelAsync(Model_CarrierDeliveryLabel label)
    {
        try
        {
            // Get connection string
            string connectionString = Helper_Database_Variables.GetConnectionString(useProduction: true);

            // Prepare stored procedure parameters
            var parameters = new MySqlParameter[]
            {
                new MySqlParameter("@p_DeliverTo", label.DeliverTo ?? string.Empty),
                new MySqlParameter("@p_Department", label.Department ?? string.Empty),
                new MySqlParameter("@p_PackageDescription", label.PackageDescription ?? string.Empty),
                new MySqlParameter("@p_PONumber", (object?)label.PONumber ?? DBNull.Value),
                new MySqlParameter("@p_WorkOrderNumber", label.WorkOrderNumber ?? string.Empty),
                new MySqlParameter("@p_EmployeeNumber", label.EmployeeNumber),
                new MySqlParameter("@p_Date", label.Date),
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
                "carrier_delivery_label_Insert",
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
                ErrorMessage = $"Unexpected error inserting carrier delivery label: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error
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
