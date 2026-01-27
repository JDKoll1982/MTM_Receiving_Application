using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Core.Models.Enums; // For other enums if needed

namespace MTM_Receiving_Application.Module_Dunnage.Contracts;

/// <summary>
/// Service interface for Dunnage Admin UI navigation workflow
/// Manages 4-section navigation: Types, Parts, Specs, Inventoried List
/// </summary>
public interface IService_DunnageAdminWorkflow
{
    /// <summary>
    /// Gets the current admin section being displayed
    /// </summary>
    public Enum_DunnageAdminSection CurrentSection { get; }

    /// <summary>
    /// Raised when admin section changes
    /// </summary>
    public event EventHandler<Enum_DunnageAdminSection>? SectionChanged;

    /// <summary>
    /// Raised when status message needs to be displayed
    /// </summary>
    public event EventHandler<string>? StatusMessageRaised;

    /// <summary>
    /// Navigate to specific admin section
    /// </summary>
    /// <param name="section">Target section (Types, Parts, Specs, InventoriedList)</param>
    public Task NavigateToSectionAsync(Enum_DunnageAdminSection section);

    /// <summary>
    /// Return to main admin hub (4-card navigation view)
    /// </summary>
    public Task NavigateToHubAsync();

    /// <summary>
    /// Validate if navigation away from current section is allowed
    /// (checks for unsaved changes, pending operations)
    /// </summary>
    /// <returns>True if navigation allowed, false if blocked</returns>
    public Task<bool> CanNavigateAwayAsync();

    /// <summary>
    /// Mark current section as having unsaved changes
    /// </summary>
    public void MarkDirty();

    /// <summary>
    /// Clear unsaved changes flag (after save or discard)
    /// </summary>
    public void MarkClean();

    /// <summary>
    /// Check if current section has unsaved changes
    /// </summary>
    public bool IsDirty { get; }
}
