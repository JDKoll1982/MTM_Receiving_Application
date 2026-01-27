using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Models.DataTransferObjects;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step2;

/// <summary>
/// Manages the copy preview dialog for bulk copy operations.
/// </summary>
public partial class ViewModel_Receiving_Wizard_Dialog_CopyPreviewDialog : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<Model_Receiving_DataTransferObjects_CopyPreview> _previewItems = new();

    [ObservableProperty]
    private int _totalAffectedLoads = 0;

    [ObservableProperty]
    private bool _userConfirmed = false;

    [ObservableProperty]
    private int _sourceLoadNumber = 1;

    [ObservableProperty]
    private ObservableCollection<string> _fieldNames = new();

    [ObservableProperty]
    private string _dialogTitle = "Copy Preview";

    [ObservableProperty]
    private string _dialogMessage = string.Empty;

    #endregion

    #region Constructor

    public ViewModel_Receiving_Wizard_Dialog_CopyPreviewDialog(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator;
        _logger.LogInfo("Copy Preview Dialog ViewModel initialized");
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void ConfirmCopy()
    {
        UserConfirmed = true;
        _logger.LogInfo($"Copy confirmed: {TotalAffectedLoads} loads will be affected");
        StatusMessage = "Copy confirmed";
    }

    [RelayCommand]
    private void CancelCopy()
    {
        UserConfirmed = false;
        _logger.LogInfo("Copy cancelled by user");
        StatusMessage = "Copy cancelled";
    }

    #endregion

    #region Public Methods

    public void InitializePreview(
        ObservableCollection<Model_Receiving_DataTransferObjects_CopyPreview> previewItems,
        int sourceLoadNumber,
        ObservableCollection<string> fieldNames)
    {
        PreviewItems = previewItems ?? new ObservableCollection<Model_Receiving_DataTransferObjects_CopyPreview>();
        SourceLoadNumber = sourceLoadNumber;
        FieldNames = fieldNames ?? new ObservableCollection<string>();
        TotalAffectedLoads = PreviewItems.Count;

        DialogMessage = $"The following fields from Load {SourceLoadNumber} will be copied to {TotalAffectedLoads} empty cells:";

        _logger.LogInfo($"Preview initialized: Source={SourceLoadNumber}, Affected={TotalAffectedLoads}, Fields={FieldNames.Count}");
    }

    public static ObservableCollection<Model_Receiving_DataTransferObjects_CopyPreview> GeneratePreview(
        IEnumerable<Model_Receiving_DataTransferObjects_LoadGridRow> loads,
        int sourceLoadNumber,
        IEnumerable<string> fieldNames)
    {
        var preview = new ObservableCollection<Model_Receiving_DataTransferObjects_CopyPreview>();

        var sourceLoad = loads.FirstOrDefault(l => l.LoadNumber == sourceLoadNumber);
        if (sourceLoad == null) return preview;

        foreach (var load in loads.Where(l => l.LoadNumber != sourceLoadNumber))
        {
            foreach (var fieldName in fieldNames)
            {
                var (currentValue, newValue, shouldCopy) = GetFieldValues(load, sourceLoad, fieldName);

                if (shouldCopy)
                {
                    preview.Add(new Model_Receiving_DataTransferObjects_CopyPreview
                    {
                        LoadNumber = load.LoadNumber,
                        FieldName = fieldName,
                        CurrentValue = currentValue,
                        NewValue = newValue
                    });
                }
            }
        }

        return preview;
    }

    #endregion

    #region Helper Methods

    private static (string currentValue, string newValue, bool shouldCopy) GetFieldValues(
        Model_Receiving_DataTransferObjects_LoadGridRow targetLoad,
        Model_Receiving_DataTransferObjects_LoadGridRow sourceLoad,
        string fieldName)
    {
        return fieldName.ToUpperInvariant() switch
        {
            "HEAT/LOT" or "HEATLOT" => (
                targetLoad.HeatLot ?? "(empty)",
                sourceLoad.HeatLot ?? "(empty)",
                string.IsNullOrWhiteSpace(targetLoad.HeatLot) && !string.IsNullOrWhiteSpace(sourceLoad.HeatLot)
            ),
            "PACKAGETYPE" or "PACKAGE TYPE" => (
                targetLoad.PackageType ?? "(empty)",
                sourceLoad.PackageType ?? "(empty)",
                string.IsNullOrWhiteSpace(targetLoad.PackageType) && !string.IsNullOrWhiteSpace(sourceLoad.PackageType)
            ),
            "PACKAGESPERLOAD" or "PACKAGES PER LOAD" => (
                targetLoad.PackagesPerLoad?.ToString() ?? "(empty)",
                sourceLoad.PackagesPerLoad?.ToString() ?? "(empty)",
                !targetLoad.PackagesPerLoad.HasValue && sourceLoad.PackagesPerLoad.HasValue
            ),
            "RECEIVINGLOCATION" or "RECEIVING LOCATION" => (
                targetLoad.ReceivingLocation ?? "(empty)",
                sourceLoad.ReceivingLocation ?? "(empty)",
                string.IsNullOrWhiteSpace(targetLoad.ReceivingLocation) && !string.IsNullOrWhiteSpace(sourceLoad.ReceivingLocation)
            ),
            _ => (string.Empty, string.Empty, false)
        };
    }

    #endregion
}
