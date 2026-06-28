using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

/// <summary>
/// ViewModel for managing global vendor-specific variable name mappings.
/// Allows users to select vendors via fuzzy search and assign custom variable names.
/// </summary>
public sealed partial class ViewModel_Settings_Receiving_VendorVariables : ViewModel_Shared_Base
{
    private readonly IService_MySQL_ReceivingVendorVariable _vendorVariableService;
    private readonly IService_InforVisual _inforVisualService;
    private readonly IService_Notification _notificationService;

    [ObservableProperty]
    private string _vendorName = string.Empty;

    [ObservableProperty]
    private string _variableName = string.Empty;

    [ObservableProperty]
    private bool _isEditingExisting;

    [ObservableProperty]
    private ObservableCollection<Model_ReceivingVendorVariableMapping> _mappings = [];

    [ObservableProperty]
    private Model_ReceivingVendorVariableMapping? _selectedMapping;

    public ViewModel_Settings_Receiving_VendorVariables(
        IService_MySQL_ReceivingVendorVariable vendorVariableService,
        IService_InforVisual inforVisualService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(vendorVariableService);
        ArgumentNullException.ThrowIfNull(inforVisualService);
        ArgumentNullException.ThrowIfNull(notificationService);
        _vendorVariableService = vendorVariableService;
        _inforVisualService = inforVisualService;
        _notificationService = notificationService;
        Title = "Custom Vendor Variables";
        _ = LoadMappingsAsync();
    }

    /// <summary>
    /// Loads all vendor variable mappings from the database.
    /// </summary>
    [RelayCommand]
    private async Task LoadMappingsAsync()
    {
        try
        {
            IsBusy = true;
            var result = await _vendorVariableService.GetAllMappingsAsync();
            if (result.IsSuccess && result.Data is not null)
            {
                Mappings = new ObservableCollection<Model_ReceivingVendorVariableMapping>(
                    result.Data.OrderBy(m => m.VendorName)
                );
            }
            else
            {
                await _errorHandler.HandleDaoErrorAsync(result, "Load vendor variable mappings");
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to load vendor variable mappings",
                Enum_ErrorSeverity.Error,
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Loads a vendor variable mapping by vendor name (used after fuzzy vendor selection).
    /// </summary>
    /// <param name="vendorName"></param>
    public async Task LoadMappingForVendorAsync(string vendorName)
    {
        ArgumentNullException.ThrowIfNull(vendorName);

        try
        {
            IsBusy = true;
            var result = await _vendorVariableService.GetMappingByVendorAsync(vendorName);
            if (result.IsSuccess)
            {
                VendorName = vendorName;
                if (result.Data is not null)
                {
                    VariableName = result.Data.VariableName;
                    IsEditingExisting = true;
                }
                else
                {
                    VariableName = string.Empty;
                    IsEditingExisting = false;
                }
            }
            else
            {
                await _errorHandler.HandleDaoErrorAsync(result, "Load vendor variable mapping");
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                $"Failed to load mapping for vendor: {vendorName}",
                Enum_ErrorSeverity.Error,
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Saves the current vendor variable mapping.
    /// Shows a warning if overwriting an existing mapping.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(VendorName))
        {
            ShowStatus("Please select a vendor first");
            return;
        }

        if (string.IsNullOrWhiteSpace(VariableName))
        {
            ShowStatus("Please enter a variable name");
            return;
        }

        try
        {
            IsBusy = true;

            if (IsEditingExisting)
            {
                ShowStatus($"Updating variable name for vendor '{VendorName}'");
            }

            var result = await _vendorVariableService.SaveMappingAsync(
                VendorName,
                VariableName
            );

            if (result.IsSuccess)
            {
                ShowStatus(
                    IsEditingExisting
                        ? $"Updated: {VendorName} → {VariableName}"
                        : $"Saved: {VendorName} → {VariableName}"
                );
                await LoadMappingsAsync();
                ClearForm();
            }
            else
            {
                await _errorHandler.HandleDaoErrorAsync(result, "Save vendor variable mapping");
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to save vendor variable mapping",
                Enum_ErrorSeverity.Error,
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Deletes the selected vendor variable mapping.
    /// </summary>
    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedMapping is null)
        {
            ShowStatus("Please select a mapping to delete");
            return;
        }

        var selectedVendorName = SelectedMapping.VendorName;

        try
        {
            IsBusy = true;
            var result = await _vendorVariableService.DeleteMappingAsync(selectedVendorName);

            if (result.IsSuccess)
            {
                ShowStatus($"Deleted mapping for {selectedVendorName}");

                if (VendorName == selectedVendorName)
                {
                    ClearForm();
                }

                await LoadMappingsAsync();
            }
            else
            {
                await _errorHandler.HandleDaoErrorAsync(result, "Delete vendor variable mapping");
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to delete vendor variable mapping",
                Enum_ErrorSeverity.Error,
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Clears the vendor and variable name form fields.
    /// </summary>
    [RelayCommand]
    private void ClearForm()
    {
        VendorName = string.Empty;
        VariableName = string.Empty;
        IsEditingExisting = false;
    }

    /// <summary>
    /// Loads the selected mapping into the form for editing.
    /// </summary>
    partial void OnSelectedMappingChanged(Model_ReceivingVendorVariableMapping? value)
    {
        if (value is not null)
        {
            VendorName = value.VendorName;
            VariableName = value.VariableName;
            IsEditingExisting = true;
        }
    }

    /// <summary>
    /// Performs fuzzy vendor search via Infor Visual.
    /// Returns list of matching vendors for the picker dialog.
    /// </summary>
    /// <param name="searchTerm"></param>
    public async Task<IReadOnlyList<Module_Core.Models.InforVisual.Model_FuzzySearchResult>> SearchVendorsAsync(
        string searchTerm
    )
    {
        ArgumentNullException.ThrowIfNull(searchTerm);

        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return [];
            }

            var result = await _inforVisualService.FuzzySearchVendorsAsync(searchTerm);
            return result.IsSuccess && result.Data is not null ? result.Data : [];
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to search vendors: {ex.Message}");
            await _errorHandler.HandleErrorAsync(
                "Failed to search vendors",
                Enum_ErrorSeverity.Error,
                ex
            );
            return [];
        }
    }
}
