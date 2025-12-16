using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.Helpers.Database;

namespace MTM_Receiving_Application.Data.Authentication
{
    /// <summary>
    /// Data Access Object for user authentication operations.
    /// Handles all database interactions for the users table.
    /// </summary>
    public class Dao_User
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor with connection string injection
        /// </summary>
        /// <param name="connectionString">MySQL connection string</param>
        public Dao_User(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // ====================================================================
        // Authentication Methods
        // ====================================================================

        /// <summary>
        /// Retrieves user by Windows username for automatic authentication.
        /// </summary>
        /// <param name="windowsUsername">Windows username from Environment.UserName</param>
        /// <returns>Result containing user data or error</returns>
        public async Task<Model_Dao_Result<Model_User>> GetUserByWindowsUsernameAsync(string windowsUsername)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand("sp_GetUserByWindowsUsername", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@p_windows_username", windowsUsername);

                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    var user = MapReaderToUser((MySqlDataReader)reader);
                    return Model_Dao_Result<Model_User>.SuccessResult(user);
                }

                return Model_Dao_Result<Model_User>.ErrorResult("User not found in database");
            }
            catch (MySqlException ex)
            {
                return Model_Dao_Result<Model_User>.ErrorResult($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Model_Dao_Result<Model_User>.ErrorResult($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates username and PIN combination for shared terminal login.
        /// </summary>
        /// <param name="username">Username (Windows username or full name)</param>
        /// <param name="pin">4-digit numeric PIN</param>
        /// <returns>Result containing user data or error</returns>
        public async Task<Model_Dao_Result<Model_User>> ValidateUserPinAsync(string username, string pin)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand("sp_ValidateUserPin", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@p_username", username);
                command.Parameters.AddWithValue("@p_pin", pin);

                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    var user = MapReaderToUser((MySqlDataReader)reader);
                    return Model_Dao_Result<Model_User>.SuccessResult(user);
                }

                return Model_Dao_Result<Model_User>.ErrorResult("Invalid username or PIN");
            }
            catch (MySqlException ex)
            {
                return Model_Dao_Result<Model_User>.ErrorResult($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Model_Dao_Result<Model_User>.ErrorResult($"Unexpected error: {ex.Message}");
            }
        }

        // ====================================================================
        // CRUD Operations
        // ====================================================================

        /// <summary>
        /// Creates a new user account with validation.
        /// </summary>
        /// <param name="user">User model with account data</param>
        /// <param name="createdBy">Windows username of account creator</param>
        /// <returns>Result containing new employee number or error</returns>
        public async Task<Model_Dao_Result<int>> CreateNewUserAsync(Model_User user, string createdBy)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand("sp_CreateNewUser", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Input parameters
                command.Parameters.AddWithValue("@p_employee_number", user.EmployeeNumber);
                command.Parameters.AddWithValue("@p_windows_username", user.WindowsUsername);
                command.Parameters.AddWithValue("@p_full_name", user.FullName);
                command.Parameters.AddWithValue("@p_pin", user.Pin);
                command.Parameters.AddWithValue("@p_department", user.Department);
                command.Parameters.AddWithValue("@p_shift", user.Shift);
                command.Parameters.AddWithValue("@p_created_by", createdBy);
                command.Parameters.AddWithValue("@p_visual_username", user.VisualUsername ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@p_visual_password", user.VisualPassword ?? (object)DBNull.Value);

                // Output parameters
                var errorMessageParam = new MySqlParameter("@p_error_message", MySqlDbType.VarChar, 500)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(errorMessageParam);

                await command.ExecuteNonQueryAsync();

                // Check for errors
                var errorMessage = errorMessageParam.Value?.ToString();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return Model_Dao_Result<int>.ErrorResult(errorMessage);
                }

                // Return the employee number that was provided
                return Model_Dao_Result<int>.SuccessResult(user.EmployeeNumber);
            }
            catch (MySqlException ex)
            {
                return Model_Dao_Result<int>.ErrorResult($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Model_Dao_Result<int>.ErrorResult($"Unexpected error: {ex.Message}");
            }
        }

        // ====================================================================
        // Validation Methods
        // ====================================================================

        /// <summary>
        /// Checks if a PIN is unique across all users.
        /// </summary>
        /// <param name="pin">4-digit PIN to check</param>
        /// <param name="excludeEmployeeNumber">Optional employee number to exclude from check</param>
        /// <returns>Result indicating if PIN is unique</returns>
        public async Task<Model_Dao_Result<bool>> IsPinUniqueAsync(string pin, int? excludeEmployeeNumber = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = excludeEmployeeNumber.HasValue
                    ? "SELECT COUNT(*) FROM users WHERE pin = @pin AND employee_number != @excludeId"
                    : "SELECT COUNT(*) FROM users WHERE pin = @pin";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@pin", pin);
                
                if (excludeEmployeeNumber.HasValue)
                {
                    command.Parameters.AddWithValue("@excludeId", excludeEmployeeNumber.Value);
                }

                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                var isUnique = count == 0;

                return Model_Dao_Result<bool>.SuccessResult(isUnique);
            }
            catch (MySqlException ex)
            {
                return Model_Dao_Result<bool>.ErrorResult($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Model_Dao_Result<bool>.ErrorResult($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a Windows username is unique across all users.
        /// </summary>
        /// <param name="username">Windows username to check</param>
        /// <param name="excludeEmployeeNumber">Optional employee number to exclude from check</param>
        /// <returns>Result indicating if username is unique</returns>
        public async Task<Model_Dao_Result<bool>> IsWindowsUsernameUniqueAsync(string username, int? excludeEmployeeNumber = null)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = excludeEmployeeNumber.HasValue
                    ? "SELECT COUNT(*) FROM users WHERE windows_username = @username AND employee_number != @excludeId"
                    : "SELECT COUNT(*) FROM users WHERE windows_username = @username";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);
                
                if (excludeEmployeeNumber.HasValue)
                {
                    command.Parameters.AddWithValue("@excludeId", excludeEmployeeNumber.Value);
                }

                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                var isUnique = count == 0;

                return Model_Dao_Result<bool>.SuccessResult(isUnique);
            }
            catch (MySqlException ex)
            {
                return Model_Dao_Result<bool>.ErrorResult($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Model_Dao_Result<bool>.ErrorResult($"Unexpected error: {ex.Message}");
            }
        }

        // ====================================================================
        // Activity Logging
        // ====================================================================

        /// <summary>
        /// Logs a user activity event for audit trail.
        /// </summary>
        /// <param name="eventType">Event type (login_success, login_failed, session_timeout, user_created)</param>
        /// <param name="username">Username involved in event</param>
        /// <param name="workstationName">Computer name where event occurred</param>
        /// <param name="details">Additional event details</param>
        /// <returns>Result indicating success or failure</returns>
        public async Task<Model_Dao_Result<bool>> LogUserActivityAsync(
            string eventType, 
            string username, 
            string workstationName, 
            string details)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand("sp_LogUserActivity", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@p_event_type", eventType);
                command.Parameters.AddWithValue("@p_username", username ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@p_workstation_name", workstationName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@p_details", details ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();

                return Model_Dao_Result<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                // Log errors but don't block application flow
                System.Diagnostics.Debug.WriteLine($"Failed to log activity: {ex.Message}");
                return Model_Dao_Result<bool>.ErrorResult($"Logging failed: {ex.Message}");
            }
        }

        // ====================================================================
        // Configuration Methods
        // ====================================================================

        /// <summary>
        /// Retrieves list of shared terminal workstation names.
        /// </summary>
        /// <returns>Result containing list of workstation names</returns>
        public async Task<Model_Dao_Result<List<string>>> GetSharedTerminalNamesAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand("sp_GetSharedTerminalNames", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                
                var names = new List<string>();
                while (await reader.ReadAsync())
                {
                    names.Add(reader.GetString("workstation_name"));
                }

                return Model_Dao_Result<List<string>>.SuccessResult(names);
            }
            catch (MySqlException ex)
            {
                return Model_Dao_Result<List<string>>.ErrorResult($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Model_Dao_Result<List<string>>.ErrorResult($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves list of active departments for dropdown population.
        /// </summary>
        /// <returns>Result containing list of department names</returns>
        public async Task<Model_Dao_Result<List<string>>> GetActiveDepartmentsAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand("sp_GetDepartments", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await command.ExecuteReaderAsync();
                
                var departments = new List<string>();
                while (await reader.ReadAsync())
                {
                    departments.Add(reader.GetString("department_name"));
                }

                return Model_Dao_Result<List<string>>.SuccessResult(departments);
            }
            catch (MySqlException ex)
            {
                return Model_Dao_Result<List<string>>.ErrorResult($"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Model_Dao_Result<List<string>>.ErrorResult($"Unexpected error: {ex.Message}");
            }
        }

        // ====================================================================
        // Helper Methods
        // ====================================================================

        /// <summary>
        /// Maps a MySqlDataReader row to a Model_User object.
        /// </summary>
        private Model_User MapReaderToUser(MySqlDataReader reader)
        {
            return new Model_User
            {
                EmployeeNumber = reader.GetInt32("employee_number"),
                WindowsUsername = reader.GetString("windows_username"),
                FullName = reader.GetString("full_name"),
                Pin = reader.GetString("pin"),
                Department = reader.GetString("department"),
                Shift = reader.GetString("shift"),
                IsActive = reader.GetBoolean("is_active"),
                VisualUsername = reader.IsDBNull(reader.GetOrdinal("visual_username")) 
                    ? null 
                    : reader.GetString("visual_username"),
                VisualPassword = reader.IsDBNull(reader.GetOrdinal("visual_password")) 
                    ? null 
                    : reader.GetString("visual_password"),
                CreatedDate = reader.GetDateTime("created_date"),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by")) 
                    ? null 
                    : reader.GetString("created_by"),
                ModifiedDate = reader.GetDateTime("modified_date")
            };
        }
    }

    /// <summary>
    /// Generic result wrapper for DAO operations with typed data.
    /// </summary>
    public class Model_Dao_Result<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public static Model_Dao_Result<T> SuccessResult(T data) => new()
        {
            Success = true,
            Data = data
        };

        public static Model_Dao_Result<T> ErrorResult(string message) => new()
        {
            Success = false,
            ErrorMessage = message
        };
    }
}
