using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.DunnageModule.Models;
using MTM_Receiving_Application.DunnageModule.Enums;
using MTM_Receiving_Application.DunnageModule.Data;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;
namespace MTM_Receiving_Application.DunnageModule.Services;

/// <summary>
/// Service implementation for Dunnage Admin UI navigation workflow
/// Manages 4-section navigation: Types, Parts, Specs, Inventoried List
/// </summary>
public class Service_DunnageAdminWorkflow : IService_DunnageAdminWorkflow
{
    private Enum_DunnageAdminSection _currentSection = Enum_DunnageAdminSection.Hub;
    private bool _isDirty;

    /// <summary>
    /// Gets the current admin section being displayed
    /// </summary>
    public Enum_DunnageAdminSection CurrentSection
    {
        get => _currentSection;
        private set
        {
            if (_currentSection != value)
            {
                _currentSection = value;
                SectionChanged?.Invoke(this, _currentSection);
            }
        }
    }

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
    public async Task NavigateToSectionAsync(Enum_DunnageAdminSection section)
    {
        // Check if navigation is allowed
        if (!await CanNavigateAwayAsync())
        {
            StatusMessageRaised?.Invoke(this, "Cannot navigate - unsaved changes exist. Please save or discard changes first.");
            return;
        }

        // Navigate to section
        CurrentSection = section;
        MarkClean();

        // Raise status message
        string sectionName = section switch
        {
            Enum_DunnageAdminSection.Hub => "Admin Hub",
            Enum_DunnageAdminSection.Types => "Dunnage Types Management",
            Enum_DunnageAdminSection.Parts => "Part Master Management",
            Enum_DunnageAdminSection.Specs => "Spec Definitions Management",
            Enum_DunnageAdminSection.InventoriedList => "Inventoried Dunnage List",
            _ => "Unknown Section"
        };

        StatusMessageRaised?.Invoke(this, $"Navigated to {sectionName}");
    }

    /// <summary>
    /// Return to main admin hub (4-card navigation view)
    /// </summary>
    public async Task NavigateToHubAsync()
    {
        await NavigateToSectionAsync(Enum_DunnageAdminSection.Hub);
    }

    /// <summary>
    /// Validate if navigation away from current section is allowed
    /// (checks for unsaved changes, pending operations)
    /// </summary>
    /// <returns>True if navigation allowed, false if blocked</returns>
    public Task<bool> CanNavigateAwayAsync()
    {
        // If no unsaved changes, allow navigation
        if (!_isDirty)
        {
            return Task.FromResult(true);
        }

        // If unsaved changes exist, block navigation
        // The UI should prompt the user to save or discard changes
        return Task.FromResult(false);
    }

    /// <summary>
    /// Mark current section as having unsaved changes
    /// </summary>
    public void MarkDirty()
    {
        _isDirty = true;
    }

    /// <summary>
    /// Clear unsaved changes flag (after save or discard)
    /// </summary>
    public void MarkClean()
    {
        _isDirty = false;
    }

    /// <summary>
    /// Check if current section has unsaved changes
    /// </summary>
    public bool IsDirty => _isDirty;
}

