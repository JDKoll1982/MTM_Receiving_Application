using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using ConsoleTables;

namespace InforVisualTest
{
    class Program
    {
        // Credentials provided by user
        private const string ConnectionString = "Server=VISUAL;Database=MTMFG;User Id=SHOP2;Password=SHOP;TrustServerCertificate=True;ApplicationIntent=ReadOnly;";

        static async Task Main(string[] args)
        {
            Console.WriteLine("================================================");
            Console.WriteLine("   Infor Visual Database Query Tester");
            Console.WriteLine("   Server: VISUAL | Database: MTMFG");
            Console.WriteLine("================================================\n");

            try
            {
                // Test Connection
                Console.WriteLine("Testing Connection...");
                await TestConnectionAsync();
                Console.WriteLine("------------------------------------------------\n");

                // Test Case 1: PO-067343 (Should have parts)
                Console.WriteLine("TEST CASE 1: Standard PO [PO-067343]");
                await TestPO("PO-067343");
                Console.WriteLine("------------------------------------------------\n");

                // Test Case 2: PO-067381 (No parts / Routing module usage)
                Console.WriteLine("TEST CASE 2: Routing/Service PO [PO-067381]");
                await TestPO("PO-067381");
                Console.WriteLine("------------------------------------------------\n");

                // Test Case 3: Part Lookup (Using a known part if found in TC1, else skipping)
                Console.WriteLine("TEST CASE 3: Part Lookup");
                // For direct testing, I'll dry run a common part or just skip if I can't determine one dynamically easily without more complex logic.
                // But I can try to grab one from the first query if I store state.
                // Let's modify TestPO to return a part number.
                
                // Test Case 4: Hardcoded Queries from Module_Routing
                Console.WriteLine("\nTEST CASE 4: Hardcoded Queries (Module_Routing)");
                await TestHardcodedRoutingQueries("PO-067343");

                // Test Case 5: Corrected Queries
                Console.WriteLine("\nTEST CASE 5: Corrected Routing Queries (Proposed)");
                await TestCorrectedRoutingQueries("PO-067343");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR: {ex.Message}");
            }
        }

        static async Task TestConnectionAsync()
        {
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();
                Console.WriteLine("✅ Connection Successful!");
                Console.WriteLine($"   Server Version: {connection.ServerVersion}");
                Console.WriteLine($"   State: {connection.State}");
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"❌ Connection Failed: {ex.Message}");
            }
        }

        static async Task TestPO(string poNumber)
        {
            // 1. Validate PO
            Console.WriteLine($"Step A: Validate PO {poNumber}");
            string validateQuery = "SELECT COUNT(*) AS POExists FROM dbo.PURCHASE_ORDER WHERE ID = @PoNumber;";
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();
                using var command = new SqlCommand(validateQuery, connection);
                command.Parameters.AddWithValue("@PoNumber", poNumber);
                var count = (int?)await command.ExecuteScalarAsync() ?? 0;
                
                if (count > 0)
                    Console.WriteLine($"   ✅ PO Exists (Count: {count})");
                else
                    Console.WriteLine($"   ⚠️ PO Not Found in PURCHASE_ORDER table");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error: {ex.Message}");
            }

            // 2. Get Details
            Console.WriteLine($"Step B: Get PO Details (01_GetPOWithParts.sql logic)");
            string detailsQuery = @"
SELECT 
    po.ID AS PoNumber,
    pol.LINE_NO AS PoLine,
    pol.PART_ID AS PartNumber,
    p.DESCRIPTION AS PartDescription,
    pol.ORDER_QTY AS OrderedQty,
    pol.TOTAL_RECEIVED_QTY AS ReceivedQty,
    (pol.ORDER_QTY - pol.TOTAL_RECEIVED_QTY) AS RemainingQty,
    pol.PURCHASE_UM AS UnitOfMeasure,
    pol.PROMISE_DATE AS DueDate,
    po.VENDOR_ID AS VendorCode,
    v.NAME AS VendorName,
    po.STATUS AS PoStatus,
    po.SITE_ID AS SiteId
FROM dbo.PURCHASE_ORDER po
INNER JOIN dbo.PURC_ORDER_LINE pol ON po.ID = pol.PURC_ORDER_ID
LEFT JOIN dbo.PART p ON pol.PART_ID = p.ID
LEFT JOIN dbo.VENDOR v ON po.VENDOR_ID = v.ID
WHERE po.ID = @PoNumber
ORDER BY pol.LINE_NO;";

            string? firstPartNumber = null;

            try
            {
                using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();
                using var command = new SqlCommand(detailsQuery, connection);
                command.Parameters.AddWithValue("@PoNumber", poNumber);
                
                using var reader = await command.ExecuteReaderAsync();
                var table = new ConsoleTable("Line", "Part", "Desc", "Ord", "Rem", "Status", "Site");

                int rows = 0;
                while (await reader.ReadAsync())
                {
                    rows++;
                    var partId = reader["PartNumber"]?.ToString();
                    if (string.IsNullOrEmpty(firstPartNumber) && !string.IsNullOrEmpty(partId))
                    {
                        firstPartNumber = partId;
                    }

                    table.AddRow(
                        reader["PoLine"],
                        Truncate(partId, 15),
                        Truncate(reader["PartDescription"]?.ToString(), 20),
                        reader["OrderedQty"],
                        reader["RemainingQty"],
                        reader["PoStatus"],
                        reader["SiteId"]
                    );
                }

                if (rows > 0)
                {
                    table.Write();
                    Console.WriteLine($"   ✅ Retrieved {rows} lines");
                }
                else
                {
                    Console.WriteLine("   ⚠️ No lines returned (Query executed successfully)");
                }

                if (!string.IsNullOrEmpty(firstPartNumber))
                {
                    await TestPart(firstPartNumber);
                }
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"   ❌ Error: {ex.Message}");
            }
        }

        static async Task TestPart(string partNumber)
        {
             Console.WriteLine($"\nStep C: Test Part Details for '{partNumber}'");
             
             // 03_GetPartByNumber.sql logic
             string partQuery = @"
SELECT 
    p.ID AS PartNumber,
    p.DESCRIPTION AS Description,
    ps.UNIT_MATERIAL_COST AS UnitCost,
    p.STOCK_UM AS PrimaryUom,
    COALESCE(ps.QTY_ON_HAND, 0) AS OnHandQty,
    COALESCE(ps.QTY_COMMITTED, 0) AS AllocatedQty,
    (COALESCE(ps.QTY_ON_HAND, 0) - COALESCE(ps.QTY_COMMITTED, 0)) AS AvailableQty,
    ps.SITE_ID AS DefaultSite,
    ps.STATUS AS PartStatus,
    p.PRODUCT_CODE AS ProductLine
FROM dbo.PART p
LEFT JOIN dbo.PART_SITE ps ON p.ID = ps.PART_ID
WHERE p.ID = @PartNumber;";

            try
            {
                using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();
                using var command = new SqlCommand(partQuery, connection);
                command.Parameters.AddWithValue("@PartNumber", partNumber);
                
                using var reader = await command.ExecuteReaderAsync();
                 if (await reader.ReadAsync())
                 {
                     Console.WriteLine($"   ✅ Found Part: {reader["PartNumber"]}");
                     Console.WriteLine($"      Desc: {reader["Description"]}");
                     Console.WriteLine($"      UOM: {reader["PrimaryUom"]} | OnHand: {reader["OnHandQty"]} | Avail: {reader["AvailableQty"]}");
                 }
                 else
                 {
                     Console.WriteLine($"   ⚠️ Part {partNumber} not found in PART table");
                 }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error: {ex.Message}");
            }

            // 04_SearchPartsByDescription.sql logic (Search by first 3 chars)
            if (partNumber.Length >= 3)
            {
                string searchTerm = partNumber.Substring(0, 3);
                Console.WriteLine($"\nStep D: Test Search Description (Term: '{searchTerm}')");
                string searchQuery = @"
SELECT TOP 5
    p.ID AS PartNumber,
    p.DESCRIPTION AS Description
FROM dbo.PART p
LEFT JOIN dbo.PART_SITE ps ON p.ID = ps.PART_ID
WHERE p.DESCRIPTION LIKE @SearchTerm + '%'
ORDER BY p.ID;";
                
                 try
                {
                    using var connection = new SqlConnection(ConnectionString);
                    await connection.OpenAsync();
                    using var command = new SqlCommand(searchQuery, connection);
                    command.Parameters.AddWithValue("@SearchTerm", searchTerm);
                    
                    using var reader = await command.ExecuteReaderAsync();
                    int found = 0;
                    while(await reader.ReadAsync()) { found++; }
                    Console.WriteLine($"   ✅ Search returned {found} results for term '{searchTerm}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ Error: {ex.Message}");
                }
            }
        }

        static async Task TestHardcodedRoutingQueries(string poNumber)
        {
             // 1. ValidatePOAsync
            Console.WriteLine($"Step A: Validate PO (Hardcoded Logic)");
            string query1 = @"
                    SELECT COUNT(*)
                    FROM PURCHASE_ORDER WITH (NOLOCK)
                    WHERE PO_ID = @PoNumber
                      AND SITE_REF = '002'
                      AND STATUS IN ('O', 'P')";
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand(query1, connection);
                command.Parameters.AddWithValue("@PoNumber", poNumber);
                var count = (int?)await command.ExecuteScalarAsync() ?? 0;
                Console.WriteLine($"   ✅ Result: {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Failed: {ex.Message}");
            }

            // 2. GetLinesAsync
            Console.WriteLine($"Step B: Get Lines (Hardcoded Logic)");
            string query2 = @"
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
            
             try
            {
                using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand(query2, connection);
                command.Parameters.AddWithValue("@PoNumber", poNumber);
                using var reader = await command.ExecuteReaderAsync();
                while(await reader.ReadAsync()) { }
                Console.WriteLine($"   ✅ Query executed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Failed: {ex.Message}");
            }
        }

        static async Task TestCorrectedRoutingQueries(string poNumber)
        {
             // 1. Corrected ValidatePOAsync
            Console.WriteLine($"Step A: Validate PO (Corrected Logic)");
            // Fixes: PO_ID -> ID, SITE_REF -> SITE_ID ('MTM2'), STATUS IN ('O','P','R')
            string query1 = @"
                    SELECT COUNT(*)
                    FROM dbo.PURCHASE_ORDER WITH (NOLOCK)
                    WHERE ID = @PoNumber
                      AND SITE_ID = 'MTM2'
                      AND STATUS IN ('O', 'P', 'R')"; 
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand(query1, connection);
                command.Parameters.AddWithValue("@PoNumber", poNumber);
                var count = (int?)await command.ExecuteScalarAsync() ?? 0;
                Console.WriteLine($"   ✅ Result: {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Failed: {ex.Message}");
            }

            // 2. Corrected GetLinesAsync
            Console.WriteLine($"Step B: Get Lines (Corrected Logic)");
            string query2 = @"
                SELECT 
                    pol.PURC_ORDER_ID AS PO_ID,
                    pol.LINE_NO AS PO_LINE,
                    pol.PART_ID,
                    pol.ORDER_QTY AS QTY_ORDERED,
                    pol.TOTAL_RECEIVED_QTY AS QTY_RECEIVED,
                    pol.UNIT_PRICE,
                    pol.LINE_STATUS AS STATUS,
                    p.DESCRIPTION AS PART_NAME,
                    po.VENDOR_ID
                FROM dbo.PURC_ORDER_LINE pol WITH (NOLOCK)
                INNER JOIN dbo.PURCHASE_ORDER po WITH (NOLOCK) ON pol.PURC_ORDER_ID = po.ID
                LEFT JOIN dbo.PART p WITH (NOLOCK) ON pol.PART_ID = p.ID
                WHERE pol.PURC_ORDER_ID = @PoNumber
                  AND po.SITE_ID = 'MTM2'
                ORDER BY pol.LINE_NO";
            
             try
            {
                using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand(query2, connection);
                command.Parameters.AddWithValue("@PoNumber", poNumber);
                
                using var reader = await command.ExecuteReaderAsync();
                int rows = 0;
                Console.WriteLine("   Columns: PO_ID, PO_LINE, PART_ID, QTY_ORDERED, QTY_RECEIVED, STATUS");
                while(await reader.ReadAsync()) 
                { 
                    rows++;
                    Console.WriteLine($"   Row {rows}: {reader["PO_ID"]}, {reader["PO_LINE"]}, {reader["PART_ID"]}, {reader["QTY_ORDERED"]}, {reader["QTY_RECEIVED"]}, {reader["STATUS"]}");
                }
                Console.WriteLine($"   ✅ Query executed successfully. Retrieved {rows} rows.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Failed: {ex.Message}");
            }
        }

        static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
        }
    }
}
