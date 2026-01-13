using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Data;

/// <summary>
/// Data Access Object for Infor Visual PO data (READ ONLY)
/// Provides read-only access to purchase order data from SQL Server
/// </summary>
public class Dao_InforVisualPO
{
    private readonly string _connectionString;

    /// <summary>
    /// Constructor with connection string injection
    /// </summary>
    /// <param name="connectionString">SQL Server connection string (should include ApplicationIntent=ReadOnly for safety)</param>
    public Dao_InforVisualPO(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Validates that a PO exists and is active in Infor Visual
    /// </summary>
    /// <param name="poNumber">Purchase order number</param>
    /// <returns>Model_Dao_Result with true if PO exists and is open, false otherwise</returns>
    public async Task<Model_Dao_Result<bool>> ValidatePOAsync(string poNumber)
    {
        try
        {
            Console.WriteLine($"[DAO] Validating PO: {poNumber}");
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT COUNT(*)
                    FROM PURCHASE_ORDER WITH (NOLOCK)
                    WHERE ID = @PoNumber;";

                await using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PoNumber", poNumber);

                    var count = (int)(await command.ExecuteScalarAsync() ?? 0);

                    return Model_Dao_Result_Factory.Success(count > 0);
                }
            }
        }
        catch (SqlException ex)
        {
            return Model_Dao_Result_Factory.Failure<bool>($"Database error validating PO: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<bool>($"Unexpected error validating PO: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets all line items for a specific purchase order
    /// </summary>
    /// <param name="poNumber">Purchase order number</param>
    /// <returns>Model_Dao_Result with list of PO lines</returns>
    public async Task<Model_Dao_Result<List<Model_InforVisualPOLine>>> GetLinesAsync(string poNumber)
    {
        try
        {
            // Issue #32: Enhanced logging
            Console.WriteLine($"[DAO] Fetching lines for PO: {poNumber}");

            // Issue #21: Reduced nesting by extracting query execution
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    pol.PURC_ORDER_ID AS PO_ID,
                    pol.LINE_NO AS PO_LINE,
                    pol.PART_ID,
                    pol.ORDER_QTY AS QTY_ORDERED,
                    pol.TOTAL_RECEIVED_QTY AS QTY_RECEIVED,
                    pol.UNIT_PRICE,
                    pol.LINE_STATUS AS STATUS,
                    p.DESCRIPTION AS PART_NAME,
                    po.VENDOR_ID,
                    CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), plb.BITS)) AS SPECS
                FROM PURC_ORDER_LINE pol WITH (NOLOCK)
                INNER JOIN PURCHASE_ORDER po WITH (NOLOCK) ON pol.PURC_ORDER_ID = po.ID
                LEFT JOIN PART p WITH (NOLOCK) ON pol.PART_ID = p.ID
                LEFT JOIN PURC_LINE_BINARY plb WITH (NOLOCK) ON pol.PURC_ORDER_ID = plb.PURC_ORDER_ID 
                    AND pol.LINE_NO = plb.PURC_ORDER_LINE_NO 
                    AND plb.TYPE = 'D'
                WHERE pol.PURC_ORDER_ID = @PoNumber
                ORDER BY pol.LINE_NO";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PoNumber", poNumber);

            var lines = new List<Model_InforVisualPOLine>();
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lines.Add(MapFromReader(reader));
            }

            return Model_Dao_Result_Factory.Success(lines);
        }
        catch (SqlException ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualPOLine>>($"Database error getting PO lines: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualPOLine>>($"Unexpected error getting PO lines: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets a specific line item from a purchase order
    /// </summary>
    /// <param name="poNumber">Purchase order number</param>
    /// <param name="lineNumber">Line number</param>
    /// <returns>Model_Dao_Result with PO line data</returns>
    public async Task<Model_Dao_Result<Model_InforVisualPOLine>> GetLineAsync(string poNumber, int lineNumber)
    {
        try
        {
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        pol.PURC_ORDER_ID AS PO_ID,
                        pol.LINE_NO AS PO_LINE,
                        pol.PART_ID,
                        pol.ORDER_QTY AS QTY_ORDERED,
                        pol.TOTAL_RECEIVED_QTY AS QTY_RECEIVED,
                        pol.UNIT_PRICE,
                        pol.LINE_STATUS AS STATUS,
                        p.DESCRIPTION AS PART_NAME,
                        po.VENDOR_ID,
                        CONVERT(NVARCHAR(MAX), CONVERT(VARBINARY(MAX), plb.BITS)) AS SPECS
                    FROM PURC_ORDER_LINE pol WITH (NOLOCK)
                    INNER JOIN PURCHASE_ORDER po WITH (NOLOCK) ON pol.PURC_ORDER_ID = po.ID
                    LEFT JOIN PART p WITH (NOLOCK) ON pol.PART_ID = p.ID
                    LEFT JOIN PURC_LINE_BINARY plb WITH (NOLOCK) ON pol.PURC_ORDER_ID = plb.PURC_ORDER_ID 
                        AND pol.LINE_NO = plb.PURC_ORDER_LINE_NO 
                        AND plb.TYPE = 'D'
                    WHERE pol.PURC_ORDER_ID = @PoNumber
                      AND pol.LINE_NO = @LineNumber";

                await using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PoNumber", poNumber);
                    command.Parameters.AddWithValue("@LineNumber", lineNumber);

                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var line = MapFromReader(reader);
                            return Model_Dao_Result_Factory.Success(line);
                        }
                        else
                        {
                            return Model_Dao_Result_Factory.Failure<Model_InforVisualPOLine>(
                                $"PO line {poNumber}-{lineNumber} not found"
                            );
                        }
                    }
                }
            }
        }
        catch (SqlException ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPOLine>($"Database error getting PO line: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPOLine>($"Unexpected error getting PO line: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks if Infor Visual connection is available
    /// </summary>
    /// <returns>Model_Dao_Result with true if connection successful, false otherwise</returns>
    public async Task<Model_Dao_Result<bool>> CheckConnectionAsync()
    {
        try
        {
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                return Model_Dao_Result_Factory.Success(true);
            }
        }
        catch (SqlException)
        {
            return Model_Dao_Result_Factory.Success(false);
        }
        catch (Exception)
        {
            return Model_Dao_Result_Factory.Success(false);
        }
    }

    /// <summary>
    /// Maps SqlDataReader row to Model_InforVisualPOLine
    /// </summary>
    /// <param name="reader"></param>
    private Model_InforVisualPOLine MapFromReader(IDataReader reader)
    {
        return new Model_InforVisualPOLine
        {
            PONumber = reader["PO_ID"].ToString() ?? string.Empty,
            // Use indexer + ToString() to safely handle smallint (Int16) vs int (Int32) mismatch
            // reader.GetInt32() throws on smallint columns
            LineNumber = reader["PO_LINE"].ToString() ?? string.Empty,
            PartID = reader["PART_ID"].ToString() ?? string.Empty,
            Description = reader.IsDBNull(reader.GetOrdinal("PART_NAME"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("PART_NAME")),
            Specifications = reader.IsDBNull(reader.GetOrdinal("SPECS"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("SPECS")),
            // Handle potentially nullable decimals safely
            QuantityOrdered = reader.IsDBNull(reader.GetOrdinal("QTY_ORDERED"))
                ? 0m
                : reader.GetDecimal(reader.GetOrdinal("QTY_ORDERED")),
            QuantityReceived = reader.IsDBNull(reader.GetOrdinal("QTY_RECEIVED"))
                ? 0m
                : reader.GetDecimal(reader.GetOrdinal("QTY_RECEIVED"))
        };
    }
}
