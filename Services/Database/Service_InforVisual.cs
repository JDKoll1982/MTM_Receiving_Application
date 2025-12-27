using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Data.Receiving;
using MTM_Receiving_Application.Models.Core;
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
                return DaoResultFactory.Failure<Model_InforVisualPO?>("PO number cannot be null or empty");

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
            return DaoResultFactory.Success<Model_InforVisualPO?>(debugPO);
#else
            try
            {
                _logger?.LogInfo($"Querying Infor Visual for PO: {poNumber} (Clean: {cleanPoNumber})");
                
                // Instantiate DAO with dynamic connection string
                var dao = new Dao_InforVisualPO(GetConnectionString());
                var result = await dao.GetPOWithPartsAsync(cleanPoNumber);
                
                if (result.IsSuccess && result.Data != null)
                {
                    _logger?.LogInfo($"Successfully retrieved PO {poNumber} with {result.Data.Parts.Count} parts");
                }
                else if (result.IsSuccess && result.Data == null)
                {
                    _logger?.LogWarning($"PO {poNumber} found but has no parts or no results returned");
                }
                else
                {
                    _logger?.LogError($"Failed to query Infor Visual for PO {poNumber}: {result.ErrorMessage}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error querying PO {poNumber}: {ex.Message}", ex);
                return DaoResultFactory.Failure<Model_InforVisualPO?>($"Unexpected error querying PO {poNumber}: {ex.Message}", ex);
            }
#endif
        }

        public async Task<Model_Dao_Result<Model_InforVisualPart?>> GetPartByIDAsync(string partID)
        {
            if (string.IsNullOrWhiteSpace(partID))
                return DaoResultFactory.Failure<Model_InforVisualPart?>("Part ID cannot be null or empty");

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
            return DaoResultFactory.Success<Model_InforVisualPart?>(debugPart);
#else
            try
            {
                _logger?.LogInfo($"Querying Infor Visual for Part: {partID}");
                
                // Instantiate DAO with dynamic connection string
                var dao = new Dao_InforVisualPart(GetConnectionString());
                var result = await dao.GetPartByIdAsync(partID);
                
                if (result.IsSuccess && result.Data != null)
                {
                    _logger?.LogInfo($"Successfully retrieved Part {partID}");
                }
                else if (result.IsSuccess && result.Data == null)
                {
                    _logger?.LogWarning($"Part {partID} not found in Infor Visual");
                }
                else
                {
                    _logger?.LogError($"Failed to query Infor Visual for Part {partID}: {result.ErrorMessage}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error querying Part {partID}: {ex.Message}", ex);
                return DaoResultFactory.Failure<Model_InforVisualPart?>($"Unexpected error querying Part {partID}: {ex.Message}", ex);
            }
#endif
        }

        public async Task<Model_Dao_Result<decimal>> GetSameDayReceivingQuantityAsync(string poNumber, string partID, DateTime date)
        {
#if DEBUG
            _logger?.LogInfo($"[DEBUG MODE] Bypassing Infor Visual same-day receiving check for PO: {poNumber}, Part: {partID}");
            return DaoResultFactory.Success<decimal>(0);
#else
            try
            {
                _logger?.LogInfo($"Checking same-day receiving for PO: {poNumber}, Part: {partID}, Date: {date:yyyy-MM-dd}");
                
                // Instantiate DAO with dynamic connection string
                var dao = new Dao_InforVisualPO(GetConnectionString());
                var result = await dao.GetSameDayReceivingQuantityAsync(poNumber, partID, date);
                
                if (result.IsSuccess)
                {
                    _logger?.LogInfo($"Same-day receiving check result: {result.Data} for PO {poNumber}, Part {partID}");
                }
                else
                {
                    _logger?.LogError($"Failed to query same-day receiving for PO {poNumber}, Part {partID}: {result.ErrorMessage}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error checking same-day receiving: {ex.Message}", ex);
                return DaoResultFactory.Failure<decimal>($"Unexpected error querying same-day receiving: {ex.Message}", ex);
            }
#endif
        }

        public async Task<Model_Dao_Result<int>> GetRemainingQuantityAsync(string poNumber, string partID)
        {
            if (string.IsNullOrWhiteSpace(poNumber))
                return DaoResultFactory.Failure<int>("PO number cannot be null or empty");

            if (string.IsNullOrWhiteSpace(partID))
                return DaoResultFactory.Failure<int>("Part ID cannot be null or empty");

#if DEBUG
            _logger?.LogInfo($"[DEBUG MODE] Bypassing Infor Visual remaining quantity check for PO: {poNumber}, Part: {partID}");
            return DaoResultFactory.Success<int>(100);
#else
            try
            {
                _logger?.LogInfo($"Calculating remaining quantity for PO: {poNumber}, Part: {partID}");
                
                // Instantiate DAO with dynamic connection string
                var dao = new Dao_InforVisualPO(GetConnectionString());
                var result = await dao.GetRemainingQuantityAsync(poNumber, partID);
                
                if (result.IsSuccess)
                {
                    _logger?.LogInfo($"Remaining quantity for PO {poNumber}, Part {partID}: {result.Data}");
                }
                else
                {
                    _logger?.LogWarning($"Failed to calculate remaining quantity: {result.ErrorMessage}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error calculating remaining quantity: {ex.Message}", ex);
                return DaoResultFactory.Failure<int>($"Unexpected error calculating remaining quantity: {ex.Message}", ex);
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
                // Use DAO to test connection? DAO doesn't have TestConnection.
                // But we can try a simple query or just open connection.
                // Since we are refactoring to use DAOs, we should ideally use DAO.
                // But for TestConnection, we might just want to check if we can connect.
                // I'll keep the direct connection check here as it's a utility method, 
                // OR I can add TestConnection to DAO.
                // Given I can't easily modify DAO right now (already did), I'll keep the direct check 
                // BUT I must use Microsoft.Data.SqlClient (or System.Data.SqlClient) which was used before.
                // The original file used `using System.Data.SqlClient;` (implied by `SqlConnection`).
                // I need to make sure I have the using directive.
                
                using var connection = new System.Data.SqlClient.SqlConnection(GetConnectionString());
                await connection.OpenAsync();
                bool isOpen = connection.State == System.Data.ConnectionState.Open;
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

