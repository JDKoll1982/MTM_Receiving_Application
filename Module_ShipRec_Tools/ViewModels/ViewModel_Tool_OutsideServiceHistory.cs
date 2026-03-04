using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the Outside Service Provider History lookup tool.
/// Searches Infor Visual (READ-ONLY) for all dispatch records associated with a given part number.
/// </summary>
public partial class ViewModel_Tool_OutsideServiceHistory : ViewModel_Shared_Base
{
    private readonly IService_Tool_OutsideServiceHistory _service;

    [ObservableProperty]
    private string _partNumber = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Model_OutsideServiceHistory> _results = new();

    public ViewModel_Tool_OutsideServiceHistory(
        IService_Tool_OutsideServiceHistory service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService)
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(PartNumber))
        {
            await _errorHandler.ShowUserErrorAsync(
                "Please enter a part number to search.",
                "Input Required",
                nameof(SearchAsync));
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus($"Searching for part {PartNumber}...");

            var result = await _service.GetHistoryByPartAsync(PartNumber);

            if (result.IsSuccess && result.Data != null)
            {
                Results.Clear();
                foreach (var item in result.Data)
                {
                    Results.Add(item);
                }

                var message = Results.Count > 0
                    ? $"Found {Results.Count} record(s) for part {PartNumber}."
                    : $"No outside service history found for part {PartNumber}.";

                ShowStatus(message);
                _logger.LogInfo($"Outside service history: {Results.Count} records for part {PartNumber}");
            }
            else
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                _logger.LogWarning($"Outside service history query failed for part {PartNumber}: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SearchAsync),
                nameof(ViewModel_Tool_OutsideServiceHistory));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        PartNumber = string.Empty;
        Results.Clear();
        ShowStatus("Enter a part number to search.");
    }
}
