using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Receiving.Models; // For Model_WorkflowStepResult if needed
using MTM_Receiving_Application.Models.Enums; // For other enums if needed
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

