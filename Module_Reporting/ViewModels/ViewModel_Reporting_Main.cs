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
    private Enum_ReportingPreviewRowDisplayMode _selectedRowDisplayMode =
        Enum_ReportingPreviewRowDisplayMode.RawRows;

    [ObservableProperty]
    private bool _isOptionsOpen;

    [ObservableProperty]
    private Model_ReportingPreviewModuleCard? _receivingPreviewModuleCard;

    [ObservableProperty]
    private Model_ReportingPreviewModuleCard? _dunnagePreviewModuleCard;

    [ObservableProperty]
    private Model_ReportingPreviewModuleCard? _volvoPreviewModuleCard;

    [ObservableProperty]
    private string _previewSummaryTitle = string.Empty;

    [ObservableProperty]
    private double _previewCardWidth = 1320d;

    [ObservableProperty]
    private double _previewTableViewportWidth = 1260d;

    [ObservableProperty]
    private bool _hasIncludedPreviewModuleCards;

    public bool HasReceivingPreviewModuleCard => ReceivingPreviewModuleCard is not null;

    public bool HasDunnagePreviewModuleCard => DunnagePreviewModuleCard is not null;

    public bool HasVolvoPreviewModuleCard => VolvoPreviewModuleCard is not null;

    public bool HasReceivingIncludedPreviewModuleCard =>
        ReceivingPreviewModuleCard?.IsIncluded == true;

    public bool HasDunnageIncludedPreviewModuleCard => DunnagePreviewModuleCard?.IsIncluded == true;

    public bool HasVolvoIncludedPreviewModuleCard => VolvoPreviewModuleCard?.IsIncluded == true;

    public bool IsRawRowsMode
    {
        get => SelectedRowDisplayMode == Enum_ReportingPreviewRowDisplayMode.RawRows;
        set
        {
            if (value)
            {
                SelectedRowDisplayMode = Enum_ReportingPreviewRowDisplayMode.RawRows;
            }
        }
    }

    public bool IsUniquePartNumbersEntireDateRangeMode
    {
        get =>
            SelectedRowDisplayMode
            == Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange;
        set
        {
            if (value)
            {
                SelectedRowDisplayMode =
                    Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange;
            }
        }
    }

    public bool IsUniquePartNumbersAndLotNumbersEntireDateRangeMode
    {
        get =>
            SelectedRowDisplayMode
            == Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersAndLotNumbersEntireDateRange;
        set
        {
            if (value)
            {
                SelectedRowDisplayMode =
                    Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersAndLotNumbersEntireDateRange;
            }
        }
    }

    public bool IsUniquePartNumbersPerDayMode
    {
        get =>
            SelectedRowDisplayMode == Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersPerDay;
        set
        {
            if (value)
            {
                SelectedRowDisplayMode =
                    Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersPerDay;
            }
        }
    }

    public bool IsUniquePartNumbersAndLotNumbersPerDayMode
    {
        get =>
            SelectedRowDisplayMode
            == Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersAndLotNumbersPerDay;
        set
        {
            if (value)
            {
                SelectedRowDisplayMode =
                    Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersAndLotNumbersPerDay;
            }
        }
    }

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
                IncludedPreviewModuleCards.ToList(),
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

    [RelayCommand]
    private void OpenOptions()
    {
        IsOptionsOpen = true;
    }

    [RelayCommand]
    private void CloseOptions()
    {
        IsOptionsOpen = false;
    }

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
        ReceivingPreviewModuleCard = null;
        DunnagePreviewModuleCard = null;
        VolvoPreviewModuleCard = null;

        var sectionList = sections.Where(section => section.Rows.Count > 0).ToList();
        var summaryTableList = summaryTables.ToList();
        var previewModuleCards = new List<Model_ReportingPreviewModuleCard>();
        var summaryLookup = summaryTableList.ToDictionary(
            table => table.ModuleName,
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var section in sectionList)
        {
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
            var previewModuleCard = CreatePreviewModuleCard(section, summaryTable);
            previewModuleCards.Add(previewModuleCard);
            SetModulePreviewCard(previewModuleCard);
        }

        PreviewSections = new ObservableCollection<Model_ReportSection>(sectionList);
        PreviewSummaryTables = new ObservableCollection<Model_ReportSummaryTable>(summaryTableList);
        PreviewModuleCards = new ObservableCollection<Model_ReportingPreviewModuleCard>(
            previewModuleCards
        );

        RefreshIncludedPreviewModuleCards();

        UpdatePreviewLayout();
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
            RowDisplayMode = SelectedRowDisplayMode,
        };

        previewModuleCard.PropertyChanged += OnPreviewModuleCardPropertyChanged;
        previewModuleCard.InitializeColumns(CreateDetailColumnOptions(section));
        return previewModuleCard;
    }

    private void UpdatePreviewLayout()
    {
        var widestTableWidth = PreviewModuleCards
            .Select(card =>
                Math.Max(card.SummaryTable.TableWidth, Math.Max(card.DetailTableWidth, 720d))
            )
            .DefaultIfEmpty(1260d)
            .Max();

        PreviewCardWidth = Math.Clamp(widestTableWidth + 84d, 1320d, 2200d);
        PreviewTableViewportWidth = Math.Max(PreviewCardWidth - 48d, 1240d);
    }

    private static List<Model_ReportingPreviewColumnOption> CreateDetailColumnOptions(
        Model_ReportSection section
    )
    {
        var candidateColumns = new List<Model_ReportingPreviewColumnOption>();
        candidateColumns.Add(CreateColumn(nameof(Model_ReportRow.DisplayPo), "PO / Line", 180d));
        candidateColumns.Add(CreateColumn(nameof(Model_ReportRow.PONumber), "PO Number", 140d));
        candidateColumns.Add(CreateColumn(nameof(Model_ReportRow.POLineNumber), "PO Line #", 110d));
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.DisplayPartOrDunnage), "Part / Dunnage", 220d, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.PartNumber), "Part Number", 170d, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.PartDescription), "Part Description", 240d, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.DunnageType), "Dunnage Type", 180d, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.SpecsCombined), "Specs Combined", 260d, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.DisplayQuantity), "Quantity", 110d, false, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.Quantity), "Raw Quantity", 120d, false, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.WeightLbs), "Weight Lbs", 110d, false, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.HeatLotNumber), "Heat/Lot", 140d, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.CreatedDate), "Created Date", 120d)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.DisplayCreatedAt), "Created At", 170d)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.DisplayTransactionDate), "Transaction Date", 140d)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.EmployeeNumber), "Employee", 120d)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.CreatedByUsername), "Created By", 160d)
        );
        candidateColumns.Add(CreateColumn(nameof(Model_ReportRow.UserId), "User ID", 170d));
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.ShipmentNumber), "Shipment #", 120d, false, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.ReceiverNumber), "Receiver #", 120d)
        );
        candidateColumns.Add(CreateColumn(nameof(Model_ReportRow.Status), "Status", 130d));
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.PartCount), "Part Count", 110d, false, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.DisplayLocation), "Location", 170d, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.VendorName), "Vendor", 220d, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.DisplayNotes), "Notes", 240d, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.LoadNumber), "Load #", 100d, false, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.LabelNumber), "Label #", 100d, false, true)
        );
        candidateColumns.Add(
            CreateColumn(
                nameof(Model_ReportRow.DisplayLoadsOrSkids),
                "Loads / Skids",
                130d,
                false,
                true
            )
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.DisplayUnitsPerSkid), "Units Per Skid", 170d, true)
        );
        candidateColumns.Add(
            CreateColumn(
                nameof(Model_ReportRow.PackagesPerLoad),
                "Packages/Load",
                130d,
                false,
                true
            )
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.PackageTypeName), "Package Type", 140d, true)
        );
        candidateColumns.Add(
            CreateColumn(
                nameof(Model_ReportRow.WeightPerPackage),
                "Weight/Package",
                140d,
                false,
                true
            )
        );
        candidateColumns.Add(CreateColumn(nameof(Model_ReportRow.PoStatus), "PO Status", 130d));
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.DisplayPoDueDate), "PO Due Date", 140d)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.QtyOrdered), "Qty Ordered", 130d, false, true)
        );
        candidateColumns.Add(CreateColumn(nameof(Model_ReportRow.UnitOfMeasure), "UOM", 90d));
        candidateColumns.Add(
            CreateColumn(
                nameof(Model_ReportRow.RemainingQuantity),
                "Remaining Qty",
                140d,
                false,
                true
            )
        );
        candidateColumns.Add(CreateColumn(nameof(Model_ReportRow.IsNonPOItem), "Non-PO", 90d));
        candidateColumns.Add(
            CreateColumn(
                nameof(Model_ReportRow.IsQualityHoldRequired),
                "Quality Hold Required",
                170d
            )
        );
        candidateColumns.Add(
            CreateColumn(
                nameof(Model_ReportRow.IsQualityHoldAcknowledged),
                "Quality Hold Ack",
                150d
            )
        );
        candidateColumns.Add(
            CreateColumn(
                nameof(Model_ReportRow.QualityHoldRestrictionType),
                "Quality Hold Type",
                180d,
                true
            )
        );
        candidateColumns.Add(
            CreateColumn(
                nameof(Model_ReportRow.PartSkidTotal),
                "Part Skid Total",
                140d,
                false,
                true
            )
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.CoilsOnSkid), "Coils/Skid", 110d, false, true)
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.QuantityPerSkid), "Qty/Skid", 110d, false, true)
        );
        candidateColumns.Add(
            CreateColumn(
                nameof(Model_ReportRow.ReceivedSkidCount),
                "Received Skids",
                130d,
                false,
                true
            )
        );
        candidateColumns.Add(
            CreateColumn(nameof(Model_ReportRow.SourceModule), "Source Module", 130d)
        );
        candidateColumns.Add(CreateColumn(nameof(Model_ReportRow.Id), "ID", 150d));

        var sectionRows = section.Rows.ToList();
        var preferredKeys = GetPreferredPreviewColumnKeys(section.ModuleName);

        var availableColumns = candidateColumns
            .Where(column => SectionHasDataForColumn(sectionRows, column.Key))
            .ToList();

        foreach (var availableColumn in availableColumns)
        {
            availableColumn.IsIncluded = preferredKeys.Contains(availableColumn.Key);
        }

        if (availableColumns.All(column => !column.IsIncluded))
        {
            foreach (var availableColumn in availableColumns.Take(6))
            {
                availableColumn.IsIncluded = true;
            }
        }

        return availableColumns;
    }

    private static HashSet<string> GetPreferredPreviewColumnKeys(string moduleName)
    {
        return moduleName switch
        {
            "Receiving" => new HashSet<string>(StringComparer.Ordinal)
            {
                nameof(Model_ReportRow.DisplayPo),
                nameof(Model_ReportRow.DisplayPartOrDunnage),
                nameof(Model_ReportRow.DisplayQuantity),
                nameof(Model_ReportRow.DisplayCreatedAt),
                nameof(Model_ReportRow.EmployeeNumber),
                nameof(Model_ReportRow.DisplayLocation),
                nameof(Model_ReportRow.DisplayLoadsOrSkids),
                nameof(Model_ReportRow.DisplayUnitsPerSkid),
                nameof(Model_ReportRow.DisplayNotes),
            },
            "Dunnage" => new HashSet<string>(StringComparer.Ordinal)
            {
                nameof(Model_ReportRow.PONumber),
                nameof(Model_ReportRow.DisplayPartOrDunnage),
                nameof(Model_ReportRow.DisplayQuantity),
                nameof(Model_ReportRow.CreatedDate),
                nameof(Model_ReportRow.EmployeeNumber),
                nameof(Model_ReportRow.CreatedByUsername),
                nameof(Model_ReportRow.DisplayLocation),
                nameof(Model_ReportRow.DisplayNotes),
            },
            "Volvo" => new HashSet<string>(StringComparer.Ordinal)
            {
                nameof(Model_ReportRow.PONumber),
                nameof(Model_ReportRow.PartNumber),
                nameof(Model_ReportRow.DisplayQuantity),
                nameof(Model_ReportRow.CreatedDate),
                nameof(Model_ReportRow.EmployeeNumber),
                nameof(Model_ReportRow.ShipmentNumber),
                nameof(Model_ReportRow.ReceiverNumber),
                nameof(Model_ReportRow.Status),
                nameof(Model_ReportRow.PartCount),
                nameof(Model_ReportRow.QuantityPerSkid),
                nameof(Model_ReportRow.ReceivedSkidCount),
                nameof(Model_ReportRow.DisplayLocation),
                nameof(Model_ReportRow.DisplayNotes),
            },
            _ => new HashSet<string>(StringComparer.Ordinal)
            {
                nameof(Model_ReportRow.CreatedDate),
                nameof(Model_ReportRow.DisplayPartOrDunnage),
                nameof(Model_ReportRow.DisplayQuantity),
                nameof(Model_ReportRow.DisplayLocation),
                nameof(Model_ReportRow.DisplayNotes),
            },
        };
    }

    private static bool SectionHasDataForColumn(IEnumerable<Model_ReportRow> rows, string columnKey)
    {
        return rows.Any(row => !string.IsNullOrWhiteSpace(row.GetColumnValue(columnKey)));
    }

    private static Model_ReportingPreviewColumnOption CreateColumn(
        string key,
        string header,
        double width,
        bool wrapText = false,
        bool isNumeric = false
    )
    {
        return new Model_ReportingPreviewColumnOption
        {
            Key = key,
            Header = header,
            Width = width,
            WrapText = wrapText,
            IsNumeric = isNumeric,
            IsIncluded = false,
        };
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

        if (e.PropertyName == nameof(Model_ReportingPreviewModuleCard.DetailTableWidth))
        {
            UpdatePreviewLayout();
            CopyEmailFormatCommand.NotifyCanExecuteChanged();
        }
    }

    private void RefreshIncludedPreviewModuleCards()
    {
        IncludedPreviewModuleCards = new ObservableCollection<Model_ReportingPreviewModuleCard>(
            PreviewModuleCards.Where(card => card.IsIncluded)
        );

        HasIncludedPreviewModuleCards = IncludedPreviewModuleCards.Count > 0;
        NotifyModulePreviewCardStateChanged();
        CopyEmailFormatCommand.NotifyCanExecuteChanged();
    }

    private void SetModulePreviewCard(Model_ReportingPreviewModuleCard previewModuleCard)
    {
        switch (previewModuleCard.ModuleName)
        {
            case "Receiving":
                ReceivingPreviewModuleCard = previewModuleCard;
                break;
            case "Dunnage":
                DunnagePreviewModuleCard = previewModuleCard;
                break;
            case "Volvo":
                VolvoPreviewModuleCard = previewModuleCard;
                break;
        }

        NotifyModulePreviewCardStateChanged();
    }

    private void ApplySelectedRowDisplayModeToPreviewCards()
    {
        foreach (var previewModuleCard in PreviewModuleCards)
        {
            previewModuleCard.RowDisplayMode = SelectedRowDisplayMode;
        }
    }

    private void NotifyModulePreviewCardStateChanged()
    {
        OnPropertyChanged(nameof(HasReceivingPreviewModuleCard));
        OnPropertyChanged(nameof(HasDunnagePreviewModuleCard));
        OnPropertyChanged(nameof(HasVolvoPreviewModuleCard));
        OnPropertyChanged(nameof(HasReceivingIncludedPreviewModuleCard));
        OnPropertyChanged(nameof(HasDunnageIncludedPreviewModuleCard));
        OnPropertyChanged(nameof(HasVolvoIncludedPreviewModuleCard));
    }

    private void ClearPreviewCardSubscriptions()
    {
        foreach (var previewModuleCard in PreviewModuleCards)
        {
            previewModuleCard.PropertyChanged -= OnPreviewModuleCardPropertyChanged;
            previewModuleCard.DetachColumnHandlers();
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
        PreviewSections = [];
        PreviewSummaryTables = [];
        PreviewModuleCards = [];
        IncludedPreviewModuleCards = [];
        ReceivingPreviewModuleCard = null;
        DunnagePreviewModuleCard = null;
        VolvoPreviewModuleCard = null;
        PreviewSummaryTitle = string.Empty;
        PreviewCardWidth = 1320d;
        PreviewTableViewportWidth = 1260d;
        HasIncludedPreviewModuleCards = false;
        IsOptionsOpen = false;
        NotifyModulePreviewCardStateChanged();
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

    partial void OnSelectedRowDisplayModeChanged(Enum_ReportingPreviewRowDisplayMode value)
    {
        ApplySelectedRowDisplayModeToPreviewCards();
        OnPropertyChanged(nameof(IsRawRowsMode));
        OnPropertyChanged(nameof(IsUniquePartNumbersEntireDateRangeMode));
        OnPropertyChanged(nameof(IsUniquePartNumbersAndLotNumbersEntireDateRangeMode));
        OnPropertyChanged(nameof(IsUniquePartNumbersPerDayMode));
        OnPropertyChanged(nameof(IsUniquePartNumbersAndLotNumbersPerDayMode));
    }

    private sealed record SelectedModuleRequest(
        string ModuleName,
        string Description,
        Func<Task<Model_Dao_Result<List<Model_ReportRow>>>> FetchDataAsync
    );
}
