using System;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Data;

/// <summary>
/// Data access for the Volvo label history archive tables.
/// <c>Clear Label Data</c> calls <see cref="ClearToHistoryAsync"/> which atomically
/// moves all <c>status='completed'</c> rows from <c>volvo_label_data</c> and
/// <c>volvo_line_data</c> into <c>volvo_label_history</c> and <c>volvo_line_history</c>.
/// </summary>
public class Dao_VolvoLabelHistory : IDao_VolvoLabelHistory
{
    private readonly string _connectionString;

    public Dao_VolvoLabelHistory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Atomically moves all completed Volvo shipments (and their lines) from the active tables
    /// to the history archive tables via <c>sp_Volvo_LabelData_ClearToHistory</c>.
    /// Returns a tuple of <c>(HeadersMoved, LinesMoved)</c> on success.
    /// </summary>
    /// <param name="archivedBy">Employee identifier to stamp on history records.</param>
    public async Task<Model_Dao_Result<(int HeadersMoved, int LinesMoved)>> ClearToHistoryAsync(string archivedBy)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_Volvo_LabelData_ClearToHistory", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("p_archived_by", archivedBy ?? "SYSTEM");

            var headersMovedParam = new MySqlParameter("p_headers_moved", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(headersMovedParam);

            var linesMovedParam = new MySqlParameter("p_lines_moved", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(linesMovedParam);

            var batchIdParam = new MySqlParameter("p_archive_batch_id", MySqlDbType.VarChar, 36)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(batchIdParam);

            var statusParam = new MySqlParameter("p_status", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(statusParam);

            var errorParam = new MySqlParameter("p_error_message", MySqlDbType.VarChar, 1000)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(errorParam);

            await command.ExecuteNonQueryAsync();

            var status = statusParam.Value == DBNull.Value ? 1 : Convert.ToInt32(statusParam.Value);
            var errorMessage = errorParam.Value == DBNull.Value ? null : errorParam.Value?.ToString();

            if (status != 0)
            {
                return Model_Dao_Result_Factory.Failure<(int, int)>(errorMessage ?? "Clear Label Data failed");
            }

            var headersMoved = headersMovedParam.Value == DBNull.Value ? 0 : Convert.ToInt32(headersMovedParam.Value);
            var linesMoved = linesMovedParam.Value == DBNull.Value ? 0 : Convert.ToInt32(linesMovedParam.Value);

            return Model_Dao_Result_Factory.Success<(int, int)>((headersMoved, linesMoved));
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<(int, int)>(
                $"Failed to clear Volvo label data to history: {ex.Message}", ex);
        }
    }
}
