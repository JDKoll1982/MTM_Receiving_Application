using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
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

    public async Task<Model_Dao_Result<Model_InforVisualPO?>> GetPOWithPartsAsync(string poNumber)
    {
        if (string.IsNullOrWhiteSpace(poNumber))
        {
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPO?>(
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
                return Model_Dao_Result_Factory.Failure<Model_InforVisualPO?>(result.ErrorMessage);
            }

            if (result.Data == null || result.Data.Count == 0)
            {
                _logger?.LogWarning($"PO {cleanPoNumber} not found");
                return Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(null);
            }

            // Convert flat DAO model to hierarchical service model
            var po = ConvertToServiceModel(result.Data);
            _logger?.LogInfo($"Successfully retrieved PO {cleanPoNumber} with {po.Parts.Count} line items");

            return Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(po);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Unexpected error querying PO {cleanPoNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPO?>(
                $"Unexpected error: {ex.Message}", ex);
        }
    }

    #endregion

    #region Part Operations

    public async Task<Model_Dao_Result<Model_InforVisualPart?>> GetPartByIDAsync(string partID)
    {
        if (string.IsNullOrWhiteSpace(partID))
        {
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPart?>(
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
                return Model_Dao_Result_Factory.Failure<Model_InforVisualPart?>(result.ErrorMessage);
            }

            if (result.Data == null)
            {
                _logger?.LogWarning($"Part {partID} not found");
                return Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(null);
            }

            // Convert DAO model to service model
            var part = ConvertPartToServiceModel(result.Data);
            _logger?.LogInfo($"Successfully retrieved Part {partID}");

            return Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(part);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Unexpected error querying Part {partID}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_InforVisualPart?>(
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
    private Model_InforVisualPO ConvertToServiceModel(List<Model_InforVisualPOLine> poLines)
    {
        var firstLine = poLines[0];

        return new Model_InforVisualPO
        {
            PONumber = firstLine.PoNumber,
            Vendor = firstLine.VendorName,
            Status = firstLine.PoStatus,
            Parts = poLines.ConvertAll(line => new Model_InforVisualPart
            {
                PartID = line.PartNumber,
                Description = line.PartDescription,
                POLineNumber = line.PoLine.ToString(),
                PartType = "FG", // Default - could be enhanced with additional query
                DefaultLocationId = line.DefaultLocationId,
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
    private Model_InforVisualPart ConvertPartToServiceModel(Model_InforVisualPartInfo daoPart)
    {
        return new Model_InforVisualPart
        {
            PartID = daoPart.PartNumber,
            Description = daoPart.Description,
            POLineNumber = "N/A",
            PartType = "FG", // Default
            DefaultLocationId = daoPart.DefaultLocationId,
            QtyOrdered = 0,
            RemainingQuantity = (int)daoPart.AvailableQty,
            UnitOfMeasure = daoPart.PrimaryUom
        };
    }

    #endregion

    #region Mock Data Generation

    private Model_Dao_Result<Model_InforVisualPO?> CreateMockPO(string poNumber)
    {
        var mockPO = new Model_InforVisualPO
        {
            PONumber = poNumber,
            Vendor = "MOCK_VENDOR",
            Status = "O",
            Parts = new List<Model_InforVisualPart>
            {
                new Model_InforVisualPart
                {
                    PartID = "MOCK-PART-001",
                    POLineNumber = "1",
                    PartType = "RAW",
                    DefaultLocationId = "A-RECV-01",
                    QtyOrdered = 100,
                    Description = "Mock Part 1 Description",
                    RemainingQuantity = 50,
                    UnitOfMeasure = "EA"
                },
                new Model_InforVisualPart
                {
                    PartID = "MOCK-PART-002",
                    POLineNumber = "2",
                    PartType = "FG",
                    DefaultLocationId = "B-RECV-02",
                    QtyOrdered = 50,
                    Description = "Mock Part 2 Description",
                    RemainingQuantity = 10,
                    UnitOfMeasure = "EA"
                }
            }
        };

        return Model_Dao_Result_Factory.Success<Model_InforVisualPO?>(mockPO);
    }

    private Model_Dao_Result<Model_InforVisualPart?> CreateMockPart(string partID)
    {
        var mockPart = new Model_InforVisualPart
        {
            PartID = partID,
            PartType = "MOCK_TYPE",
            Description = "Mock Part Description",
            POLineNumber = "N/A",
            DefaultLocationId = "A-RECV-01",
            QtyOrdered = 0,
            RemainingQuantity = 100,
            UnitOfMeasure = "EA"
        };

        return Model_Dao_Result_Factory.Success<Model_InforVisualPart?>(mockPart);
    }

    private Model_Dao_Result<List<Model_OutsideServiceHistory>> CreateMockOutsideServiceHistory(string partNumber)
    {
        var records = new List<Model_OutsideServiceHistory>
        {
            new Model_OutsideServiceHistory
            {
                VendorID       = "MOCK-VENDOR-001",
                VendorName     = "Acme Heat Treating Co.",
                VendorCity     = "Detroit",
                VendorState    = "MI",
                DispatchID     = "SD-001234",
                DispatchDate   = DateTime.Today.AddMonths(-1),
                PartNumber     = partNumber,
                QuantitySent   = 25,
                DispatchStatus = "Closed"
            },
            new Model_OutsideServiceHistory
            {
                VendorID       = "MOCK-VENDOR-002",
                VendorName     = "Precision Plating Inc.",
                VendorCity     = "Grand Rapids",
                VendorState    = "MI",
                DispatchID     = "SD-001189",
                DispatchDate   = DateTime.Today.AddMonths(-3),
                PartNumber     = partNumber,
                QuantitySent   = 50,
                DispatchStatus = "Closed"
            },
            new Model_OutsideServiceHistory
            {
                VendorID       = "MOCK-VENDOR-001",
                VendorName     = "Acme Heat Treating Co.",
                VendorCity     = "Detroit",
                VendorState    = "MI",
                DispatchID     = "SD-001302",
                DispatchDate   = DateTime.Today.AddDays(-7),
                PartNumber     = partNumber,
                QuantitySent   = 10,
                DispatchStatus = "Open"
            }
        };

        return Model_Dao_Result_Factory.Success(records);
    }

    private Model_Dao_Result<List<Model_OutsideServiceHistory>> CreateMockOutsideServiceHistoryByVendor(string vendorId)
    {
        var records = new List<Model_OutsideServiceHistory>
        {
            new Model_OutsideServiceHistory
            {
                VendorID       = vendorId,
                VendorName     = "Mock Vendor Corp.",
                VendorCity     = "Detroit",
                VendorState    = "MI",
                DispatchID     = "SD-002100",
                DispatchDate   = DateTime.Today.AddMonths(-2),
                PartNumber     = "MOCK-PART-A",
                QuantitySent   = 30,
                DispatchStatus = "Closed"
            },
            new Model_OutsideServiceHistory
            {
                VendorID       = vendorId,
                VendorName     = "Mock Vendor Corp.",
                VendorCity     = "Detroit",
                VendorState    = "MI",
                DispatchID     = "SD-002250",
                DispatchDate   = DateTime.Today.AddDays(-14),
                PartNumber     = "MOCK-PART-B",
                QuantitySent   = 15,
                DispatchStatus = "Open"
            }
        };

        return Model_Dao_Result_Factory.Success(records);
    }

    private Model_Dao_Result<List<Model_FuzzySearchResult>> CreateMockFuzzyParts(string term)
    {
        var results = new List<Model_FuzzySearchResult>
        {
            new Model_FuzzySearchResult { Key = $"21-{term.ToUpper()}-001", Label = $"21-{term.ToUpper()}-001", Detail = "Mock Part — Heat Treated Rod Assembly" },
            new Model_FuzzySearchResult { Key = $"21-{term.ToUpper()}-002", Label = $"21-{term.ToUpper()}-002", Detail = "Mock Part — Plated Bracket" },
            new Model_FuzzySearchResult { Key = $"MMC-{term.ToUpper()}",    Label = $"MMC-{term.ToUpper()}",    Detail = "Mock Part — Machined Component" }
        };

        return Model_Dao_Result_Factory.Success(results);
    }

    private Model_Dao_Result<List<Model_FuzzySearchResult>> CreateMockFuzzyVendors(string term)
    {
        var results = new List<Model_FuzzySearchResult>
        {
            new Model_FuzzySearchResult { Key = "MOCK-V001", Label = $"Acme {term} Co.",          Detail = "Detroit, MI" },
            new Model_FuzzySearchResult { Key = "MOCK-V002", Label = $"Precision {term} Inc.",    Detail = "Grand Rapids, MI" },
            new Model_FuzzySearchResult { Key = "MOCK-V003", Label = $"Allied {term} Solutions",  Detail = "Toledo, OH" }
        };

        return Model_Dao_Result_Factory.Success(results);
    }

    private Model_Dao_Result<List<Model_FuzzySearchResult>> CreateMockPartsByVendor(string _)
    {
        var results = new List<Model_FuzzySearchResult>
        {
            new Model_FuzzySearchResult { Key = "MOCK-PART-A", Label = "MOCK-PART-A", Detail = "3 dispatch(es) — last 01/15/2025" },
            new Model_FuzzySearchResult { Key = "MOCK-PART-B", Label = "MOCK-PART-B", Detail = "1 dispatch(es) — last 03/20/2025" },
            new Model_FuzzySearchResult { Key = "MOCK-PART-C", Label = "MOCK-PART-C", Detail = "5 dispatch(es) — last 04/01/2025" }
        };

        return Model_Dao_Result_Factory.Success(results);
    }

    #endregion

    #region Outside Service Operations

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetOutsideServiceHistoryByPartAsync(string partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>("Part number cannot be empty");
        }

        if (_useMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock outside service history for part: {partNumber}");
            return CreateMockOutsideServiceHistory(partNumber);
        }

        return await _dao.GetOutsideServiceHistoryByPartAsync(partNumber);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetOutsideServiceHistoryByVendorAsync(string vendorId)
    {
        if (string.IsNullOrWhiteSpace(vendorId))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>("Vendor ID cannot be empty");
        }

        if (_useMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock outside service history for vendor: {vendorId}");
            return CreateMockOutsideServiceHistoryByVendor(vendorId);
        }

        return await _dao.GetOutsideServiceHistoryByVendorAsync(vendorId);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchPartsAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>("Search term cannot be empty");
        }

        if (_useMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock fuzzy part results for: {term}");
            return CreateMockFuzzyParts(term);
        }

        return await _dao.FuzzySearchPartsByIdAsync(term);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchVendorsAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>("Search term cannot be empty");
        }

        if (_useMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock fuzzy vendor results for: {term}");
            return CreateMockFuzzyVendors(term);
        }

        return await _dao.FuzzySearchVendorsByNameAsync(term);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> GetPartsByVendorAsync(string vendorId)
    {
        if (string.IsNullOrWhiteSpace(vendorId))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>("Vendor ID cannot be empty");
        }

        if (_useMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock parts for vendor: {vendorId}");
            return CreateMockPartsByVendor(vendorId);
        }

        return await _dao.GetPartsByVendorAsync(vendorId);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_OutsideServiceHistory>>> GetOutsideServiceHistoryByVendorAndPartAsync(
        string vendorId,
        string partNumber)
    {
        if (string.IsNullOrWhiteSpace(vendorId))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>("Vendor ID cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>("Part number cannot be empty");
        }

        if (_useMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock history for vendor {vendorId}, part {partNumber}");
            return CreateMockOutsideServiceHistory(partNumber);
        }

        return await _dao.GetOutsideServiceHistoryByVendorAndPartAsync(vendorId, partNumber);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> FuzzySearchLocationsAsync(string term, string warehouseCode)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>("Search term cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(warehouseCode))
        {
            return Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>("Warehouse code cannot be empty");
        }

        if (_useMockData)
        {
            _logger?.LogInfo($"[MOCK DATA MODE] Returning mock location results for term '{term}' in warehouse '{warehouseCode}'");
            return CreateMockFuzzyLocations(term, warehouseCode);
        }

        return await _dao.FuzzySearchLocationsByWarehouseAsync(term, warehouseCode);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<bool>> PartExistsAsync(string partId)
    {
        if (string.IsNullOrWhiteSpace(partId))
            return Model_Dao_Result_Factory.Failure<bool>("Part ID cannot be empty");

        if (_useMockData)
            return Model_Dao_Result_Factory.Success(true);

        return await _dao.PartExistsAsync(partId);
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<bool>> LocationExistsAsync(string locationId, string warehouseCode)
    {
        if (string.IsNullOrWhiteSpace(locationId))
            return Model_Dao_Result_Factory.Failure<bool>("Location ID cannot be empty");

        if (string.IsNullOrWhiteSpace(warehouseCode))
            return Model_Dao_Result_Factory.Failure<bool>("Warehouse code cannot be empty");

        if (_useMockData)
            return Model_Dao_Result_Factory.Success(true);

        return await _dao.LocationExistsAsync(locationId, warehouseCode);
    }

    private Model_Dao_Result<List<Model_FuzzySearchResult>> CreateMockFuzzyLocations(string term, string warehouseCode)
    {
        var results = new List<Model_FuzzySearchResult>
        {
            new Model_FuzzySearchResult { Key = $"A-{term.ToUpper()}-01", Label = $"A-{term.ToUpper()}-01", Detail = $"Warehouse {warehouseCode} — Aisle A" },
            new Model_FuzzySearchResult { Key = $"B-{term.ToUpper()}-02", Label = $"B-{term.ToUpper()}-02", Detail = $"Warehouse {warehouseCode} — Aisle B" },
            new Model_FuzzySearchResult { Key = $"C-{term.ToUpper()}-03", Label = $"C-{term.ToUpper()}-03", Detail = $"Warehouse {warehouseCode} — Aisle C" }
        };

        return Model_Dao_Result_Factory.Success(results);
    }

    #endregion
}

