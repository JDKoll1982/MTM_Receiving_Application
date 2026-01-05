using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models;
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
    /// <param name="connectionString">SQL Server connection string (must include ApplicationIntent=ReadOnly)</param>
    public Dao_InforVisualPO(string connectionString)
    {
        if (!connectionString.Contains("ApplicationIntent=ReadOnly", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Infor Visual connection must be READ ONLY (ApplicationIntent=ReadOnly)", nameof(connectionString));
        }
        
        _connectionString = connectionString;
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
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT COUNT(*)
                    FROM PURCHASE_ORDER WITH (NOLOCK)
                    WHERE PO_ID = @PoNumber
                      AND SITE_REF = '002'
                      AND STATUS IN ('O', 'P')"; // Open or Partial

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PoNumber", poNumber);
                    
                    var count = (int)await command.ExecuteScalarAsync();
                    
                    return Model_Dao_Result<bool>.Success(
                        count > 0,
                        count > 0 ? "PO is valid" : "PO not found or not open",
                        1
                    );
                }
            }
        }
        catch (SqlException ex)
        {
            return Model_Dao_Result<bool>.Failure($"Database error validating PO: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result<bool>.Failure($"Unexpected error validating PO: {ex.Message}", ex);
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
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        pol.PO_ID,
                        pol.PO_LINE,
                        pol.PART_ID,
                        pol.QTY_ORDERED,
                        pol.QTY_RECEIVED,
                        pol.UNIT_PRICE,
                        pol.STATUS,
                        p.PART_NAME,
                        po.VENDOR_ID
                    FROM PURC_ORDER_LINE pol WITH (NOLOCK)
                    INNER JOIN PURCHASE_ORDER po WITH (NOLOCK) ON pol.PO_ID = po.PO_ID
                    LEFT JOIN PART p WITH (NOLOCK) ON pol.PART_ID = p.PART_ID
                    WHERE pol.PO_ID = @PoNumber
                      AND pol.SITE_REF = '002'
                    ORDER BY pol.PO_LINE";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PoNumber", poNumber);
                    
                    var lines = new List<Model_InforVisualPOLine>();
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            lines.Add(MapFromReader(reader));
                        }
                    }
                    
                    return Model_Dao_Result<List<Model_InforVisualPOLine>>.Success(
                        lines,
                        $"Retrieved {lines.Count} lines for PO {poNumber}",
                        lines.Count
                    );
                }
            }
        }
        catch (SqlException ex)
        {
            return Model_Dao_Result<List<Model_InforVisualPOLine>>.Failure($"Database error getting PO lines: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result<List<Model_InforVisualPOLine>>.Failure($"Unexpected error getting PO lines: {ex.Message}", ex);
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
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        pol.PO_ID,
                        pol.PO_LINE,
                        pol.PART_ID,
                        pol.QTY_ORDERED,
                        pol.QTY_RECEIVED,
                        pol.UNIT_PRICE,
                        pol.STATUS,
                        p.PART_NAME,
                        po.VENDOR_ID
                    FROM PURC_ORDER_LINE pol WITH (NOLOCK)
                    INNER JOIN PURCHASE_ORDER po WITH (NOLOCK) ON pol.PO_ID = po.PO_ID
                    LEFT JOIN PART p WITH (NOLOCK) ON pol.PART_ID = p.PART_ID
                    WHERE pol.PO_ID = @PoNumber
                      AND pol.PO_LINE = @LineNumber
                      AND pol.SITE_REF = '002'";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PoNumber", poNumber);
                    command.Parameters.AddWithValue("@LineNumber", lineNumber);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var line = MapFromReader(reader);
                            return Model_Dao_Result<Model_InforVisualPOLine>.Success(
                                line,
                                "PO line retrieved successfully",
                                1
                            );
                        }
                        else
                        {
                            return Model_Dao_Result<Model_InforVisualPOLine>.Failure(
                                $"PO line {poNumber}-{lineNumber} not found"
                            );
                        }
                    }
                }
            }
        }
        catch (SqlException ex)
        {
            return Model_Dao_Result<Model_InforVisualPOLine>.Failure($"Database error getting PO line: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result<Model_InforVisualPOLine>.Failure($"Unexpected error getting PO line: {ex.Message}", ex);
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
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                return Model_Dao_Result<bool>.Success(
                    true,
                    "Infor Visual connection successful",
                    1
                );
            }
        }
        catch (SqlException ex)
        {
            return Model_Dao_Result<bool>.Success(
                false,
                $"Infor Visual connection failed: {ex.Message}",
                0
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result<bool>.Success(
                false,
                $"Infor Visual connection error: {ex.Message}",
                0
            );
        }
    }

    /// <summary>
    /// Maps SqlDataReader row to Model_InforVisualPOLine
    /// </summary>
    private Model_InforVisualPOLine MapFromReader(IDataReader reader)
    {
        return new Model_InforVisualPOLine
        {
            PONumber = reader["PO_ID"].ToString() ?? string.Empty,
            LineNumber = Convert.ToInt32(reader["PO_LINE"]),
            PartID = reader["PART_ID"].ToString() ?? string.Empty,
            PartName = reader.IsDBNull(reader.GetOrdinal("PART_NAME")) 
                ? string.Empty 
                : reader["PART_NAME"].ToString() ?? string.Empty,
            QuantityOrdered = Convert.ToDecimal(reader["QTY_ORDERED"]),
            QuantityReceived = Convert.ToDecimal(reader["QTY_RECEIVED"]),
            UnitPrice = Convert.ToDecimal(reader["UNIT_PRICE"]),
            Status = reader["STATUS"].ToString() ?? string.Empty,
            VendorID = reader["VENDOR_ID"].ToString() ?? string.Empty
        };
    }
}
