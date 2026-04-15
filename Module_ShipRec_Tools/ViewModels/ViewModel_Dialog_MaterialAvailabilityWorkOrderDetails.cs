using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.Settings;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the Material Availability work-order details dialog.
/// </summary>
public sealed partial class ViewModel_Dialog_MaterialAvailabilityWorkOrderDetails
    : ViewModel_Shared_Base
{
    private readonly IService_Tool_MaterialAvailabilityBoard _service;
    private readonly Model_Tool_MaterialAvailabilityAssociatedPartRun _associatedRun;
    private readonly Model_Tool_MaterialAvailabilityFieldSettings _fieldSettings;

    [ObservableProperty]
    private bool _isShowingAllFields;

    public Func<
        Model_FormattedReportDocument,
        Task<Model_Dao_Result<bool>>
    >? RequestPrintAsync { get; set; }

    public ObservableCollection<Model_Tool_MaterialAvailabilityDetailSection> Sections { get; } =
    [];

    public string Heading => $"Work Order Details - {_associatedRun.WorkOrderDisplay}";

    public string Subheading =>
        $"{_associatedRun.AssociatedPartNumber} - {_associatedRun.AssociatedPartDescription}";

    public string SummaryText =>
        $"Required Parts {_associatedRun.RequiredPartsQuantityDisplay} {_associatedRun.NormalizedUsageUnitOfMeasure} | Estimated Coil Use {_associatedRun.EstimatedCoilUseDisplay} {_associatedRun.NormalizedUsageUnitOfMeasure}";

    public bool IsShowAllChipVisible => _fieldSettings.IsShowAllChipEnabled;

    public string ShowAllButtonText => IsShowingAllFields ? "Hide logic-only fields" : "Show all";

    public ViewModel_Dialog_MaterialAvailabilityWorkOrderDetails(
        Model_Tool_MaterialAvailabilityAssociatedPartRun associatedRun,
        Model_Tool_MaterialAvailabilityFieldSettings fieldSettings,
        IService_Tool_MaterialAvailabilityBoard service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _associatedRun = associatedRun ?? throw new ArgumentNullException(nameof(associatedRun));
        _fieldSettings = fieldSettings ?? throw new ArgumentNullException(nameof(fieldSettings));
        _service = service ?? throw new ArgumentNullException(nameof(service));
        RebuildSections();
    }

    partial void OnIsShowingAllFieldsChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowAllButtonText));
        RebuildSections();
    }

    [RelayCommand]
    private void ToggleShowAllFields()
    {
        if (!_fieldSettings.IsShowAllChipEnabled)
        {
            return;
        }

        IsShowingAllFields = !IsShowingAllFields;
    }

    [RelayCommand]
    private async Task PrintAsync()
    {
        if (RequestPrintAsync is null)
        {
            return;
        }

        var printSections = MaterialAvailabilityWorkOrderDetailBuilder.BuildPrintSections(
            _associatedRun,
            _fieldSettings
        );
        var documentResult = await _service.FormatWorkOrderDetailsForPrintAsync(
            _associatedRun,
            printSections
        );
        if (!documentResult.IsSuccess || documentResult.Data is null)
        {
            await _errorHandler.ShowUserErrorAsync(
                documentResult.ErrorMessage,
                "Work Order Print",
                nameof(PrintAsync)
            );
            return;
        }

        var printResult = await RequestPrintAsync(documentResult.Data);
        if (!printResult.IsSuccess || !printResult.Data)
        {
            await _errorHandler.ShowUserErrorAsync(
                printResult.ErrorMessage,
                "Work Order Print",
                nameof(PrintAsync)
            );
        }
    }

    private void RebuildSections()
    {
        Sections.Clear();
        foreach (
            var section in MaterialAvailabilityWorkOrderDetailBuilder.BuildUiSections(
                _associatedRun,
                _fieldSettings,
                IsShowingAllFields
            )
        )
        {
            Sections.Add(section);
        }
    }
}
