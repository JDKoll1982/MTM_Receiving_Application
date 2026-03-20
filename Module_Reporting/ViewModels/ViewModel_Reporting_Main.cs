using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Reporting.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using Windows.ApplicationModel.DataTransfer;

namespace MTM_Receiving_Application.Module_Reporting.ViewModels;

public partial class ViewModel_Reporting_Main : ViewModel_Shared_Base
{
    private readonly IService_Reporting _reportingService;
    private readonly IService_ReportingClipboard _reportingClipboard;
    private bool _isSynchronizingModuleSelections;

    public event EventHandler? PreviewRequested;

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
    private ObservableCollection<Model_ReportSection> _previewSections = [];

    [ObservableProperty]
    private ObservableCollection<Model_ReportSummaryTable> _previewSummaryTables = [];

    [ObservableProperty]
    private ObservableCollection<Model_ReportingPreviewModuleCard> _previewModuleCards = [];

    [ObservableProperty]
    private ObservableCollection<Model_ReportingPreviewModuleCard> _includedPreviewModuleCards = [];

    [ObservableProperty]
    private string _previewSummaryTitle = string.Empty;

    [ObservableProperty]
    private double _previewCardWidth = 940d;

    [ObservableProperty]
    private double _previewTableViewportWidth = 908d;

    [ObservableProperty]
    private bool _hasIncludedPreviewModuleCards;

    public ViewModel_Reporting_Main(
        IService_Reporting reportingService,
        IService_ReportingClipboard reportingClipboard,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _reportingService =
            reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        _reportingClipboard =
            reportingClipboard ?? throw new ArgumentNullException(nameof(reportingClipboard));
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
            NotifyActionCommands();
            ShowStatus("Checking data availability...", InfoBarSeverity.Informational);

            var result = await _reportingService.CheckAvailabilityAsync(
                StartDate.DateTime,
                EndDate.DateTime
            );

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
                ShowStatus(
                    totalCount == 0
                        ? "No report data found for the selected date range"
                        : $"Found {totalCount} total records",
                    totalCount == 0 ? InfoBarSeverity.Warning : InfoBarSeverity.Success
                );
            }
            else
            {
                ResetAvailabilityState();
                ClearPreviewState();
                ShowStatus(
                    result.ErrorMessage ?? "Failed to check availability",
                    InfoBarSeverity.Error
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking availability: {ex.Message}", ex);
            ResetAvailabilityState();
            ClearPreviewState();
            ShowStatus("Error checking data availability", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
            NotifyActionCommands();
        }
    }

    [RelayCommand(CanExecute = nameof(CanGenerateReports))]
    private async Task GenerateReportsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var selectedModules = GetSelectedModules().ToList();
        if (selectedModules.Count == 0)
        {
            ShowStatus(
                "Select at least one module before generating a report.",
                InfoBarSeverity.Warning
            );
            return;
        }

        try
        {
            IsBusy = true;
            NotifyActionCommands();
            ShowStatus("Generating report preview...", InfoBarSeverity.Informational);

            var sections = new List<Model_ReportSection>();

            foreach (var module in selectedModules)
            {
                var result = await module.FetchDataAsync();
                if (!result.IsSuccess)
                {
                    ClearPreviewState();
                    ShowStatus(
                        result.ErrorMessage ?? $"Failed to load {module.ModuleName} data",
                        InfoBarSeverity.Error
                    );
                    return;
                }

                sections.Add(
                    new Model_ReportSection
                    {
                        ModuleName = module.ModuleName,
                        Title = $"{module.ModuleName} Activity for {GetDateRangeText()}",
                        Description = module.Description,
                        Rows = new ObservableCollection<Model_ReportRow>(result.Data ?? []),
                    }
                );
            }

            var summaryTablesResult = await _reportingService.BuildSummaryTablesAsync(sections);
            if (!summaryTablesResult.IsSuccess || summaryTablesResult.Data is null)
            {
                ClearPreviewState();
                ShowStatus(
                    summaryTablesResult.ErrorMessage ?? "Failed to build report summaries",
                    InfoBarSeverity.Error
                );
                return;
            }

            BuildPreviewState(sections, summaryTablesResult.Data);

            if (PreviewSections.Count == 0)
            {
                ShowStatus(
                    "No report data found for the selected modules and date range.",
                    InfoBarSeverity.Warning
                );
                return;
            }

            ShowStatus(
                $"Prepared preview for {PreviewSections.Count} selected module(s)",
                InfoBarSeverity.Success
            );
            PreviewRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error generating reports: {ex.Message}", ex);
            ClearPreviewState();
            ShowStatus("Error generating report preview", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
            NotifyActionCommands();
        }
    }

    private bool CanGenerateReports() => !IsBusy && GetSelectedModules().Any();

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
            NotifyActionCommands();
            ShowStatus("Copying formatted report...", InfoBarSeverity.Informational);

            var formatResult = await _reportingService.FormatForEmailAsync(
                IncludedPreviewModuleCards.Select(card => card.DetailSection).ToList(),
                IncludedPreviewModuleCards.Select(card => card.SummaryTable).ToList(),
                PreviewSummaryTitle
            );

            if (!formatResult.IsSuccess || formatResult.Data == null)
            {
                ShowStatus(formatResult.ErrorMessage ?? "Formatting failed", InfoBarSeverity.Error);
                return;
            }

            var clipboardResult = _reportingClipboard.CreateClipboardPackage(formatResult.Data);
            if (!clipboardResult.IsSuccess || clipboardResult.Data == null)
            {
                ShowStatus(
                    clipboardResult.ErrorMessage ?? "Failed to prepare clipboard content",
                    InfoBarSeverity.Error
                );
                return;
            }

            Clipboard.SetContent(clipboardResult.Data);
            ShowStatus("Formatted report copied to clipboard", InfoBarSeverity.Success);
            _logger.LogInfo("Formatted report copied to clipboard");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error copying email format: {ex.Message}", ex);
            ShowStatus("Error copying formatted report", InfoBarSeverity.Error);
        }
        finally
        {
            IsBusy = false;
            NotifyActionCommands();
        }
    }

    private bool CanCopyEmail() => IncludedPreviewModuleCards.Count > 0 && !IsBusy;

    private IEnumerable<SelectedModuleRequest> GetSelectedModules()
    {
        if (IsReceivingChecked && IsReceivingEnabled)
        {
            yield return new SelectedModuleRequest(
                "Receiving",
                "Detailed receiving rows captured for the selected date range.",
                () =>
                    _reportingService.GetReceivingHistoryAsync(StartDate.DateTime, EndDate.DateTime)
            );
        }

        if (IsDunnageChecked && IsDunnageEnabled)
        {
            yield return new SelectedModuleRequest(
                "Dunnage",
                "Detailed dunnage activity captured for the selected date range.",
                () => _reportingService.GetDunnageHistoryAsync(StartDate.DateTime, EndDate.DateTime)
            );
        }

        if (IsVolvoChecked && IsVolvoEnabled)
        {
            yield return new SelectedModuleRequest(
                "Volvo",
                "Detailed Volvo activity captured for the selected date range.",
                () => _reportingService.GetVolvoHistoryAsync(StartDate.DateTime, EndDate.DateTime)
            );
        }
    }

    private void BuildPreviewState(
        IEnumerable<Model_ReportSection> sections,
        IEnumerable<Model_ReportSummaryTable> summaryTables
    )
    {
        ClearPreviewCardSubscriptions();
        PreviewSections.Clear();
        PreviewSummaryTables.Clear();
        PreviewModuleCards.Clear();
        IncludedPreviewModuleCards.Clear();

        var sectionList = sections.Where(section => section.Rows.Count > 0).ToList();
        var summaryTableList = summaryTables.ToList();
        var summaryLookup = summaryTableList.ToDictionary(
            table => table.ModuleName,
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var section in sectionList)
        {
            PreviewSections.Add(section);

            var summaryTable = summaryLookup.TryGetValue(
                section.ModuleName,
                out var matchedSummaryTable
            )
                ? matchedSummaryTable
                : new Model_ReportSummaryTable
                {
                    ModuleName = section.ModuleName,
                    Title = $"{section.ModuleName} Summary",
                };

            PreviewModuleCards.Add(CreatePreviewModuleCard(section, summaryTable));
        }

        foreach (var summaryTable in summaryTableList)
        {
            PreviewSummaryTables.Add(summaryTable);
        }

        RefreshIncludedPreviewModuleCards();

        var widestTableWidth = PreviewModuleCards
            .Select(card => Math.Max(card.SummaryTable.TableWidth, card.DetailTableWidth))
            .DefaultIfEmpty(900d)
            .Max();

        PreviewCardWidth = Math.Max(980d, widestTableWidth + 56d);
        PreviewTableViewportWidth = Math.Max(920d, PreviewCardWidth - 32d);
        PreviewSummaryTitle = $"End of Day Report Preview for {GetDateRangeText()}";
        CopyEmailFormatCommand.NotifyCanExecuteChanged();
    }

    private Model_ReportingPreviewModuleCard CreatePreviewModuleCard(
        Model_ReportSection section,
        Model_ReportSummaryTable summaryTable
    )
    {
        var (accentBackgroundBrush, accentForegroundBrush) = GetModulePreviewBrushes(
            section.ModuleName
        );
        var previewModuleCard = new Model_ReportingPreviewModuleCard
        {
            ModuleName = section.ModuleName,
            CardTitle = $"{section.ModuleName} Report for {GetDateRangeText()}",
            Description = section.Description,
            AccentBackgroundBrush = accentBackgroundBrush,
            AccentForegroundBrush = accentForegroundBrush,
            SummaryTable = summaryTable,
            DetailSection = section,
            IsIncluded = GetModuleCheckedState(section.ModuleName),
        };

        previewModuleCard.PropertyChanged += OnPreviewModuleCardPropertyChanged;
        return previewModuleCard;
    }

    private string GetDateRangeText() => $"{StartDate:M/d/yyyy} - {EndDate:M/d/yyyy}";

    private void OnPreviewModuleCardPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Model_ReportingPreviewModuleCard.IsIncluded))
        {
            if (sender is Model_ReportingPreviewModuleCard previewModuleCard)
            {
                SyncMainModuleSelection(previewModuleCard.ModuleName, previewModuleCard.IsIncluded);
            }

            RefreshIncludedPreviewModuleCards();
        }
    }

    private void RefreshIncludedPreviewModuleCards()
    {
        IncludedPreviewModuleCards.Clear();

        foreach (var previewModuleCard in PreviewModuleCards.Where(card => card.IsIncluded))
        {
            IncludedPreviewModuleCards.Add(previewModuleCard);
        }

        HasIncludedPreviewModuleCards = IncludedPreviewModuleCards.Count > 0;
        CopyEmailFormatCommand.NotifyCanExecuteChanged();
    }

    private void ClearPreviewCardSubscriptions()
    {
        foreach (var previewModuleCard in PreviewModuleCards)
        {
            previewModuleCard.PropertyChanged -= OnPreviewModuleCardPropertyChanged;
        }
    }

    private static (
        SolidColorBrush AccentBackgroundBrush,
        SolidColorBrush AccentForegroundBrush
    ) GetModulePreviewBrushes(string moduleName)
    {
        return moduleName switch
        {
            "Receiving" => (
                new SolidColorBrush(ColorHelper.FromArgb(255, 11, 97, 87)),
                new SolidColorBrush(Colors.White)
            ),
            "Dunnage" => (
                new SolidColorBrush(ColorHelper.FromArgb(255, 138, 94, 0)),
                new SolidColorBrush(Colors.White)
            ),
            "Volvo" => (
                new SolidColorBrush(ColorHelper.FromArgb(255, 30, 79, 122)),
                new SolidColorBrush(Colors.White)
            ),
            _ => (
                new SolidColorBrush(ColorHelper.FromArgb(255, 74, 85, 104)),
                new SolidColorBrush(Colors.White)
            ),
        };
    }

    private bool GetModuleCheckedState(string moduleName)
    {
        return moduleName switch
        {
            "Receiving" => IsReceivingChecked,
            "Dunnage" => IsDunnageChecked,
            "Volvo" => IsVolvoChecked,
            _ => true,
        };
    }

    private void SyncMainModuleSelection(string moduleName, bool isIncluded)
    {
        if (_isSynchronizingModuleSelections)
        {
            return;
        }

        _isSynchronizingModuleSelections = true;

        try
        {
            switch (moduleName)
            {
                case "Receiving":
                    IsReceivingChecked = isIncluded;
                    break;
                case "Dunnage":
                    IsDunnageChecked = isIncluded;
                    break;
                case "Volvo":
                    IsVolvoChecked = isIncluded;
                    break;
            }
        }
        finally
        {
            _isSynchronizingModuleSelections = false;
        }
    }

    private void SyncPreviewModuleSelection(string moduleName, bool isIncluded)
    {
        if (_isSynchronizingModuleSelections)
        {
            return;
        }

        var previewModuleCard = PreviewModuleCards.FirstOrDefault(card =>
            card.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase)
        );
        if (previewModuleCard is null || previewModuleCard.IsIncluded == isIncluded)
        {
            return;
        }

        _isSynchronizingModuleSelections = true;

        try
        {
            previewModuleCard.IsIncluded = isIncluded;
        }
        finally
        {
            _isSynchronizingModuleSelections = false;
        }

        RefreshIncludedPreviewModuleCards();
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
        NotifyActionCommands();
    }

    private void ClearPreviewState()
    {
        ClearPreviewCardSubscriptions();
        PreviewSections.Clear();
        PreviewSummaryTables.Clear();
        PreviewModuleCards.Clear();
        IncludedPreviewModuleCards.Clear();
        PreviewSummaryTitle = string.Empty;
        PreviewCardWidth = 940d;
        PreviewTableViewportWidth = 908d;
        HasIncludedPreviewModuleCards = false;
        CopyEmailFormatCommand.NotifyCanExecuteChanged();
    }

    private void ResetForDateRangeChange()
    {
        ResetAvailabilityState();
        ClearPreviewState();
        ShowStatus(
            "Date range changed. Check availability to refresh the report options.",
            InfoBarSeverity.Informational
        );
    }

    private void NotifyActionCommands()
    {
        GenerateReportsCommand.NotifyCanExecuteChanged();
        CopyEmailFormatCommand.NotifyCanExecuteChanged();
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
        GenerateReportsCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsDunnageEnabledChanged(bool value)
    {
        GenerateReportsCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsVolvoEnabledChanged(bool value)
    {
        GenerateReportsCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsReceivingCheckedChanged(bool value)
    {
        SyncPreviewModuleSelection("Receiving", value);
        GenerateReportsCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsDunnageCheckedChanged(bool value)
    {
        SyncPreviewModuleSelection("Dunnage", value);
        GenerateReportsCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsVolvoCheckedChanged(bool value)
    {
        SyncPreviewModuleSelection("Volvo", value);
        GenerateReportsCommand.NotifyCanExecuteChanged();
    }

    private sealed record SelectedModuleRequest(
        string ModuleName,
        string Description,
        Func<Task<Model_Dao_Result<List<Model_ReportRow>>>> FetchDataAsync
    );
}
