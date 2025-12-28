using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.InforVisual;

namespace MTM_Receiving_Application.Data.InforVisual;

public class Dao_InforVisualPO
{
    private readonly string _connectionString;

    public Dao_InforVisualPO(string inforVisualConnectionString)
    {
        ValidateReadOnlyConnection(inforVisualConnectionString);
        _connectionString = inforVisualConnectionString;
    }

    private static void ValidateReadOnlyConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            if (builder.ApplicationIntent != ApplicationIntent.ReadOnly)
            {
                throw new InvalidOperationException(
                    $"CONSTITUTIONAL VIOLATION: Infor Visual DAO requires ApplicationIntent=ReadOnly. " +
                    $"Current ApplicationIntent: {builder.ApplicationIntent}. " +
                    $"Writing to Infor Visual ERP database is STRICTLY PROHIBITED. " +
                    "See Constitution Principle X: Infor Visual DAO Architecture.");
            }
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                $"Invalid Infor Visual connection string format: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<List<Model_InforVisualPO>>> GetByPoNumberAsync(string poNumber)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    po.po_num AS PoNumber,
                    pol.po_line AS PoLine,
                    pol.part AS PartNumber,
                    p.description AS PartDescription,
                    pol.qty_ordered AS OrderedQty,
                    pol.qty_received AS ReceivedQty,
                    (pol.qty_ordered - pol.qty_received) AS RemainingQty,
                    pol.u_m AS UnitOfMeasure,
                    pol.due_date AS DueDate,
                    po.vend_id AS VendorCode,
                    v.name AS VendorName,
                    po.stat AS PoStatus,
                    po.site_id AS SiteId
                FROM po
                INNER JOIN po_line pol ON po.po_num = pol.po_num
                INNER JOIN part p ON pol.part = p.part_id
                LEFT JOIN vendor v ON po.vend_id = v.vend_id
                WHERE po.po_num = @PoNumber
                AND po.site_id = '002'
                ORDER BY pol.po_line";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PoNumber", poNumber);

            var list = new List<Model_InforVisualPO>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Model_InforVisualPO
                {
                    PoNumber = reader["PoNumber"].ToString() ?? string.Empty,
                    PoLine = Convert.ToInt32(reader["PoLine"]),
                    PartNumber = reader["PartNumber"].ToString() ?? string.Empty,
                    PartDescription = reader["PartDescription"].ToString() ?? string.Empty,
                    OrderedQty = Convert.ToDecimal(reader["OrderedQty"]),
                    ReceivedQty = Convert.ToDecimal(reader["ReceivedQty"]),
                    RemainingQty = Convert.ToDecimal(reader["RemainingQty"]),
                    UnitOfMeasure = reader["UnitOfMeasure"].ToString() ?? "EA",
                    DueDate = reader["DueDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["DueDate"]),
                    VendorCode = reader["VendorCode"].ToString() ?? string.Empty,
                    VendorName = reader["VendorName"].ToString() ?? string.Empty,
                    PoStatus = reader["PoStatus"].ToString() ?? string.Empty,
                    SiteId = reader["SiteId"].ToString() ?? "002"
                });
            }

            return Model_Dao_Result_Factory.Success(list);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_InforVisualPO>>(
                $"Error retrieving PO {poNumber}: {ex.Message}",
                ex);
        }
    }

    public async Task<Model_Dao_Result<bool>> ValidatePoNumberAsync(string poNumber)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT COUNT(*) FROM po WHERE po_num = @PoNumber AND site_id = '002'";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PoNumber", poNumber);

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return Model_Dao_Result_Factory.Success(count > 0);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<bool>(
                $"Error validating PO {poNumber}: {ex.Message}",
                ex);
        }
    }
}
