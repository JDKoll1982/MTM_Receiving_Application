using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.Contracts.Services;

/// <summary>
/// Service interface for Dunnage Admin UI navigation workflow
/// Manages 4-section navigation: Types, Parts, Specs, Inventoried List
/// </summary>
public interface IService_DunnageAdminWorkflow
{
    /// <summary>
    /// Gets the current admin section being displayed
    /// </summary>
    Enum_DunnageAdminSection CurrentSection { get; }
    
    /// <summary>
    /// Raised when admin section changes
    /// </summary>
    event EventHandler<Enum_DunnageAdminSection>? SectionChanged;
    
    /// <summary>
    /// Raised when status message needs to be displayed
    /// </summary>
    event EventHandler<string>? StatusMessageRaised;
    
    /// <summary>
    /// Navigate to specific admin section
    /// </summary>
    /// <param name="section">Target section (Types, Parts, Specs, InventoriedList)</param>
    Task NavigateToSectionAsync(Enum_DunnageAdminSection section);
    
    /// <summary>
    /// Return to main admin hub (4-card navigation view)
    /// </summary>
    Task NavigateToHubAsync();
    
    /// <summary>
    /// Validate if navigation away from current section is allowed
    /// (checks for unsaved changes, pending operations)
    /// </summary>
    /// <returns>True if navigation allowed, false if blocked</returns>
    Task<bool> CanNavigateAwayAsync();
    
    /// <summary>
    /// Mark current section as having unsaved changes
    /// </summary>
    void MarkDirty();
    
    /// <summary>
    /// Clear unsaved changes flag (after save or discard)
    /// </summary>
    void MarkClean();
    
    /// <summary>
    /// Check if current section has unsaved changes
    /// </summary>
    bool IsDirty { get; }
}

/// <summary>
/// Enum for Dunnage Admin UI sections
/// </summary>
public enum Enum_DunnageAdminSection
{
    /// <summary>
    /// Main navigation hub (4-card view)
    /// </summary>
    Hub = 0,
    
    /// <summary>
    /// Dunnage Types management (CRUD on dunnage_types table)
    /// </summary>
    Types = 1,
    
    /// <summary>
    /// Part Master management (CRUD on dunnage_part_numbers table)
    /// </summary>
    Parts = 2,
    
    /// <summary>
    /// Spec Definitions management (CRUD on dunnage_specs table)
    /// </summary>
    Specs = 3,
    
    /// <summary>
    /// Inventoried Dunnage List (CRUD on inventoried_dunnage_list table)
    /// </summary>
    InventoriedList = 4
}
