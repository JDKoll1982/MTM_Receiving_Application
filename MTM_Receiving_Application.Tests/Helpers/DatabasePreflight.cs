using System;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Helpers;

internal static class DatabasePreflight
{
    internal static async Task<(bool IsReady, string? Reason)> CheckAsync(
        string connectionString,
        string requiredStoredProcedure)
    {
        try
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(@"SELECT ROUTINE_NAME
FROM information_schema.ROUTINES
WHERE ROUTINE_SCHEMA = DATABASE()
  AND ROUTINE_TYPE = 'PROCEDURE'
  AND ROUTINE_NAME = @proc;", connection)
            {
                CommandType = CommandType.Text
            };

            command.Parameters.AddWithValue("@proc", requiredStoredProcedure);

            var result = await command.ExecuteScalarAsync();
            if (result is null)
            {
                return (false,
                    $"MySQL test database is reachable but missing required stored procedure '{requiredStoredProcedure}'. " +
                    "Provision the test schema/stored procedures for 'mtm_receiving_application_test'.");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"MySQL test database is not ready: {ex.Message}");
        }
    }
}
