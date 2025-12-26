using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Services.Database
{
    /// <summary>
    /// Service for querying Infor Visual database (SQL Server).
    /// ⚠️ STRICTLY READ ONLY - NO WRITES ALLOWED TO INFOR VISUAL ⚠️
    /// Server: VISUAL, Database: MTMFG, Warehouse: 002
    /// </summary>
    public class Service_InforVisual : IService_InforVisual
    {
        private readonly IService_UserSessionManager _sessionManager;
        private readonly ILoggingService? _logger;
        private const string DefaultServer = "VISUAL";
        private const string DefaultDatabase = "MTMFG";
        private const string DefaultUsername = "SHOP2";
        private const string DefaultPassword = "SHOP";

        public Service_InforVisual(IService_UserSessionManager sessionManager, ILoggingService? logger = null)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _logger = logger;
        }

        /// <summary>
        /// Builds connection string dynamically based on current user session.
        /// </summary>
        private string GetConnectionString()
        {
            var user = _sessionManager.CurrentSession?.User;
            var username = !string.IsNullOrWhiteSpace(user?.VisualUsername) ? user.VisualUsername : DefaultUsername;
            var password = !string.IsNullOrWhiteSpace(user?.VisualPassword) ? user.VisualPassword : DefaultPassword;

            return $"Server={DefaultServer};Database={DefaultDatabase};User Id={username};Password={password};TrustServerCertificate=True;ApplicationIntent=ReadOnly;";
        }

        /// <summary>
        /// Builds default connection string for Infor Visual (READ ONLY).
        /// </summary>
        public static string BuildDefaultConnectionString()
        {
            return $"Server={DefaultServer};Database={DefaultDatabase};User Id={DefaultUsername};Password={DefaultPassword};TrustServerCertificate=True;ApplicationIntent=ReadOnly;";
        }

        public async Task<Model_Dao_Result<Model_InforVisualPO?>> GetPOWithPartsAsync(string poNumber)
        {
            if (string.IsNullOrWhiteSpace(poNumber))
                return Model_Dao_Result<Model_InforVisualPO?>.Failure("PO number cannot be null or empty");

            // Strip "PO-" prefix if present for database querying/debug logic
            string cleanPoNumber = poNumber;
            if (poNumber.StartsWith("PO-", StringComparison.OrdinalIgnoreCase))
            {
                cleanPoNumber = poNumber.Substring(3);
            }

#if DEBUG
            _logger?.LogInfo($"[DEBUG MODE] Bypassing Infor Visual query for PO: {poNumber} (Clean: {cleanPoNumber})");
            var debugPO = new Model_InforVisualPO
            {
                PONumber = cleanPoNumber, // Return the clean number
                Vendor = "DEBUG_VENDOR",
                Status = "O", // Open
                Parts = new List<Model_InforVisualPart>
                {
                    new Model_InforVisualPart
                    {
                        PartID = "DEBUG-PART-001",
                        POLineNumber = "1",
                        PartType = "RAW",
                        QtyOrdered = 100,
                        Description = "Debug Part 1 Description",
                        RemainingQuantity = 50
                    },
                    new Model_InforVisualPart
                    {
                        PartID = "DEBUG-PART-002",
                        POLineNumber = "2",
                        PartType = "FG",
                        QtyOrdered = 50,
                        Description = "Debug Part 2 Description",
                        RemainingQuantity = 10
                    }
                }
            };
            return Model_Dao_Result<Model_InforVisualPO?>.SuccessResult(debugPO);
#else
            try
            {
                _logger?.LogInfo($"Querying Infor Visual for PO: {poNumber} (Clean: {cleanPoNumber})");
                using var connection = new SqlConnection(GetConnectionString());
                
                const string query = @"
                    SELECT 
                        pol.PART_ID AS PartID,
                        CAST(pol.LINE_NO AS VARCHAR(10)) AS POLineNumber,
                        ISNULL(p.PRODUCT_CODE, 'UNKNOWN') AS PartType,
                        pol.ORDER_QTY AS QtyOrdered,
                        ISNULL(p.DESCRIPTION, '') AS Description,
                        po.VENDOR_ID AS VendorID,
                        pol.TOTAL_RECEIVED_QTY AS TotalReceivedQty,
                        po.STATUS AS POStatus
                    FROM 
                        dbo.PURC_ORDER_LINE pol
                    LEFT JOIN 
                        dbo.PART p ON pol.PART_ID = p.ID
                    INNER JOIN
                        dbo.PURCHASE_ORDER po ON pol.PURC_ORDER_ID = po.ID
                    WHERE 
                        po.ID = @PONumber
                        -- AND pol.LINE_STATUS != 'X'  -- Relaxed for testing
                        -- Allow all PO statuses including Closed
                    ORDER BY 
                        pol.LINE_NO;";

                using var command = new SqlCommand(query, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 30
                };

                command.Parameters.AddWithValue("@PONumber", cleanPoNumber);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var po = new Model_InforVisualPO
                {
                    PONumber = cleanPoNumber,
                    Parts = new List<Model_InforVisualPart>()
                };

                while (await reader.ReadAsync())
                {
                    po.Parts.Add(new Model_InforVisualPart
                    {
                        PartID = !reader.IsDBNull(reader.GetOrdinal("PartID")) ? reader.GetString(reader.GetOrdinal("PartID")) : string.Empty,
                        POLineNumber = !reader.IsDBNull(reader.GetOrdinal("POLineNumber")) ? reader.GetString(reader.GetOrdinal("POLineNumber")) : string.Empty,
                        PartType = !reader.IsDBNull(reader.GetOrdinal("PartType")) ? reader.GetString(reader.GetOrdinal("PartType")) : "UNKNOWN",
                        QtyOrdered = !reader.IsDBNull(reader.GetOrdinal("QtyOrdered")) ? reader.GetDecimal(reader.GetOrdinal("QtyOrdered")) : 0m,
                        Description = !reader.IsDBNull(reader.GetOrdinal("Description")) ? reader.GetString(reader.GetOrdinal("Description")) : string.Empty
                    });

                    if (po.Vendor == string.Empty && !reader.IsDBNull(reader.GetOrdinal("VendorID")))
                    {
                        po.Vendor = reader.GetString(reader.GetOrdinal("VendorID"));
                    }
                    
                    if (po.Status == string.Empty && !reader.IsDBNull(reader.GetOrdinal("POStatus")))
                    {
                        po.Status = reader.GetString(reader.GetOrdinal("POStatus"));
                    }
                }

                if (po.HasParts)
                {
                    _logger?.LogInfo($"Successfully retrieved PO {poNumber} with {po.Parts.Count} parts");
                    return Model_Dao_Result<Model_InforVisualPO?>.SuccessResult(po);
                }
                else
                {
                    _logger?.LogWarning($"PO {poNumber} found but has no parts or no results returned");
                    return Model_Dao_Result<Model_InforVisualPO?>.SuccessResult(null);
                }
            }
            catch (SqlException ex)
            {
                _logger?.LogError($"SQL Error querying Infor Visual for PO {poNumber}: {ex.Message}", ex);
                return Model_Dao_Result<Model_InforVisualPO?>.Failure($"Failed to query Infor Visual for PO {poNumber}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error querying PO {poNumber}: {ex.Message}", ex);
                return Model_Dao_Result<Model_InforVisualPO?>.Failure($"Unexpected error querying PO {poNumber}: {ex.Message}", ex);
            }
#endif
        }

        public async Task<Model_Dao_Result<Model_InforVisualPart?>> GetPartByIDAsync(string partID)
        {
            if (string.IsNullOrWhiteSpace(partID))
                return Model_Dao_Result<Model_InforVisualPart?>.Failure("Part ID cannot be null or empty");

#if DEBUG
            _logger?.LogInfo($"[DEBUG MODE] Bypassing Infor Visual query for Part: {partID}");
            var debugPart = new Model_InforVisualPart
            {
                PartID = partID,
                PartType = "DEBUG_TYPE",
                Description = "Debug Part Description",
                POLineNumber = "N/A",
                QtyOrdered = 0
            };
            return Model_Dao_Result<Model_InforVisualPart?>.SuccessResult(debugPart);
#else
            try
            {
                _logger?.LogInfo($"Querying Infor Visual for Part: {partID}");
                using var connection = new SqlConnection(GetConnectionString());
                
                const string query = @"
                    SELECT 
                        p.ID AS PartID,
                        ISNULL(p.PRODUCT_CODE, 'UNKNOWN') AS PartType,
                        ISNULL(p.DESCRIPTION, '') AS Description,
                        p.STOCK_UM AS UnitOfMeasure,
                        CASE 
                            WHEN p.INVENTORY_LOCKED = 'Y' THEN 'LOCKED'
                            WHEN p.STOCKED = 'Y' THEN 'ACTIVE'
                            ELSE 'INACTIVE'
                        END AS Status
                    FROM 
                        dbo.PART p
                    WHERE 
                        p.ID = @PartID
                        AND p.INVENTORY_LOCKED != 'Y';";

                using var command = new SqlCommand(query, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 30
                };

                command.Parameters.AddWithValue("@PartID", partID);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var part = new Model_InforVisualPart
                    {
                        PartID = !reader.IsDBNull(reader.GetOrdinal("PartID")) ? reader.GetString(reader.GetOrdinal("PartID")) : string.Empty,
                        PartType = !reader.IsDBNull(reader.GetOrdinal("PartType")) ? reader.GetString(reader.GetOrdinal("PartType")) : "UNKNOWN",
                        Description = !reader.IsDBNull(reader.GetOrdinal("Description")) ? reader.GetString(reader.GetOrdinal("Description")) : string.Empty,
                        POLineNumber = "N/A",
                        QtyOrdered = 0
                    };
                    _logger?.LogInfo($"Successfully retrieved Part {partID}");
                    return Model_Dao_Result<Model_InforVisualPart?>.SuccessResult(part);
                }

                _logger?.LogWarning($"Part {partID} not found in Infor Visual");
                return Model_Dao_Result<Model_InforVisualPart?>.SuccessResult(null);
            }
            catch (SqlException ex)
            {
                _logger?.LogError($"SQL Error querying Infor Visual for Part {partID}: {ex.Message}", ex);
                return Model_Dao_Result<Model_InforVisualPart?>.Failure($"Failed to query Infor Visual for Part {partID}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error querying Part {partID}: {ex.Message}", ex);
                return Model_Dao_Result<Model_InforVisualPart?>.Failure($"Unexpected error querying Part {partID}: {ex.Message}", ex);
            }
#endif
        }

        public async Task<Model_Dao_Result<decimal>> GetSameDayReceivingQuantityAsync(string poNumber, string partID, DateTime date)
        {
#if DEBUG
            _logger?.LogInfo($"[DEBUG MODE] Bypassing Infor Visual same-day receiving check for PO: {poNumber}, Part: {partID}");
            return Model_Dao_Result<decimal>.SuccessResult(0);
#else
            try
            {
                _logger?.LogInfo($"Checking same-day receiving for PO: {poNumber}, Part: {partID}, Date: {date:yyyy-MM-dd}");
                using var connection = new SqlConnection(GetConnectionString());
                
                const string query = @"
                    SELECT 
                        ISNULL(SUM(it.QTY), 0) AS TotalQtyReceived,
                        COUNT(*) AS ReceiptCount,
                        MAX(it.TRANSACTION_DATE) AS LastReceiptTime
                    FROM 
                        dbo.INVENTORY_TRANS it
                    WHERE 
                        it.PURC_ORDER_ID = @PONumber
                        AND it.PART_ID = @PartID
                        AND CAST(it.TRANSACTION_DATE AS DATE) = @Date
                        AND it.TYPE = 'R'  -- Receipt transaction
                        AND it.CLASS = '1'  -- PO Receipt
                        AND it.SITE_ID = (SELECT SITE_ID FROM dbo.PURCHASE_ORDER WHERE ID = @PONumber);";

                using var command = new SqlCommand(query, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 30
                };

                command.Parameters.AddWithValue("@PONumber", poNumber);
                command.Parameters.AddWithValue("@PartID", partID);
                command.Parameters.AddWithValue("@Date", date.Date);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                decimal qty = 0;
                if (await reader.ReadAsync())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("TotalQtyReceived")))
                    {
                        qty = reader.GetDecimal(reader.GetOrdinal("TotalQtyReceived"));
                    }
                }

                _logger?.LogInfo($"Same-day receiving check result: {qty} for PO {poNumber}, Part {partID}");
                return Model_Dao_Result<decimal>.SuccessResult(qty);
            }
            catch (SqlException ex)
            {
                _logger?.LogError($"SQL Error checking same-day receiving for PO {poNumber}, Part {partID}: {ex.Message}", ex);
                return Model_Dao_Result<decimal>.Failure($"Failed to query same-day receiving for PO {poNumber}, Part {partID}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error checking same-day receiving: {ex.Message}", ex);
                return Model_Dao_Result<decimal>.Failure($"Unexpected error querying same-day receiving: {ex.Message}", ex);
            }
#endif
        }

        public async Task<Model_Dao_Result<int>> GetRemainingQuantityAsync(string poNumber, string partID)
        {
            if (string.IsNullOrWhiteSpace(poNumber))
                return Model_Dao_Result<int>.Failure("PO number cannot be null or empty");
            
            if (string.IsNullOrWhiteSpace(partID))
                return Model_Dao_Result<int>.Failure("Part ID cannot be null or empty");

#if DEBUG
            _logger?.LogInfo($"[DEBUG MODE] Bypassing Infor Visual remaining quantity check for PO: {poNumber}, Part: {partID}");
            return Model_Dao_Result<int>.SuccessResult(100);
#else
            try
            {
                _logger?.LogInfo($"Calculating remaining quantity for PO: {poNumber}, Part: {partID}");
                using var connection = new SqlConnection(GetConnectionString());
                
                const string query = @"
                    SELECT 
                        pol.ORDER_QTY - ISNULL(pol.TOTAL_RECEIVED_QTY, 0) AS RemainingQuantity
                    FROM 
                        dbo.PURC_ORDER_LINE pol
                    WHERE 
                        pol.PURC_ORDER_ID = @PONumber 
                        AND pol.PART_ID = @PartID;";

                using var command = new SqlCommand(query, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 30
                };

                command.Parameters.AddWithValue("@PONumber", poNumber);
                command.Parameters.AddWithValue("@PartID", partID);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    decimal remainingQty = Convert.ToDecimal(result);
                    int remainingQtyInt = (int)Math.Floor(remainingQty); // Whole numbers only
                    
                    _logger?.LogInfo($"Remaining quantity for PO {poNumber}, Part {partID}: {remainingQtyInt}");
                    return Model_Dao_Result<int>.SuccessResult(remainingQtyInt);
                }
                else
                {
                    _logger?.LogWarning($"No data found for PO {poNumber}, Part {partID}");
                    return Model_Dao_Result<int>.Failure($"No data found for PO {poNumber}, Part {partID}");
                }
            }
            catch (SqlException ex)
            {
                _logger?.LogError($"SQL Error calculating remaining quantity for PO {poNumber}, Part {partID}: {ex.Message}", ex);
                return Model_Dao_Result<int>.Failure($"Failed to calculate remaining quantity: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error calculating remaining quantity: {ex.Message}", ex);
                return Model_Dao_Result<int>.Failure($"Unexpected error calculating remaining quantity: {ex.Message}", ex);
            }
#endif
        }

        public async Task<bool> TestConnectionAsync()
        {
#if DEBUG
            _logger?.LogInfo("[DEBUG MODE] Bypassing Infor Visual connection test");
            return true;
#else
            try
            {
                _logger?.LogInfo("Testing Infor Visual connection...");
                using var connection = new SqlConnection(GetConnectionString());
                await connection.OpenAsync();
                bool isOpen = connection.State == ConnectionState.Open;
                _logger?.LogInfo($"Infor Visual connection test result: {isOpen}");
                return isOpen;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Infor Visual connection test failed: {ex.Message}", ex);
                return false;
            }
#endif
        }
    }
}
