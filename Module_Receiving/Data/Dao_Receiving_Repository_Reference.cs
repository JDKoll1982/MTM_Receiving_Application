using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data Access Object for reference data (PartTypes, PackageTypes, Locations)
/// Manages read-only reference data lookups
/// </summary>
public class Dao_Receiving_Repository_Reference
{
    private readonly string _connectionString;
    private readonly IService_LoggingUtility _logger;

    // Optional: Cache reference data since it changes infrequently
    private List<Model_Receiving_TableEntitys_PartType>? _partTypesCache;
    private List<Model_Receiving_TableEntitys_PackageType>? _packageTypesCache;
    private List<Model_Receiving_TableEntitys_Location>? _locationsCache;
    private DateTime? _cacheExpiration;
    private readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(30);

    public Dao_Receiving_Repository_Reference(
        string connectionString,
        IService_LoggingUtility logger)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(logger);
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all active part types
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_Receiving_TableEntitys_PartType>>> GetPartTypesAsync()
    {
        try
        {
            // Check cache first
            if (_partTypesCache != null && _cacheExpiration.HasValue && DateTime.UtcNow < _cacheExpiration.Value)
            {
                _logger.LogInfo("Returning cached part types");
                return new Model_Dao_Result<List<Model_Receiving_TableEntitys_PartType>>
                {
                    Success = true,
                    Data = _partTypesCache
                };
            }

            _logger.LogInfo("Loading part types from database");

            var result = await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_Receiving_PartType_SelectAll",
                MapPartType,
                null);

            if (result.Success && result.Data != null)
            {
                _partTypesCache = result.Data;
                _cacheExpiration = DateTime.UtcNow.Add(_cacheLifetime);
                _logger.LogInfo($"Loaded {result.Data.Count} part types");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in GetPartTypesAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_PartType>>(
                $"Error loading part types: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves all active package types
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_Receiving_TableEntitys_PackageType>>> GetPackageTypesAsync()
    {
        try
        {
            // Check cache first
            if (_packageTypesCache != null && _cacheExpiration.HasValue && DateTime.UtcNow < _cacheExpiration.Value)
            {
                _logger.LogInfo("Returning cached package types");
                return new Model_Dao_Result<List<Model_Receiving_TableEntitys_PackageType>>
                {
                    Success = true,
                    Data = _packageTypesCache
                };
            }

            _logger.LogInfo("Loading package types from database");

            var result = await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_Receiving_PackageType_SelectAll",
                MapPackageType,
                null);

            if (result.Success && result.Data != null)
            {
                _packageTypesCache = result.Data;
                _cacheExpiration = DateTime.UtcNow.Add(_cacheLifetime);
                _logger.LogInfo($"Loaded {result.Data.Count} package types");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in GetPackageTypesAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_PackageType>>(
                $"Error loading package types: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves all active receiving locations
    /// </summary>
    /// <param name="allowReceivingOnly">Filter to only locations that allow receiving</param>
    public async Task<Model_Dao_Result<List<Model_Receiving_TableEntitys_Location>>> GetLocationsAsync(bool allowReceivingOnly = true)
    {
        try
        {
            // Check cache first (only use cache if allowReceivingOnly filter matches)
            if (_locationsCache != null && _cacheExpiration.HasValue && DateTime.UtcNow < _cacheExpiration.Value)
            {
                _logger.LogInfo("Returning cached locations");
                var filteredCache = allowReceivingOnly
                    ? _locationsCache.FindAll(l => l.AllowReceiving)
                    : _locationsCache;

                return new Model_Dao_Result<List<Model_Receiving_TableEntitys_Location>>
                {
                    Success = true,
                    Data = filteredCache
                };
            }

            _logger.LogInfo("Loading locations from database");

            var result = await Helper_Database_StoredProcedure.ExecuteListAsync(
                _connectionString,
                "sp_Receiving_Location_SelectAll",
                MapLocation,
                null);

            if (result.Success && result.Data != null)
            {
                _locationsCache = result.Data;
                _cacheExpiration = DateTime.UtcNow.Add(_cacheLifetime);

                var filteredData = allowReceivingOnly
                    ? result.Data.FindAll(l => l.AllowReceiving)
                    : result.Data;

                _logger.LogInfo($"Loaded {filteredData.Count} locations");

                return new Model_Dao_Result<List<Model_Receiving_TableEntitys_Location>>
                {
                    Success = true,
                    Data = filteredData,
                    AffectedRows = filteredData.Count,
                    ExecutionTimeMs = result.ExecutionTimeMs
                };
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in GetLocationsAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_Location>>(
                $"Error loading locations: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Retrieves a specific location by its code
    /// </summary>
    /// <param name="locationCode">Location code to lookup</param>
    public async Task<Model_Dao_Result<Model_Receiving_TableEntitys_Location>> GetLocationByCodeAsync(string locationCode)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(locationCode);

            var parameters = new Dictionary<string, object>
            {
                { "p_LocationCode", locationCode }
            };

            _logger.LogInfo($"Loading location by code: {locationCode}");

            var result = await Helper_Database_StoredProcedure.ExecuteSingleAsync(
                _connectionString,
                "sp_Receiving_Location_SelectByCode",
                MapLocation,
                parameters);

            if (result.Success)
            {
                _logger.LogInfo($"Location found: {locationCode}");
            }
            else
            {
                _logger.LogWarning($"Location not found: {locationCode}");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in GetLocationByCodeAsync: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_Receiving_TableEntitys_Location>(
                $"Error loading location: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Clears the reference data cache (call after data changes)
    /// </summary>
    public void ClearCache()
    {
        _partTypesCache = null;
        _packageTypesCache = null;
        _locationsCache = null;
        _cacheExpiration = null;
        _logger.LogInfo("Reference data cache cleared");
    }

    // Mapper methods

    private static Model_Receiving_TableEntitys_PartType MapPartType(IDataReader reader)
    {
        return new Model_Receiving_TableEntitys_PartType
        {
            PartTypeId = reader.GetInt32(reader.GetOrdinal("PartTypeId")),
            PartTypeName = reader.GetString(reader.GetOrdinal("PartTypeName")),
            PartTypeCode = reader.GetString(reader.GetOrdinal("PartTypeCode")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            PartPrefixes = reader.IsDBNull(reader.GetOrdinal("PartPrefixes")) ? null : reader.GetString(reader.GetOrdinal("PartPrefixes")),
            RequiresDiameter = reader.GetBoolean(reader.GetOrdinal("RequiresDiameter")),
            RequiresWidth = reader.GetBoolean(reader.GetOrdinal("RequiresWidth")),
            RequiresLength = reader.GetBoolean(reader.GetOrdinal("RequiresLength")),
            RequiresThickness = reader.GetBoolean(reader.GetOrdinal("RequiresThickness")),
            RequiresWeight = reader.GetBoolean(reader.GetOrdinal("RequiresWeight")),
            SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder")),
            IsSystemDefault = reader.GetBoolean(reader.GetOrdinal("IsSystemDefault"))
        };
    }

    private static Model_Receiving_TableEntitys_PackageType MapPackageType(IDataReader reader)
    {
        return new Model_Receiving_TableEntitys_PackageType
        {
            PackageTypeId = reader.GetInt32(reader.GetOrdinal("PackageTypeId")),
            PackageTypeName = reader.GetString(reader.GetOrdinal("PackageTypeName")),
            PackageTypeCode = reader.GetString(reader.GetOrdinal("PackageTypeCode")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            DefaultPackagesPerLoad = reader.IsDBNull(reader.GetOrdinal("DefaultPackagesPerLoad")) ? null : reader.GetDecimal(reader.GetOrdinal("DefaultPackagesPerLoad")),
            TypicalCapacityLBS = reader.IsDBNull(reader.GetOrdinal("TypicalCapacityLBS")) ? null : reader.GetDecimal(reader.GetOrdinal("TypicalCapacityLBS")),
            SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder")),
            IsSystemDefault = reader.GetBoolean(reader.GetOrdinal("IsSystemDefault"))
        };
    }

    private static Model_Receiving_TableEntitys_Location MapLocation(IDataReader reader)
    {
        return new Model_Receiving_TableEntitys_Location
        {
            LocationId = reader.GetInt32(reader.GetOrdinal("LocationId")),
            LocationCode = reader.GetString(reader.GetOrdinal("LocationCode")),
            LocationName = reader.GetString(reader.GetOrdinal("LocationName")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            AllowReceiving = reader.GetBoolean(reader.GetOrdinal("AllowReceiving")),
            IsQualityHoldArea = reader.GetBoolean(reader.GetOrdinal("IsQualityHoldArea")),
            SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder")),
            IsSystemDefault = reader.GetBoolean(reader.GetOrdinal("IsSystemDefault"))
        };
    }
}
