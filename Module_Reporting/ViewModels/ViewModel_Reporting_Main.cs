using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Reporting.ViewModels;

/// <summary>
/// Main ViewModel for Reporting module
/// Handles date range selection, module availability checking, and report generation
/// </summary>
public partial class ViewModel_Reporting_Main : ViewModel_Shared_Base
{
    private readonly IService_Reporting _reportingService;

    #region Observable Properties

    [ObservableProperty]
    private DateTimeOffset _startDate = DateTimeOffset.Now.AddDays(-7);

    [ObservableProperty]
    private DateTimeOffset _endDate = DateTimeOffset.Now;

    [ObservableProperty]
    private bool _isReceivingChecked;

    [ObservableProperty]
    private bool _isDunnageChecked;

    [ObservableProperty]
    private bool _isVolvoChecked;

    [ObservableProperty]
    private bool _isReceivingEnabled = true;

    [ObservableProperty]
    private bool _isDunnageEnabled = true;

    [ObservableProperty]
    private bool _isVolvoEnabled = true;

    [ObservableProperty]
    private int _receivingCount;

    [ObservableProperty]
    private int _dunnageCount;

    [ObservableProperty]
    private int _volvoCount;

    [ObservableProperty]
    private ObservableCollection<Model_ReportRow> _reportData = new();

    [ObservableProperty]
    private string _currentModuleName = string.Empty;

    #endregion

    public ViewModel_Reporting_Main(
        IService_Reporting reportingService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        Title = "End of Day Reports";
    }

    /// <summary>
    /// Checks data availability for each module in the selected date range
    /// </summary>
    [RelayCommand]
    private async Task CheckAvailabilityAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus("Checking data availability...", InfoBarSeverity.Informational);

            var result = await _reportingService.CheckAvailabilityAsync(
                StartDate.DateTime,
                EndDate.DateTime);

            if (result.IsSuccess && result.Data != null)
            {
                // Update counts
                ReceivingCount = result.Data.GetValueOrDefault("Receiving", 0);
                DunnageCount = result.Data.GetValueOrDefault("Dunnage", 0);
                VolvoCount = result.Data.GetValueOrDefault("Volvo", 0);

                // Enable/disable checkboxes based on availability
                IsReceivingEnabled = ReceivingCount > 0;
                IsDunnageEnabled = DunnageCount > 0;
                IsVolvoEnabled = VolvoCount > 0;

                var totalCount = ReceivingCount + DunnageCount + VolvoCount;
                ShowStatus($"Found {totalCount} total records", InfoBarSeverity.Success);
            }
            else
            {
                ShowStatus(result.ErrorMessage ?? "Failed to check availability", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking availability: {ex.Message}", ex);
            ShowStatus("Error checking data availability", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Generates report for Receiving module
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGenerateReceiving))]
    private async Task GenerateReceivingReportAsync()
    {
        await GenerateReportForModuleAsync("Receiving",
            () => _reportingService.GetReceivingHistoryAsync(StartDate.DateTime, EndDate.DateTime));
    }

    private bool CanGenerateReceiving() => IsReceivingChecked && IsReceivingEnabled && !IsBusy;

    /// <summary>
    /// Generates report for Dunnage module
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGenerateDunnage))]
    private async Task GenerateDunnageReportAsync()
    {
        await GenerateReportForModuleAsync("Dunnage",
            () => _reportingService.GetDunnageHistoryAsync(StartDate.DateTime, EndDate.DateTime));
    }

    private bool CanGenerateDunnage() => IsDunnageChecked && IsDunnageEnabled && !IsBusy;

    /// <summary>
    /// Generates report for Volvo module
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGenerateVolvo))]
    private async Task GenerateVolvoReportAsync()
    {
        await GenerateReportForModuleAsync("Volvo",
            () => _reportingService.GetVolvoHistoryAsync(StartDate.DateTime, EndDate.DateTime));
    }

    private bool CanGenerateVolvo() => IsVolvoChecked && IsVolvoEnabled && !IsBusy;

    /// <summary>
    /// Exports current report data to CSV
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportToCSVAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus("Exporting to CSV...", InfoBarSeverity.Informational);

            var result = await _reportingService.ExportToCSVAsync(
                ReportData.ToList(),
                CurrentModuleName);

            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
            {
                ShowStatus($"CSV exported to: {result.Data}", InfoBarSeverity.Success);
                _logger.LogInfo($"CSV exported: {result.Data}");
            }
            else
            {
                ShowStatus(result.ErrorMessage ?? "Export failed", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error exporting CSV: {ex.Message}", ex);
            ShowStatus("Error exporting CSV", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExport() => ReportData.Count > 0 && !string.IsNullOrEmpty(CurrentModuleName) && !IsBusy;

    /// <summary>
    /// Formats current report data for email and copies to clipboard
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCopyEmail))]
    private async Task CopyEmailFormatAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus("Formatting for email...", InfoBarSeverity.Informational);

            var result = await _reportingService.FormatForEmailAsync(
                ReportData.ToList(),
                applyDateGrouping: true);

            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
            {
                // Copy to clipboard
                var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                dataPackage.SetHtmlFormat(result.Data);
                dataPackage.SetText(result.Data);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);

                ShowStatus("Email format copied to clipboard", InfoBarSeverity.Success);
                _logger.LogInfo("Email format copied to clipboard");
            }
            else
            {
                ShowStatus(result.ErrorMessage ?? "Formatting failed", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error copying email format: {ex.Message}", ex);
            ShowStatus("Error formatting for email", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanCopyEmail() => ReportData.Count > 0 && !IsBusy;

    /// <summary>
    /// Helper method to generate report for a specific module
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="fetchDataFunc"></param>
    private async Task GenerateReportForModuleAsync(
        string moduleName,
        Func<Task<Module_Core.Models.Core.Model_Dao_Result<List<Model_ReportRow>>>> fetchDataFunc)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus($"Generating {moduleName} report...", InfoBarSeverity.Informational);

            var result = await fetchDataFunc();

            if (result.IsSuccess && result.Data != null)
            {
                ReportData.Clear();
                foreach (var row in result.Data)
                {
                    ReportData.Add(row);
                }

                CurrentModuleName = moduleName;
                ShowStatus($"Loaded {ReportData.Count} {moduleName} records", InfoBarSeverity.Success);
                _logger.LogInfo($"Generated {moduleName} report: {ReportData.Count} records");

                // Update command availability
                ExportToCSVCommand.NotifyCanExecuteChanged();
                CopyEmailFormatCommand.NotifyCanExecuteChanged();
            }
            else
            {
                ShowStatus(result.ErrorMessage ?? $"Failed to load {moduleName} data", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating {moduleName} report: {ex.Message}", ex);
            ShowStatus($"Error generating {moduleName} report", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Update command availability when checkbox states change
    /// </summary>
    partial void OnIsReceivingCheckedChanged(bool value)
    {
        GenerateReceivingReportCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsDunnageCheckedChanged(bool value)
    {
        GenerateDunnageReportCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsVolvoCheckedChanged(bool value)
    {
        GenerateVolvoReportCommand.NotifyCanExecuteChanged();
    }
}
