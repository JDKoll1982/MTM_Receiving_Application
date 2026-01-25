using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Core.Services.Database;

/// <summary>
/// Service for connecting to and querying Infor Visual database (SQL Server)
/// ⚠️ STRICTLY READ ONLY - NO WRITES ALLOWED TO INFOR VISUAL ⚠️
/// Server: VISUAL, Database: MTMFG, Warehouse: 002
/// </summary>
public class Service_InforVisualConnect : IService_InforVisual
{
    private readonly Dao_InforVisualConnection _dao;
    private readonly IService_LoggingUtility? _logger;
    private readonly bool _useMockData;

    public Service_InforVisualConnect(
        Dao_InforVisualConnection dao,
        bool useMockData = false,
        IService_LoggingUtility? logger = null)
    {
        _dao = dao ?? throw new ArgumentNullException(nameof(dao));
        _useMockData = useMockData;
        _logger = logger;
    }

    #region Connection Testing

    public async Task<bool> TestConnectionAsync()
    {
        if (_useMockData)
        {
            _logger?.LogInfo("[MOCK DATA MODE] Simulating successful Infor Visual connection test");
            return true;
        }

        try
        {
            _logger?.LogInfo("Testing Infor Visual connection...");
            var result = await _dao.TestConnectionAsync();

            if (result.IsSuccess)
            {
                _logger?.LogInfo($"Infor Visual connection test: {(result.Data ? "SUCCESS" : "FAILED")}");
                return result.Data;
            }

            _logger?.LogError($"Connection test failed: {result.ErrorMessage}");
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Connection test exception: {ex.Message}", ex);
            return false;
        }
    }

    #endregion

    #region Purchase Order Operations

    public async Task<Model_Dao_Result<Model_Receiving_DTO_InforVisualPO?>> GetPOWithPartsAsync(string poNumber)
    {
        if (string.IsNullOrWhiteSpace(poNumber))
        {
            return Model_Dao_Result_Factory.Failure<Model_Receiving_DTO_InforVisualPO?>(
                "PO number cannot be null or empty");
        }

        // Use the PO number as provided - Infor Visual IDs include the prefix (e.g. "PO-123456")
        string cleanPoNumber = poNumber;

        if (_useMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock data for PO: {cleanPoNumber}");
            return CreateMockPO(cleanPoNumber);
        }

        try
        {
            _logger?.LogInfo($"Querying Infor Visual for PO: {cleanPoNumber}");

            var result = await _dao.GetPOWithPartsAsync(cleanPoNumber);

            if (!result.IsSuccess)
            {
                _logger?.LogError($"Failed to retrieve PO {cleanPoNumber}: {result.ErrorMessage}");
                return Model_Dao_Result_Factory.Failure<Model_Receiving_DTO_InforVisualPO?>(result.ErrorMessage);
            }

            if (result.Data == null || result.Data.Count == 0)
            {
                _logger?.LogWarning($"PO {cleanPoNumber} not found");
                return Model_Dao_Result_Factory.Success<Model_Receiving_DTO_InforVisualPO?>(null);
            }

            // Convert flat DAO model to hierarchical service model
            var po = ConvertToServiceModel(result.Data);
            _logger?.LogInfo($"Successfully retrieved PO {cleanPoNumber} with {po.Parts.Count} line items");

            return Model_Dao_Result_Factory.Success<Model_Receiving_DTO_InforVisualPO?>(po);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Unexpected error querying PO {cleanPoNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_Receiving_DTO_InforVisualPO?>(
                $"Unexpected error: {ex.Message}", ex);
        }
    }

    #endregion

    #region Part Operations

    public async Task<Model_Dao_Result<Model_Receiving_DTO_InforVisualPart?>> GetPartByIDAsync(string partID)
    {
        if (string.IsNullOrWhiteSpace(partID))
        {
            return Model_Dao_Result_Factory.Failure<Model_Receiving_DTO_InforVisualPart?>(
                "Part ID cannot be null or empty");
        }

        if (_useMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock data for Part: {partID}");
            return CreateMockPart(partID);
        }

        try
        {
            _logger?.LogInfo($"Querying Infor Visual for Part: {partID}");

            var result = await _dao.GetPartByNumberAsync(partID);

            if (!result.IsSuccess)
            {
                _logger?.LogError($"Failed to retrieve Part {partID}: {result.ErrorMessage}");
                return Model_Dao_Result_Factory.Failure<Model_Receiving_DTO_InforVisualPart?>(result.ErrorMessage);
            }

            if (result.Data == null)
            {
                _logger?.LogWarning($"Part {partID} not found");
                return Model_Dao_Result_Factory.Success<Model_Receiving_DTO_InforVisualPart?>(null);
            }

            // Convert DAO model to service model
            var part = ConvertPartToServiceModel(result.Data);
            _logger?.LogInfo($"Successfully retrieved Part {partID}");

            return Model_Dao_Result_Factory.Success<Model_Receiving_DTO_InforVisualPart?>(part);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Unexpected error querying Part {partID}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_Receiving_DTO_InforVisualPart?>(
                $"Unexpected error: {ex.Message}", ex);
        }
    }

    #endregion

    #region Quantity Calculations

    public async Task<Model_Dao_Result<decimal>> GetSameDayReceivingQuantityAsync(
        string poNumber,
        string partID,
        DateTime date)
    {
        if (_useMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning 0 for same-day receiving");
            return Model_Dao_Result_Factory.Success<decimal>(0);
        }

        // This functionality requires custom query/stored procedure in Infor Visual
        // Not implemented in current SQL files
        _logger?.LogWarning("Same-day receiving quantity not implemented - returning 0");
        return Model_Dao_Result_Factory.Success<decimal>(0);
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

        if (_useMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock remaining quantity: 100");
            return Model_Dao_Result_Factory.Success<int>(100);
        }

        try
        {
            _logger?.LogInfo($"Calculating remaining quantity for PO: {poNumber}, Part: {partID}");

            var result = await _dao.GetPOWithPartsAsync(poNumber);

            if (!result.IsSuccess || result.Data == null || result.Data.Count == 0)
            {
                _logger?.LogWarning($"Failed to retrieve PO {poNumber}");
                return Model_Dao_Result_Factory.Failure<int>("PO not found");
            }

            var matchingLine = result.Data.FirstOrDefault(line =>
                line.PartNumber.Equals(partID, StringComparison.OrdinalIgnoreCase));

            if (matchingLine == null)
            {
                _logger?.LogWarning($"Part {partID} not found on PO {poNumber}");
                return Model_Dao_Result_Factory.Failure<int>("Part not found on PO");
            }

            int remaining = (int)matchingLine.RemainingQty;
            _logger?.LogInfo($"Remaining quantity: {remaining}");

            return Model_Dao_Result_Factory.Success<int>(remaining);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Unexpected error calculating remaining quantity: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<int>($"Unexpected error: {ex.Message}", ex);
        }
    }

    #endregion

    #region Helper Methods - Model Conversion

    /// <summary>
    /// Converts flat DAO PO lines to hierarchical service model
    /// </summary>
    /// <param name="poLines"></param>
    private Model_Receiving_DTO_InforVisualPO ConvertToServiceModel(List<Model_InforVisualPO> poLines)
    {
        var firstLine = poLines[0];

        return new Model_Receiving_DTO_InforVisualPO
        {
            PONumber = firstLine.PoNumber,
            Vendor = firstLine.VendorName,
            Status = firstLine.PoStatus,
            Parts = poLines.ConvertAll(line => new Model_Receiving_DTO_InforVisualPart
            {
                PartID = line.PartNumber,
                Description = line.PartDescription,
                POLineNumber = line.PoLine.ToString(),
                PartType = "FG", // Default - could be enhanced with additional query
                QtyOrdered = (int)line.OrderedQty,
                RemainingQuantity = (int)line.RemainingQty,
                UnitOfMeasure = line.UnitOfMeasure
            })
        };
    }

    /// <summary>
    /// Converts DAO part model to service model
    /// </summary>
    /// <param name="daoPart"></param>
    private Model_Receiving_DTO_InforVisualPart ConvertPartToServiceModel(Model_InforVisualPart daoPart)
    {
        return new Model_Receiving_DTO_InforVisualPart
        {
            PartID = daoPart.PartNumber,
            Description = daoPart.Description,
            POLineNumber = "N/A",
            PartType = "FG", // Default
            QtyOrdered = 0,
            RemainingQuantity = (int)daoPart.AvailableQty,
            UnitOfMeasure = daoPart.PrimaryUom
        };
    }

    #endregion

    #region Mock Data Generation

    private Model_Dao_Result<Model_Receiving_DTO_InforVisualPO?> CreateMockPO(string poNumber)
    {
        var mockPO = new Model_Receiving_DTO_InforVisualPO
        {
            PONumber = poNumber,
            Vendor = "MOCK_VENDOR",
            Status = "O",
            Parts = new List<Model_Receiving_DTO_InforVisualPart>
            {
                new Model_Receiving_DTO_InforVisualPart
                {
                    PartID = "MOCK-PART-001",
                    POLineNumber = "1",
                    PartType = "RAW",
                    QtyOrdered = 100,
                    Description = "Mock Part 1 Description",
                    RemainingQuantity = 50,
                    UnitOfMeasure = "EA"
                },
                new Model_Receiving_DTO_InforVisualPart
                {
                    PartID = "MOCK-PART-002",
                    POLineNumber = "2",
                    PartType = "FG",
                    QtyOrdered = 50,
                    Description = "Mock Part 2 Description",
                    RemainingQuantity = 10,
                    UnitOfMeasure = "EA"
                }
            }
        };

        return Model_Dao_Result_Factory.Success<Model_Receiving_DTO_InforVisualPO?>(mockPO);
    }

    private Model_Dao_Result<Model_Receiving_DTO_InforVisualPart?> CreateMockPart(string partID)
    {
        var mockPart = new Model_Receiving_DTO_InforVisualPart
        {
            PartID = partID,
            PartType = "MOCK_TYPE",
            Description = "Mock Part Description",
            POLineNumber = "N/A",
            QtyOrdered = 0,
            RemainingQuantity = 100,
            UnitOfMeasure = "EA"
        };

        return Model_Dao_Result_Factory.Success<Model_Receiving_DTO_InforVisualPart?>(mockPart);
    }

    #endregion
}

