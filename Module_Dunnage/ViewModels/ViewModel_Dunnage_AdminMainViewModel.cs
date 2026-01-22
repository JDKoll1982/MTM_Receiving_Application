using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Admin Main Navigation Hub
/// Provides 4-section navigation: Types, Parts, Specs, Inventoried List
/// </summary>
public partial class ViewModel_Dunnage_AdminMain : ViewModel_Shared_Base
{
    private readonly IService_DunnageAdminWorkflow _adminWorkflow;

    public ViewModel_Dunnage_AdminMain(
        IService_DunnageAdminWorkflow adminWorkflow,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _adminWorkflow = adminWorkflow;

        // Subscribe to workflow events
        _adminWorkflow.SectionChanged += OnSectionChanged;
        _adminWorkflow.StatusMessageRaised += OnStatusMessageRaised;

        // Initialize to Hub view
        UpdateVisibility(Enum_DunnageAdminSection.Hub);
    }

    #region Observable Properties

    [ObservableProperty]
    private bool _isMainNavigationVisible = true;

    [ObservableProperty]
    private bool _isManageTypesVisible = false;

    [ObservableProperty]
    private bool _isManagePartsVisible = false;

    [ObservableProperty]
    private bool _isManageSpecsVisible = false;

    [ObservableProperty]
    private bool _isInventoriedListVisible = false;

    #endregion

    #region Navigation Commands

    /// <summary>
    /// Navigate to Dunnage Types Management
    /// </summary>
    [RelayCommand]
    private async Task NavigateToManageTypesAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Opening Type Management...";

            await _adminWorkflow.NavigateToSectionAsync(Enum_DunnageAdminSection.Types);

            await _logger.LogInfoAsync("Navigated to Type Management", "AdminMain");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error navigating to Type Management",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Navigate to Dunnage Specs Management
    /// </summary>
    [RelayCommand]
    private async Task NavigateToManageSpecsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Opening Spec Management...";

            await _adminWorkflow.NavigateToSectionAsync(Enum_DunnageAdminSection.Specs);

            await _logger.LogInfoAsync("Navigated to Spec Management", "AdminMain");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error navigating to Spec Management",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Navigate to Dunnage Parts Management
    /// </summary>
    [RelayCommand]
    private async Task NavigateToManagePartsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Opening Part Management...";

            await _adminWorkflow.NavigateToSectionAsync(Enum_DunnageAdminSection.Parts);

            await _logger.LogInfoAsync("Navigated to Part Management", "AdminMain");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error navigating to Part Management",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Navigate to Inventoried Dunnage List
    /// </summary>
    [RelayCommand]
    private async Task NavigateToInventoriedListAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Opening Inventoried List...";

            await _adminWorkflow.NavigateToSectionAsync(Enum_DunnageAdminSection.InventoriedList);

            await _logger.LogInfoAsync("Navigated to Inventoried List", "AdminMain");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error navigating to Inventoried List",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Return to main navigation hub (4-card view)
    /// </summary>
    [RelayCommand]
    private async Task ReturnToMainNavigationAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Returning to main menu...";

            await _adminWorkflow.NavigateToHubAsync();

            await _logger.LogInfoAsync("Returned to main navigation hub", "AdminMain");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error returning to main navigation",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Event Handlers

    private void OnSectionChanged(object? sender, Enum_DunnageAdminSection section)
    {
        UpdateVisibility(section);
    }

    private void OnStatusMessageRaised(object? sender, string message)
    {
        StatusMessage = message;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Update visibility flags based on current section
    /// </summary>
    /// <param name="section"></param>
    private void UpdateVisibility(Enum_DunnageAdminSection section)
    {
        IsMainNavigationVisible = section == Enum_DunnageAdminSection.Hub;
        IsManageTypesVisible = section == Enum_DunnageAdminSection.Types;
        IsManagePartsVisible = section == Enum_DunnageAdminSection.Parts;
        IsManageSpecsVisible = section == Enum_DunnageAdminSection.Specs;
        IsInventoriedListVisible = section == Enum_DunnageAdminSection.InventoriedList;
    }

    #endregion
}

