using System;
using System.Collections.Generic;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Represents a single receiving transaction workflow session (3-step guided workflow).
/// Tracks state across Order & Part Selection → Load Details Entry → Review & Save.
/// </summary>
public class Model_Receiving_Entity_WorkflowSession
{
    // ===== Identity =====
    
    /// <summary>
    /// Unique identifier for this workflow session (GUID).
    /// </summary>
    public Guid SessionId { get; init; }
    
    /// <summary>
    /// Timestamp when the session was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Timestamp of last modification to the session.
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }
    
    // ===== Workflow State =====
    
    /// <summary>
    /// Current step in the workflow (1 = Order & Part, 2 = Load Details, 3 = Review & Save).
    /// </summary>
    public int CurrentStep { get; set; }
    
    /// <summary>
    /// True if user navigated back from review step to edit data.
    /// </summary>
    public bool IsEditMode { get; set; }
    
    /// <summary>
    /// True if there are unsaved changes in the workflow.
    /// </summary>
    public bool HasUnsavedChanges { get; set; }
    
    // ===== Step 1 Data (Order & Part Selection) =====
    
    /// <summary>
    /// Purchase order number (null for non-PO mode).
    /// </summary>
    public string? PONumber { get; set; }
    
    /// <summary>
    /// Part ID from Infor Visual database.
    /// </summary>
    public int? PartId { get; set; }
    
    /// <summary>
    /// Part number for display purposes.
    /// </summary>
    public string? PartNumber { get; set; }
    
    /// <summary>
    /// Part description for display purposes.
    /// </summary>
    public string? PartDescription { get; set; }
    
    /// <summary>
    /// Number of loads in this receiving transaction (1-100).
    /// </summary>
    public int LoadCount { get; set; }
    
    // ===== Step 2 Data Collection =====
    
    /// <summary>
    /// Collection of individual load details (initialized based on LoadCount).
    /// </summary>
    public List<Model_Receiving_Entity_LoadDetail> Loads { get; init; } = new();
    
    // ===== Copy Operation State =====
    
    /// <summary>
    /// Load number currently selected as the copy source (default: 1).
    /// </summary>
    public int CopySourceLoadNumber { get; set; } = 1;
    
    /// <summary>
    /// Timestamp of the last bulk copy operation.
    /// </summary>
    public DateTime? LastCopyOperationAt { get; set; }
    
    // ===== Validation State =====
    
    /// <summary>
    /// Collection of validation errors for this session.
    /// </summary>
    public List<Model_Receiving_DTO_ValidationError> ValidationErrors { get; init; } = new();
    
    // ===== Save State =====
    
    /// <summary>
    /// True when the workflow has been completed and saved.
    /// </summary>
    public bool IsSaved { get; set; }
    
    /// <summary>
    /// Timestamp when the workflow was saved.
    /// </summary>
    public DateTime? SavedAt { get; set; }
    
    /// <summary>
    /// File path to the saved CSV export.
    /// </summary>
    public string? SavedCsvPath { get; set; }
}
