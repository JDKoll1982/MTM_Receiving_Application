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
using Windows.ApplicationModel.DataTransfer;

namespace MTM_Receiving_Application.Module_Reporting.ViewModels;

public partial class ViewModel_Reporting_Main : ViewModel_Shared_Base
{
    private readonly IService_Reporting _reportingService;
    private readonly IService_ReportingClipboard _reportingClipboard;

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
        IService_ReportingClipboard reportingClipboard,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        _reportingClipboard = reportingClipboard ?? throw new ArgumentNullException(nameof(reportingClipboard));
        Title = "End of Day Reports";
        ResetAvailabilityState();
    }

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
            NotifyGenerateCommands();
            CopyEmailFormatCommand.NotifyCanExecuteChanged();
            ShowStatus("Checking data availability...", InfoBarSeverity.Informational);

            var result = await _reportingService.CheckAvailabilityAsync(
                StartDate.DateTime,
                EndDate.DateTime);

            if (result.IsSuccess && result.Data != null)
            {
                ReceivingCount = result.Data.GetValueOrDefault("Receiving", 0);
                DunnageCount = result.Data.GetValueOrDefault("Dunnage", 0);
                VolvoCount = result.Data.GetValueOrDefault("Volvo", 0);

                IsReceivingEnabled = ReceivingCount > 0;
                IsDunnageEnabled = DunnageCount > 0;
                IsVolvoEnabled = VolvoCount > 0;

                if (!IsReceivingEnabled)
                {
                    IsReceivingChecked = false;
                }

                if (!IsDunnageEnabled)
                {
                    IsDunnageChecked = false;
                }

                if (!IsVolvoEnabled)
                {
                    IsVolvoChecked = false;
                }

                var totalCount = ReceivingCount + DunnageCount + VolvoCount;
                if (totalCount == 0)
                {
                    ShowStatus("No report data found for the selected date range", InfoBarSeverity.Warning);
                }
                else
                {
                    ShowStatus($"Found {totalCount} total records", InfoBarSeverity.Success);
                }
            }
            else
            {
                ResetAvailabilityState();
                ClearReportState();
                ShowStatus(result.ErrorMessage ?? "Failed to check availability", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking availability: {ex.Message}", ex);
            ResetAvailabilityState();
            ClearReportState();
            ShowStatus("Error checking data availability", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
            NotifyGenerateCommands();
            CopyEmailFormatCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanGenerateReceiving))]
    private async Task GenerateReceivingReportAsync()
    {
        await GenerateReportForModuleAsync("Receiving",
            () => _reportingService.GetReceivingHistoryAsync(StartDate.DateTime, EndDate.DateTime));
    }

    private bool CanGenerateReceiving() => IsReceivingChecked && IsReceivingEnabled && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanGenerateDunnage))]
    private async Task GenerateDunnageReportAsync()
    {
        await GenerateReportForModuleAsync("Dunnage",
            () => _reportingService.GetDunnageHistoryAsync(StartDate.DateTime, EndDate.DateTime));
    }

    private bool CanGenerateDunnage() => IsDunnageChecked && IsDunnageEnabled && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanGenerateVolvo))]
    private async Task GenerateVolvoReportAsync()
    {
        await GenerateReportForModuleAsync("Volvo",
            () => _reportingService.GetVolvoHistoryAsync(StartDate.DateTime, EndDate.DateTime));
    }

    private bool CanGenerateVolvo() => IsVolvoChecked && IsVolvoEnabled && !IsBusy;

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
            NotifyGenerateCommands();
            CopyEmailFormatCommand.NotifyCanExecuteChanged();
            ShowStatus("Copying formatted report...", InfoBarSeverity.Informational);

            var result = await _reportingService.FormatForEmailAsync(
                ReportData.ToList(),
                applyDateGrouping: true);

            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
            {
                var clipboardResult = _reportingClipboard.CreateClipboardPackage(ReportData.ToList(), result.Data);
                if (!clipboardResult.IsSuccess || clipboardResult.Data == null)
                {
                    ShowStatus(clipboardResult.ErrorMessage ?? "Failed to prepare clipboard content", InfoBarSeverity.Error);
                    return;
                }

                Clipboard.SetContent(clipboardResult.Data);

                ShowStatus("Formatted report copied to clipboard", InfoBarSeverity.Success);
                _logger.LogInfo("Formatted report copied to clipboard");
            }
            else
            {
                ClearReportState();
                ShowStatus(result.ErrorMessage ?? "Formatting failed", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error copying email format: {ex.Message}", ex);
            CopyEmailFormatCommand.NotifyCanExecuteChanged();
            ShowStatus("Error copying formatted report", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
            NotifyGenerateCommands();
            CopyEmailFormatCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanCopyEmail() => ReportData.Count > 0 && !IsBusy;

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
            NotifyGenerateCommands();
            CopyEmailFormatCommand.NotifyCanExecuteChanged();
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
                if (ReportData.Count == 0)
                {
                    ShowStatus($"No {moduleName} records found for the selected date range", InfoBarSeverity.Warning);
                }
                else
                {
                    ShowStatus($"Loaded {ReportData.Count} {moduleName} records", InfoBarSeverity.Success);
                }

                _logger.LogInfo($"Generated {moduleName} report: {ReportData.Count} records");
                CopyEmailFormatCommand.NotifyCanExecuteChanged();
            }
            else
            {
                ClearReportState();
                ShowStatus(result.ErrorMessage ?? $"Failed to load {moduleName} data", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating {moduleName} report: {ex.Message}", ex);
            ClearReportState();
            ShowStatus($"Error generating {moduleName} report", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
            NotifyGenerateCommands();
            CopyEmailFormatCommand.NotifyCanExecuteChanged();
        }
    }

    private void ResetAvailabilityState()
    {
        ReceivingCount = 0;
        DunnageCount = 0;
        VolvoCount = 0;
        IsReceivingEnabled = false;
        IsDunnageEnabled = false;
        IsVolvoEnabled = false;
        IsReceivingChecked = false;
        IsDunnageChecked = false;
        IsVolvoChecked = false;
        NotifyGenerateCommands();
    }

    private void ClearReportState()
    {
        ReportData.Clear();
        CurrentModuleName = string.Empty;
        CopyEmailFormatCommand.NotifyCanExecuteChanged();
    }

    private void ResetForDateRangeChange()
    {
        ResetAvailabilityState();
        ClearReportState();
        ShowStatus("Date range changed. Check availability to refresh the report options.", InfoBarSeverity.Informational);
    }

    private void NotifyGenerateCommands()
    {
        GenerateReceivingReportCommand.NotifyCanExecuteChanged();
        GenerateDunnageReportCommand.NotifyCanExecuteChanged();
        GenerateVolvoReportCommand.NotifyCanExecuteChanged();
    }

    partial void OnStartDateChanged(DateTimeOffset value)
    {
        ResetForDateRangeChange();
    }

    partial void OnEndDateChanged(DateTimeOffset value)
    {
        ResetForDateRangeChange();
    }

    partial void OnIsReceivingEnabledChanged(bool value)
    {
        GenerateReceivingReportCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsDunnageEnabledChanged(bool value)
    {
        GenerateDunnageReportCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsVolvoEnabledChanged(bool value)
    {
        GenerateVolvoReportCommand.NotifyCanExecuteChanged();
    }

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
