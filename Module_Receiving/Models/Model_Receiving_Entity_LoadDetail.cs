using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Represents data for a single load in a receiving transaction.
/// Supports bulk copy operations with auto-fill tracking to preserve user-entered data.
/// </summary>
public class Model_Receiving_Entity_LoadDetail
{
    // ===== Identity =====
    
    /// <summary>
    /// Load number within the session (1-based sequence).
    /// </summary>
    public int LoadNumber { get; init; }
    
    // ===== Data Fields =====
    
    /// <summary>
    /// Weight or quantity for this load.
    /// </summary>
    public decimal? WeightOrQuantity { get; set; }
    
    /// <summary>
    /// Heat lot number for traceability.
    /// </summary>
    public string? HeatLot { get; set; }
    
    /// <summary>
    /// Package type identifier (e.g., "Pallet", "Box", "Crate").
    /// </summary>
    public string? PackageType { get; set; }
    
    /// <summary>
    /// Number of packages in this load.
    /// </summary>
    public int? PackagesPerLoad { get; set; }
    
    // ===== Auto-Fill Metadata =====
    
    /// <summary>
    /// True if WeightOrQuantity was set by a copy operation (not user-entered).
    /// Used to preserve user data during subsequent copy operations.
    /// </summary>
    public bool IsWeightAutoFilled { get; set; }
    
    /// <summary>
    /// True if HeatLot was set by a copy operation (not user-entered).
    /// </summary>
    public bool IsHeatLotAutoFilled { get; set; }
    
    /// <summary>
    /// True if PackageType was set by a copy operation (not user-entered).
    /// </summary>
    public bool IsPackageTypeAutoFilled { get; set; }
    
    /// <summary>
    /// True if PackagesPerLoad was set by a copy operation (not user-entered).
    /// </summary>
    public bool IsPackagesPerLoadAutoFilled { get; set; }
    
    // ===== Validation =====
    
    /// <summary>
    /// Collection of validation error messages for this load.
    /// </summary>
    public List<string> ValidationErrors { get; init; } = new();
    
    /// <summary>
    /// True if this load has no validation errors.
    /// </summary>
    public bool IsValid => ValidationErrors.Count == 0;
}
