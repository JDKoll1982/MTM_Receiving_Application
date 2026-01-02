using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Data.InforVisual;
using MTM_Receiving_Application.Data.Receiving;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Receiving;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IService_LoggingUtility? _logger;
        private readonly Dao_InforVisualPO _daoPO;
        private readonly Dao_InforVisualPart _daoPart;
        private const string DefaultServer = "VISUAL";
        private const string DefaultDatabase = "MTMFG";
        private const string DefaultUsername = "SHOP2";
        private const string DefaultPassword = "SHOP";

        public Service_InforVisual(
            IService_UserSessionManager sessionManager,
            Dao_InforVisualPO daoPO,
            Dao_InforVisualPart daoPart,
            IService_LoggingUtility? logger = null)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _daoPO = daoPO ?? throw new ArgumentNullException(nameof(daoPO));
            _daoPart = daoPart ?? throw new ArgumentNullException(nameof(daoPart));
            _logger = logger;
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
            {
                return Model_Dao_Result_Factory.Failure<Model_InforVisualPO?>("PO number cannot be null or empty");
            }

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
            return Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(debugPO);
#else
            try
            {
                _logger?.LogInfo($"Querying Infor Visual for PO: {poNumber} (Clean: {cleanPoNumber})");
                
                var result = await _daoPO.GetByPoNumberAsync(cleanPoNumber);
                
                if (result.IsSuccess && result.Data?.Count > 0)
                {
                    // Convert List<InforVisual.Model_InforVisualPO> to Receiving.Model_InforVisualPO
                    var firstRecord = result.Data[0];
                    var po = new Model_InforVisualPO
                    {
                        PONumber = firstRecord.PoNumber,
                        Vendor = firstRecord.VendorName,
                        Status = firstRecord.PoStatus,
                        Parts = result.Data.ConvertAll(line => new Model_InforVisualPart
                        {
                            PartID = line.PartNumber,
                            Description = line.PartDescription,
                            QtyOrdered = (int)line.OrderedQty,
                            RemainingQuantity = (int)line.RemainingQty,
                            POLineNumber = line.PoLine.ToString(),
                            PartType = "FG", // Default, could be enhanced
                            UnitOfMeasure = line.UnitOfMeasure
                        })
                    };
                    
                    _logger?.LogInfo($"Successfully retrieved PO {poNumber} with {po.Parts.Count} parts");
                    return Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(po);
                }
                else if (result.IsSuccess && (result.Data == null || result.Data.Count == 0))
                {
                    _logger?.LogWarning($"PO {poNumber} not found or has no parts");
                    return Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(null);
                }
                else
                {
                    _logger?.LogError($"Failed to query Infor Visual for PO {poNumber}: {result.ErrorMessage}");
                    return Model_Dao_Result_Factory.Failure<Model_InforVisualPO?>(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error querying PO {poNumber}: {ex.Message}", ex);
                return Model_Dao_Result_Factory.Failure<Model_InforVisualPO?>($"Unexpected error querying PO {poNumber}: {ex.Message}", ex);
            }
#endif
        }

        public async Task<Model_Dao_Result<Model_InforVisualPart?>> GetPartByIDAsync(string partID)
        {
            if (string.IsNullOrWhiteSpace(partID))
            {
                return Model_Dao_Result_Factory.Failure<Model_InforVisualPart?>("Part ID cannot be null or empty");
            }

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
            return Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(debugPart);
#else
            try
            {
                _logger?.LogInfo($"Querying Infor Visual for Part: {partID}");
                
                var result = await _daoPart.GetByPartNumberAsync(partID);
                
                if (result.IsSuccess && result.Data != null)
                {
                    // Convert InforVisual.Model_InforVisualPart to Receiving.Model_InforVisualPart
                    var part = new Model_InforVisualPart
                    {
                        PartID = result.Data.PartNumber,
                        Description = result.Data.Description,
                        PartType = result.Data.PartType,
                        UnitOfMeasure = result.Data.PrimaryUom,
                        QtyOrdered = 0, // Not applicable for non-PO parts
                        RemainingQuantity = (int)result.Data.AvailableQty,
                        POLineNumber = "N/A"
                    };
                    
                    _logger?.LogInfo($"Successfully retrieved Part {partID}");
                    return Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(part);
                }
                else if (result.IsSuccess && result.Data == null)
                {
                    _logger?.LogWarning($"Part {partID} not found in Infor Visual");
                    return Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(null);
                }
                else
                {
                    _logger?.LogError($"Failed to query Infor Visual for Part {partID}: {result.ErrorMessage}");
                    return Model_Dao_Result_Factory.Failure<Model_InforVisualPart?>(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error querying Part {partID}: {ex.Message}", ex);
                return Model_Dao_Result_Factory.Failure<Model_InforVisualPart?>($"Unexpected error querying part {partID}: {ex.Message}", ex);
            }
#endif
        }

        public async Task<Model_Dao_Result<decimal>> GetSameDayReceivingQuantityAsync(string poNumber, string partID, DateTime date)
        {
#if DEBUG
            _logger?.LogInfo($"[DEBUG MODE] Bypassing Infor Visual same-day receiving check for PO: {poNumber}, Part: {partID}");
            return Model_Dao_Result_Factory.Success<decimal>(0);
#else
            try
            {
                _logger?.LogInfo($"Checking same-day receiving for PO: {poNumber}, Part: {partID}, Date: {date:yyyy-MM-dd}");
                // Note: Same-day receiving check not implemented in Infor Visual DAO
                // This would require a complex query joining receiving transactions
                _logger?.LogWarning("Same-day receiving check not implemented for Release mode - returning 0");
                return Model_Dao_Result_Factory.Success<decimal>(0);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error in same-day receiving check: {ex.Message}", ex);
                return Model_Dao_Result_Factory.Failure<decimal>($"Unexpected error: {ex.Message}", ex);
            }
#endif
        }

        public async Task<Model_Dao_Result<int>> GetRemainingQuantityAsync(string poNumber, string partID)
        {
            if (string.IsNullOrWhiteSpace(poNumber))
            {
                return Model_Dao_Result_Factory.Failure<int>("PO number cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(partID))
            {
                return Model_Dao_Result_Factory.Failure<int>("Part ID cannot be null or empty");
            }

#if DEBUG
            _logger?.LogInfo($"[DEBUG MODE] Bypassing Infor Visual remaining quantity check for PO: {poNumber}, Part: {partID}");
            return Model_Dao_Result_Factory.Success<int>(100);
#else
            try
            {
                _logger?.LogInfo($"Calculating remaining quantity for PO: {poNumber}, Part: {partID}");
                
                var result = await _daoPO.GetByPoNumberAsync(poNumber);
                
                if (result.IsSuccess && result.Data?.Count > 0)
                {
                    var matchingLine = result.Data.FirstOrDefault(line => line.PartNumber == partID);
                    if (matchingLine != null)
                    {
                        int remaining = (int)matchingLine.RemainingQty;
                        _logger?.LogInfo($"Remaining quantity for PO {poNumber}, Part {partID}: {remaining}");
                        return Model_Dao_Result_Factory.Success<int>(remaining);
                    }
                    else
                    {
                        _logger?.LogWarning($"Part {partID} not found on PO {poNumber}");
                        return Model_Dao_Result_Factory.Failure<int>("Part not found on PO");
                    }
                }
                else
                {
                    _logger?.LogWarning($"Failed to retrieve PO: {result?.ErrorMessage}");
                    return Model_Dao_Result_Factory.Failure<int>(result?.ErrorMessage ?? "PO not found");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Unexpected error calculating remaining quantity: {ex.Message}", ex);
                return Model_Dao_Result_Factory.Failure<int>($"Unexpected error: {ex.Message}", ex);
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
                // Use a simple validation query to test connection
                var testResult = await _daoPO.ValidatePoNumberAsync("000001");
                bool isConnected = testResult.IsSuccess;
                _logger?.LogInfo($"Infor Visual connection test result: {isConnected}");
                return isConnected;
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

