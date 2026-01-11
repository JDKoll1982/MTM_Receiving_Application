using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;


namespace MTM_Receiving_Application.Module_Core.Data.Authentication
{
    /// <summary>
    /// Data Access Object for user authentication operations.
    /// Handles all database interactions for the auth_users table.
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
            var parameters = new Dictionary<string, object>
            {
                { "@p_windows_username", windowsUsername }
            };

            return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
                _connectionString,
                "sp_GetUserByWindowsUsername",
                MapReaderToUser,
                parameters
            );
        }

        /// <summary>
        /// Validates username and PIN combination for shared terminal login.
        /// </summary>
        /// <param name="username">Username (Windows username or full name)</param>
        /// <param name="pin">4-digit numeric PIN</param>
        /// <returns>Result containing user data or error</returns>
        public async Task<Model_Dao_Result<Model_User>> ValidateUserPinAsync(string username, string pin)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@p_username", username },
                { "@p_pin", pin }
            };

            return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
                _connectionString,
                "sp_ValidateUserPin",
                MapReaderToUser,
                parameters
            );
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
                await using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand("sp_CreateNewUser", connection)
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
                    return Model_Dao_Result_Factory.Failure<int>(errorMessage);
                }

                // Return the employee number that was provided
                // Defaults are seeded server-side by sp_CreateNewUser -> sp_seed_user_default_modes.
                // Extra safety: best-effort call in case the DB procedure set isn't deployed yet.
                try
                {
                    await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                        _connectionString,
                        "sp_seed_user_default_modes",
                        new Dictionary<string, object>
                        {
                            { "user_id", user.EmployeeNumber }
                        });
                }
                catch
                {
                    // Swallow: defaults are non-critical and must not block first run.
                }

                return Model_Dao_Result_Factory.Success<int>(user.EmployeeNumber);
            }
            catch (MySqlException ex)
            {
                return Model_Dao_Result_Factory.Failure<int>($"Database error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                return Model_Dao_Result_Factory.Failure<int>($"Unexpected error: {ex.Message}", ex);
            }
        }



        // ====================================================================
        // Validation Methods
        // ====================================================================

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
                await using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = excludeEmployeeNumber.HasValue
                    ? "SELECT COUNT(*) FROMauth_users WHERE windows_username = @username AND employee_number != @excludeId"
                    : "SELECT COUNT(*) FROMauth_users WHERE windows_username = @username";

                await using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);

                if (excludeEmployeeNumber.HasValue)
                {
                    command.Parameters.AddWithValue("@excludeId", excludeEmployeeNumber.Value);
                }

                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                var isUnique = count == 0;

                return Model_Dao_Result_Factory.Success<bool>(isUnique);
            }
            catch (MySqlException ex)
            {
                return Model_Dao_Result_Factory.Failure<bool>($"Database error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                return Model_Dao_Result_Factory.Failure<bool>($"Unexpected error: {ex.Message}", ex);
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
            var parameters = new Dictionary<string, object>
            {
                { "@p_event_type", eventType },
                { "@p_username", username ?? (object)DBNull.Value },
                { "@p_workstation_name", workstationName ?? (object)DBNull.Value },
                { "@p_details", details ?? (object)DBNull.Value }
            };

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_LogUserActivity",
                parameters
            );

            if (result.Success)
            {
                return Model_Dao_Result_Factory.Success<bool>(true);
            }
            else
            {
                return Model_Dao_Result_Factory.Failure<bool>(result.ErrorMessage, result.Exception);
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
            return await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_GetSharedTerminalNames",
                reader => reader.GetString(reader.GetOrdinal("workstation_name"))
            );
        }

        /// <summary>
        /// Inserts or updates the current workstation configuration record.
        /// Best-effort: caller may ignore failures if the SP is not deployed yet.
        /// </summary>
        public async Task<Model_Dao_Result> UpsertWorkstationConfigAsync(
            string workstationName,
            string workstationType,
            bool isActive,
            string? description)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@p_workstation_name", workstationName },
                { "@p_workstation_type", workstationType },
                { "@p_is_active", isActive },
                { "@p_description", description ?? (object)DBNull.Value }
            };

            return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_UpsertWorkstationConfig",
                parameters
            );
        }

        /// <summary>
        /// Retrieves list of active departments for dropdown population.
        /// </summary>
        /// <returns>Result containing list of department names</returns>
        public async Task<Model_Dao_Result<List<string>>> GetActiveDepartmentsAsync()
        {
            return await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_GetDepartments",
                reader => reader.GetString(reader.GetOrdinal("department_name"))
            );
        }

        // ====================================================================
        // Helper Methods
        // ====================================================================

        /// <summary>
        /// Maps a IDataReader row to a Model_User object.
        /// </summary>
        /// <param name="reader"></param>
        private Model_User MapReaderToUser(IDataReader reader)
        {
            return new Model_User
            {
                EmployeeNumber = reader.GetInt32(reader.GetOrdinal("employee_number")),
                WindowsUsername = reader.GetString(reader.GetOrdinal("windows_username")),
                FullName = reader.GetString(reader.GetOrdinal("full_name")),
                Pin = reader.GetString(reader.GetOrdinal("pin")),
                Department = reader.GetString(reader.GetOrdinal("department")),
                Shift = reader.GetString(reader.GetOrdinal("shift")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
                VisualUsername = reader.IsDBNull(reader.GetOrdinal("visual_username"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("visual_username")),
                VisualPassword = reader.IsDBNull(reader.GetOrdinal("visual_password"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("visual_password")),
                DefaultReceivingMode = TryGetDefaultReceivingMode(reader),
                DefaultDunnageMode = TryGetDefaultDunnageMode(reader),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("created_by"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("created_by")),
                ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modified_date"))
            };
        }

        /// <summary>
        /// Updates user's default receiving mode preference
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="defaultMode"></param>
        public async Task<Model_Dao_Result> UpdateDefaultModeAsync(int userId, string? defaultMode)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@p_user_id", userId },
                { "@p_default_mode", (object?)defaultMode ?? DBNull.Value }
            };

            return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_update_user_default_mode",
                parameters
            );
        }

        /// <summary>
        /// Updates the user's default receiving workflow mode preference (guided, manual, edit)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="defaultMode"></param>
        public async Task<Model_Dao_Result> UpdateDefaultReceivingModeAsync(int userId, string? defaultMode)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@p_user_id", userId },
                { "@p_default_mode", (object?)defaultMode ?? DBNull.Value }
            };

            return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_update_user_default_receiving_mode",
                parameters
            );
        }

        /// <summary>
        /// Updates the user's default dunnage workflow mode preference
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="defaultMode"></param>
        public async Task<Model_Dao_Result> UpdateDefaultDunnageModeAsync(int userId, string? defaultMode)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@p_user_id", userId },
                { "@p_default_mode", (object?)defaultMode ?? DBNull.Value }
            };

            return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_update_user_default_dunnage_mode",
                parameters
            );
        }

        /// <summary>
        /// Safely attempts to read default_receiving_mode column, returns null if column doesn't exist
        /// </summary>
        /// <param name="reader"></param>
        private static string? TryGetDefaultReceivingMode(IDataReader reader)
        {
            try
            {
                var ordinal = reader.GetOrdinal("default_receiving_mode");
                return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                // Column doesn't exist yet (migration not run) - return null
                return null;
            }
        }

        /// <summary>
        /// Safely attempts to read default_dunnage_mode column, returns null if column doesn't exist
        /// </summary>
        /// <param name="reader"></param>
        private static string? TryGetDefaultDunnageMode(IDataReader reader)
        {
            try
            {
                var ordinal = reader.GetOrdinal("default_dunnage_mode");
                return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                // Column doesn't exist yet (migration not run) - return null
                return null;
            }
        }
    }
}
