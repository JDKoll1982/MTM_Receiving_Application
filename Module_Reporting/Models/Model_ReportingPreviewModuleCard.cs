using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Models.Reporting;

namespace MTM_Receiving_Application.Module_Reporting.Models;

public partial class Model_ReportingPreviewModuleCard : ObservableObject
{
    private const string NoLotLabel = "No Lot";
    private const string NoPartNumberLabel = "No Part Number";
    private const string LoadsOrSkidsColumnHeader = "Loads / Skids";
    private const double LoadsOrSkidsColumnWidth = 130d;
    private const string SourceOrderSortKey = "__source_order";

    private bool _isApplyingColumnRules;

    [ObservableProperty]
    private bool _isIncluded = true;

    [ObservableProperty]
    private Enum_ReportingPreviewRowDisplayMode _rowDisplayMode =
        Enum_ReportingPreviewRowDisplayMode.RawRows;

    [ObservableProperty]
    private string _selectedSortOptionKey = SourceOrderSortKey;

    [ObservableProperty]
    private Enum_ReportingPreviewSortDirection _selectedSortDirection =
        Enum_ReportingPreviewSortDirection.Ascending;

    public string ModuleName { get; set; } = string.Empty;

    public string CardTitle { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public SolidColorBrush AccentBackgroundBrush { get; set; } = null!;

    public SolidColorBrush AccentForegroundBrush { get; set; } = null!;

    public Model_ReportSummaryTable SummaryTable { get; set; } = new();

    public Model_ReportSection DetailSection { get; set; } = new();

    public double DetailTableWidth { get; set; } = 900d;

    public ObservableCollection<Model_ReportingPreviewColumnOption> AvailableColumns { get; } = [];

    public ObservableCollection<Model_ReportingPreviewSortOption> SortOptions { get; } = [];

    public ObservableCollection<Model_ReportingPreviewColumnOption> IncludedColumns { get; } = [];

    public ObservableCollection<Model_ReportingPreviewRow> PreviewRows { get; } = [];

    public ObservableCollection<Model_ReportRow> Rows => DetailSection.Rows;

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    public bool HasDetailSectionNote => !string.IsNullOrWhiteSpace(DetailSectionNote);

    public bool HasIncludedColumns => IncludedColumns.Count > 0;

    public string DetailSectionTitle => GetDetailSectionTitle();

    public string DetailSectionNote => GetDetailSectionNote();

    public bool IsSortDirectionEnabled =>
        !string.Equals(SelectedSortOptionKey, SourceOrderSortKey, StringComparison.Ordinal);

    public bool IsAscendingSortDirection
    {
        get => SelectedSortDirection == Enum_ReportingPreviewSortDirection.Ascending;
        set
        {
            if (value)
            {
                SelectedSortDirection = Enum_ReportingPreviewSortDirection.Ascending;
            }
        }
    }

    public bool IsDescendingSortDirection
    {
        get => SelectedSortDirection == Enum_ReportingPreviewSortDirection.Descending;
        set
        {
            if (value)
            {
                SelectedSortDirection = Enum_ReportingPreviewSortDirection.Descending;
            }
        }
    }

    public void InitializeColumns(IEnumerable<Model_ReportingPreviewColumnOption> availableColumns)
    {
        DetachColumnHandlers();

        AvailableColumns.Clear();
        foreach (var availableColumn in availableColumns)
        {
            availableColumn.PropertyChanged += OnColumnPropertyChanged;
            AvailableColumns.Add(availableColumn);
        }

        InitializeSortOptions();
        RefreshPreviewRows();
    }

    public void DetachColumnHandlers()
    {
        foreach (var availableColumn in AvailableColumns)
        {
            availableColumn.PropertyChanged -= OnColumnPropertyChanged;
        }
    }

    public void RefreshPreviewRows()
    {
        ApplyColumnRules();

        IncludedColumns.Clear();
        foreach (var availableColumn in AvailableColumns.Where(column => column.IsIncluded))
        {
            IncludedColumns.Add(availableColumn);
        }

        PreviewRows.Clear();
        foreach (var previewRowSource in BuildPreviewRowSources())
        {
            var previewRow = new Model_ReportingPreviewRow();
            foreach (var includedColumn in IncludedColumns)
            {
                previewRow.Cells.Add(
                    new Model_ReportingPreviewCell
                    {
                        Value = GetPreviewCellValue(previewRowSource, includedColumn.Key),
                        Width = includedColumn.Width,
                        WrapText = includedColumn.WrapText,
                        IsNumeric = includedColumn.IsNumeric,
                        TextAlignment = includedColumn.IsNumeric
                            ? TextAlignment.Right
                            : TextAlignment.Left,
                    }
                );
            }

            PreviewRows.Add(previewRow);
        }

        DetailTableWidth = IncludedColumns.Sum(column => column.Width);
        OnPropertyChanged(nameof(DetailTableWidth));
        OnPropertyChanged(nameof(HasIncludedColumns));
        OnPropertyChanged(nameof(DetailSectionTitle));
        OnPropertyChanged(nameof(DetailSectionNote));
        OnPropertyChanged(nameof(HasDetailSectionNote));
    }

    partial void OnRowDisplayModeChanged(Enum_ReportingPreviewRowDisplayMode value)
    {
        RefreshPreviewRows();
    }

    partial void OnSelectedSortOptionKeyChanged(string value)
    {
        OnPropertyChanged(nameof(IsSortDirectionEnabled));
        RefreshPreviewRows();
    }

    partial void OnSelectedSortDirectionChanged(Enum_ReportingPreviewSortDirection value)
    {
        OnPropertyChanged(nameof(IsAscendingSortDirection));
        OnPropertyChanged(nameof(IsDescendingSortDirection));
        RefreshPreviewRows();
    }

    private void OnColumnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isApplyingColumnRules)
        {
            return;
        }

        if (e.PropertyName == nameof(Model_ReportingPreviewColumnOption.IsIncluded))
        {
            RefreshPreviewRows();
        }
    }

    private void ApplyColumnRules()
    {
        var shouldForceCombinedRowCountColumn = IsCombinationModeSelected();
        var loadsOrSkidsColumn = GetOrCreateLoadsOrSkidsColumn(shouldForceCombinedRowCountColumn);

        if (loadsOrSkidsColumn is null)
        {
            return;
        }

        _isApplyingColumnRules = true;
        try
        {
            loadsOrSkidsColumn.CanChangeInOptions = !shouldForceCombinedRowCountColumn;

            if (shouldForceCombinedRowCountColumn && !loadsOrSkidsColumn.IsIncluded)
            {
                loadsOrSkidsColumn.IsIncluded = true;
            }
        }
        finally
        {
            _isApplyingColumnRules = false;
        }
    }

    private void InitializeSortOptions()
    {
        var previousSortKey = SelectedSortOptionKey;

        SortOptions.Clear();
        SortOptions.Add(
            new Model_ReportingPreviewSortOption
            {
                Key = SourceOrderSortKey,
                Label = "Source order",
            }
        );

        foreach (var availableColumn in AvailableColumns)
        {
            SortOptions.Add(
                new Model_ReportingPreviewSortOption
                {
                    Key = availableColumn.Key,
                    Label = availableColumn.Header,
                }
            );
        }

        if (SortOptions.Any(option => option.Key == previousSortKey))
        {
            OnPropertyChanged(nameof(IsSortDirectionEnabled));
            return;
        }

        SelectedSortOptionKey = SourceOrderSortKey;
    }

    private Model_ReportingPreviewColumnOption? GetOrCreateLoadsOrSkidsColumn(bool createIfMissing)
    {
        Model_ReportingPreviewColumnOption? existingColumn = null;
        foreach (var availableColumn in AvailableColumns)
        {
            if (availableColumn.Key == nameof(Model_ReportRow.DisplayLoadsOrSkids))
            {
                existingColumn = availableColumn;
                break;
            }
        }

        if (existingColumn is not null || !createIfMissing)
        {
            return existingColumn;
        }

        var generatedColumn = new Model_ReportingPreviewColumnOption
        {
            Key = nameof(Model_ReportRow.DisplayLoadsOrSkids),
            Header = LoadsOrSkidsColumnHeader,
            Width = LoadsOrSkidsColumnWidth,
            IsNumeric = true,
            IsIncluded = false,
        };

        generatedColumn.PropertyChanged += OnColumnPropertyChanged;
        AvailableColumns.Add(generatedColumn);
        return generatedColumn;
    }

    private List<PreviewRowSource> BuildPreviewRowSources()
    {
        var rows = Rows.ToList();
        var effectiveRowDisplayMode = GetEffectiveRowDisplayMode(rows);

        List<PreviewRowSource> previewRowSources;

        if (effectiveRowDisplayMode == Enum_ReportingPreviewRowDisplayMode.RawRows)
        {
            previewRowSources = new List<PreviewRowSource>(rows.Count);
            for (var index = 0; index < rows.Count; index++)
            {
                previewRowSources.Add(PreviewRowSource.CreateRaw(rows[index], index));
            }

            ApplySorting(previewRowSources);
            return previewRowSources;
        }

        IEnumerable<IGrouping<PreviewGroupingKey, Model_ReportRow>> groupedRows = rows.GroupBy(
            row => CreateGroupingKey(row, effectiveRowDisplayMode)
        );

        var orderedGroupedRows = effectiveRowDisplayMode switch
        {
            Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange =>
                groupedRows.OrderBy(
                    group => group.Key.PartNumber,
                    StringComparer.OrdinalIgnoreCase
                ),
            Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersAndLotNumbersEntireDateRange =>
                groupedRows
                    .OrderBy(group => group.Key.PartNumber, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(group => group.Key.LotNumber, StringComparer.OrdinalIgnoreCase),
            Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersPerDay => groupedRows
                .OrderBy(group => group.Key.CreatedDate)
                .ThenBy(group => group.Key.PartNumber, StringComparer.OrdinalIgnoreCase),
            _ => groupedRows
                .OrderBy(group => group.Key.CreatedDate)
                .ThenBy(group => group.Key.PartNumber, StringComparer.OrdinalIgnoreCase)
                .ThenBy(group => group.Key.LotNumber, StringComparer.OrdinalIgnoreCase),
        };

        previewRowSources = orderedGroupedRows
            .Select(
                (group, index) =>
                    new PreviewRowSource(
                        group.ToList(),
                        group.Key.PartNumber,
                        group.Key.LotNumber,
                        group.Key.CreatedDate,
                        index
                    )
            )
            .ToList();

        ApplySorting(previewRowSources);
        return previewRowSources;
    }

    private void ApplySorting(List<PreviewRowSource> previewRowSources)
    {
        if (
            previewRowSources.Count < 2
            || string.Equals(SelectedSortOptionKey, SourceOrderSortKey, StringComparison.Ordinal)
        )
        {
            return;
        }

        previewRowSources.Sort(ComparePreviewRowSources);
    }

    private int ComparePreviewRowSources(PreviewRowSource left, PreviewRowSource right)
    {
        var comparison = ComparePreviewRowSources(left, right, SelectedSortOptionKey);
        if (comparison == 0)
        {
            comparison = left.SourceIndex.CompareTo(right.SourceIndex);
        }

        return SelectedSortDirection == Enum_ReportingPreviewSortDirection.Descending
            ? -comparison
            : comparison;
    }

    private int ComparePreviewRowSources(
        PreviewRowSource left,
        PreviewRowSource right,
        string columnKey
    )
    {
        return columnKey switch
        {
            nameof(Model_ReportRow.CreatedDate) => CompareNullableDateTime(
                GetMinimumDateTime(
                    left.Rows.Select(
                        row => row.CreatedDate == default ? (DateTime?)null : row.CreatedDate.Date
                    )
                ),
                GetMinimumDateTime(
                    right.Rows.Select(
                        row => row.CreatedDate == default ? (DateTime?)null : row.CreatedDate.Date
                    )
                )
            ),
            nameof(Model_ReportRow.CreatedAt) or nameof(Model_ReportRow.DisplayCreatedAt) =>
                CompareNullableDateTime(
                    GetMinimumDateTime(
                        left.Rows.Select(
                            row => row.CreatedAt ?? (row.CreatedDate == default ? null : row.CreatedDate)
                        )
                    ),
                    GetMinimumDateTime(
                        right.Rows.Select(
                            row => row.CreatedAt ?? (row.CreatedDate == default ? null : row.CreatedDate)
                        )
                    )
                ),
            nameof(Model_ReportRow.TransactionDate)
            or nameof(Model_ReportRow.DisplayTransactionDate) => CompareNullableDateTime(
                GetMinimumDateTime(left.Rows.Select(row => row.TransactionDate)),
                GetMinimumDateTime(right.Rows.Select(row => row.TransactionDate))
            ),
            nameof(Model_ReportRow.PoDueDate) or nameof(Model_ReportRow.DisplayPoDueDate) =>
                CompareNullableDateTime(
                    GetMinimumDateTime(left.Rows.Select(row => row.PoDueDate)),
                    GetMinimumDateTime(right.Rows.Select(row => row.PoDueDate))
                ),
            nameof(Model_ReportRow.Quantity) or nameof(Model_ReportRow.DisplayQuantity) =>
                CompareDecimal(
                    left.Rows.Sum(row => row.Quantity ?? 0m),
                    right.Rows.Sum(row => row.Quantity ?? 0m)
                ),
            nameof(Model_ReportRow.WeightLbs) => CompareDecimal(
                left.Rows.Sum(row => row.WeightLbs ?? 0m),
                right.Rows.Sum(row => row.WeightLbs ?? 0m)
            ),
            nameof(Model_ReportRow.QtyOrdered) => CompareDecimal(
                left.Rows.Sum(row => row.QtyOrdered ?? 0m),
                right.Rows.Sum(row => row.QtyOrdered ?? 0m)
            ),
            nameof(Model_ReportRow.PartCount) => CompareInt(
                left.Rows.Sum(row => row.PartCount ?? 0),
                right.Rows.Sum(row => row.PartCount ?? 0)
            ),
            nameof(Model_ReportRow.RemainingQuantity) => CompareInt(
                left.Rows.Sum(row => row.RemainingQuantity ?? 0),
                right.Rows.Sum(row => row.RemainingQuantity ?? 0)
            ),
            nameof(Model_ReportRow.PartSkidTotal) => CompareInt(
                left.Rows.Sum(row => row.PartSkidTotal ?? 0),
                right.Rows.Sum(row => row.PartSkidTotal ?? 0)
            ),
            nameof(Model_ReportRow.ReceivedSkidCount) => CompareInt(
                left.Rows.Sum(row => row.ReceivedSkidCount ?? 0),
                right.Rows.Sum(row => row.ReceivedSkidCount ?? 0)
            ),
            nameof(Model_ReportRow.LoadNumber) => CompareNullableInt(
                GetMinimumInt(left.Rows.Select(row => row.LoadNumber)),
                GetMinimumInt(right.Rows.Select(row => row.LoadNumber))
            ),
            nameof(Model_ReportRow.LabelNumber) => CompareNullableInt(
                GetMinimumInt(left.Rows.Select(row => row.LabelNumber)),
                GetMinimumInt(right.Rows.Select(row => row.LabelNumber))
            ),
            nameof(Model_ReportRow.ShipmentNumber) => CompareNullableInt(
                GetMinimumInt(left.Rows.Select(row => row.ShipmentNumber)),
                GetMinimumInt(right.Rows.Select(row => row.ShipmentNumber))
            ),
            nameof(Model_ReportRow.PackagesPerLoad) => CompareNullableInt(
                GetMinimumInt(left.Rows.Select(row => row.PackagesPerLoad)),
                GetMinimumInt(right.Rows.Select(row => row.PackagesPerLoad))
            ),
            nameof(Model_ReportRow.WeightPerPackage) => CompareNullableDecimal(
                GetMinimumDecimal(left.Rows.Select(row => row.WeightPerPackage)),
                GetMinimumDecimal(right.Rows.Select(row => row.WeightPerPackage))
            ),
            nameof(Model_ReportRow.QuantityPerSkid) => CompareNullableInt(
                GetMinimumInt(left.Rows.Select(row => row.QuantityPerSkid)),
                GetMinimumInt(right.Rows.Select(row => row.QuantityPerSkid))
            ),
            nameof(Model_ReportRow.DisplayLoadsOrSkids) => CompareNullableInt(
                GetLoadsOrSkidsSortValue(left),
                GetLoadsOrSkidsSortValue(right)
            ),
            _ => CompareText(
                GetPreviewCellValue(left, columnKey),
                GetPreviewCellValue(right, columnKey)
            ),
        };
    }

    private string GetPreviewCellValue(PreviewRowSource previewRowSource, string columnKey)
    {
        if (columnKey == nameof(Model_ReportRow.DisplayLoadsOrSkids) && IsCombinationModeSelected())
        {
            return FormatInt(previewRowSource.Rows.Count);
        }

        if (previewRowSource.Rows.Count == 1)
        {
            return previewRowSource.Rows[0].GetColumnValue(columnKey);
        }

        return columnKey switch
        {
            nameof(Model_ReportRow.Id) => string.Empty,
            nameof(Model_ReportRow.PartNumber) => previewRowSource.PartNumber,
            nameof(Model_ReportRow.DisplayPartOrDunnage) => previewRowSource.PartNumber,
            nameof(Model_ReportRow.HeatLotNumber) => previewRowSource.LotNumber,
            nameof(Model_ReportRow.CreatedDate) => previewRowSource.CreatedDate.HasValue
                ? FormatDate(previewRowSource.CreatedDate.Value)
                : GetCommonTextValue(previewRowSource.Rows.Select(row => row.DisplayCreatedDate)),
            nameof(Model_ReportRow.Quantity) or nameof(Model_ReportRow.DisplayQuantity) =>
                FormatDecimal(previewRowSource.Rows.Sum(row => row.Quantity ?? 0m)),
            nameof(Model_ReportRow.WeightLbs) => FormatDecimal(
                previewRowSource.Rows.Sum(row => row.WeightLbs ?? 0m)
            ),
            nameof(Model_ReportRow.QtyOrdered) => FormatDecimal(
                previewRowSource.Rows.Sum(row => row.QtyOrdered ?? 0m)
            ),
            nameof(Model_ReportRow.PartCount) => FormatInt(
                previewRowSource.Rows.Sum(row => row.PartCount ?? 0)
            ),
            nameof(Model_ReportRow.RemainingQuantity) => FormatInt(
                previewRowSource.Rows.Sum(row => row.RemainingQuantity ?? 0)
            ),
            nameof(Model_ReportRow.PartSkidTotal) => FormatInt(
                previewRowSource.Rows.Sum(row => row.PartSkidTotal ?? 0)
            ),
            nameof(Model_ReportRow.ReceivedSkidCount) => FormatInt(
                previewRowSource.Rows.Sum(row => row.ReceivedSkidCount ?? 0)
            ),
            nameof(Model_ReportRow.DisplayLoadsOrSkids) => GetDisplayLoadsOrSkids(previewRowSource),
            nameof(Model_ReportRow.IsNonPOItem) => GetUniformBooleanValue(
                previewRowSource.Rows.Select(row => row.IsNonPOItem)
            ),
            nameof(Model_ReportRow.IsQualityHoldRequired) => GetUniformBooleanValue(
                previewRowSource.Rows.Select(row => row.IsQualityHoldRequired)
            ),
            nameof(Model_ReportRow.IsQualityHoldAcknowledged) => GetUniformBooleanValue(
                previewRowSource.Rows.Select(row => row.IsQualityHoldAcknowledged)
            ),
            nameof(Model_ReportRow.DisplayTransactionDate)
            or nameof(Model_ReportRow.TransactionDate) => GetCommonTextValue(
                previewRowSource.Rows.Select(row => row.DisplayTransactionDate)
            ),
            nameof(Model_ReportRow.DisplayCreatedAt) or nameof(Model_ReportRow.CreatedAt) =>
                GetCommonTextValue(previewRowSource.Rows.Select(row => row.DisplayCreatedAt)),
            nameof(Model_ReportRow.DisplayPoDueDate) or nameof(Model_ReportRow.PoDueDate) =>
                GetCommonTextValue(previewRowSource.Rows.Select(row => row.DisplayPoDueDate)),
            nameof(Model_ReportRow.DisplayUnitsPerSkid) => GetCommonTextValue(
                previewRowSource.Rows.Select(row => row.DisplayUnitsPerSkid)
            ),
            _ => GetCommonTextValue(
                previewRowSource.Rows.Select(row => row.GetColumnValue(columnKey))
            ),
        };
    }

    private string GetDetailSectionTitle()
    {
        return GetEffectiveRowDisplayMode(Rows) switch
        {
            Enum_ReportingPreviewRowDisplayMode.RawRows => "Detailed Activity",
            Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersEntireDateRange =>
                "Combined Activity - Unique Part Numbers (Entire Date Range)",
            Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersAndLotNumbersEntireDateRange =>
                "Combined Activity - Unique Part Numbers And Lot Numbers (Entire Date Range)",
            Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersPerDay =>
                "Combined Activity - Unique Part Numbers Per Day",
            _ => "Combined Activity - Unique Part Numbers And Lot Numbers Per Day",
        };
    }

    private string GetDetailSectionNote()
    {
        var rows = Rows.ToList();
        if (
            RowDisplayMode != Enum_ReportingPreviewRowDisplayMode.RawRows
            && GetEffectiveRowDisplayMode(rows) == Enum_ReportingPreviewRowDisplayMode.RawRows
        )
        {
            return "This module stays in raw-row mode for part-based options because its rows do not expose part-number grouping data.";
        }

        return string.Empty;
    }

    private static PreviewGroupingKey CreateGroupingKey(
        Model_ReportRow row,
        Enum_ReportingPreviewRowDisplayMode rowDisplayMode
    )
    {
        var partNumber = NormalizeGroupValue(row.PartNumber, NoPartNumberLabel);
        var createdDate = rowDisplayMode
            is Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersPerDay
                or Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersAndLotNumbersPerDay
            ? (DateTime?)row.CreatedDate.Date
            : null;
        var lotNumber = rowDisplayMode
            is Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersAndLotNumbersEntireDateRange
                or Enum_ReportingPreviewRowDisplayMode.UniquePartNumbersAndLotNumbersPerDay
            ? NormalizeGroupValue(row.HeatLotNumber, NoLotLabel)
            : string.Empty;

        return new PreviewGroupingKey(createdDate, partNumber, lotNumber);
    }

    private static Enum_ReportingPreviewRowDisplayMode GetEffectiveRowDisplayMode(
        IReadOnlyCollection<Model_ReportRow> rows,
        Enum_ReportingPreviewRowDisplayMode requestedMode =
            Enum_ReportingPreviewRowDisplayMode.RawRows
    )
    {
        if (requestedMode == Enum_ReportingPreviewRowDisplayMode.RawRows)
        {
            return requestedMode;
        }

        return rows.Any(row => !string.IsNullOrWhiteSpace(row.PartNumber))
            ? requestedMode
            : Enum_ReportingPreviewRowDisplayMode.RawRows;
    }

    private Enum_ReportingPreviewRowDisplayMode GetEffectiveRowDisplayMode(
        IReadOnlyCollection<Model_ReportRow> rows
    )
    {
        return GetEffectiveRowDisplayMode(rows, RowDisplayMode);
    }

    private bool IsCombinationModeSelected()
    {
        return RowDisplayMode != Enum_ReportingPreviewRowDisplayMode.RawRows;
    }

    private int? GetLoadsOrSkidsSortValue(PreviewRowSource previewRowSource)
    {
        if (IsCombinationModeSelected())
        {
            return previewRowSource.Rows.Count;
        }

        return int.TryParse(GetDisplayLoadsOrSkids(previewRowSource), out var parsedValue)
            ? parsedValue
            : null;
    }

    private static string GetCommonTextValue(IEnumerable<string?> values)
    {
        var distinctValues = values
            .Select(value => value?.Trim() ?? string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return distinctValues.Count == 1 ? distinctValues[0] : string.Empty;
    }

    private static string GetDisplayLoadsOrSkids(PreviewRowSource previewRowSource)
    {
        var sourceModule = GetCommonTextValue(
            previewRowSource.Rows.Select(row => row.SourceModule)
        );
        if (sourceModule.Equals("Volvo", StringComparison.OrdinalIgnoreCase))
        {
            return FormatInt(previewRowSource.Rows.Sum(row => row.ReceivedSkidCount ?? 0));
        }

        if (sourceModule.Equals("Receiving", StringComparison.OrdinalIgnoreCase))
        {
            var totalCoils = previewRowSource.Rows.Sum(row => row.CoilsOnSkid ?? 0);
            if (totalCoils > 0)
            {
                return FormatInt(totalCoils);
            }

            var totalPackages = previewRowSource.Rows.Sum(row => row.PackagesPerLoad ?? 0);
            return totalPackages > 0 ? FormatInt(totalPackages) : string.Empty;
        }

        return GetCommonTextValue(previewRowSource.Rows.Select(row => row.DisplayLoadsOrSkids));
    }

    private static string GetUniformBooleanValue(IEnumerable<bool> values)
    {
        var distinctValues = values.Distinct().ToList();
        if (distinctValues.Count != 1)
        {
            return string.Empty;
        }

        return distinctValues[0] ? "Yes" : "No";
    }

    private static string NormalizeGroupValue(string? value, string emptyLabel)
    {
        return string.IsNullOrWhiteSpace(value) ? emptyLabel : value.Trim();
    }

    private static int CompareText(string? left, string? right)
    {
        left ??= string.Empty;
        right ??= string.Empty;

        var leftIsEmpty = string.IsNullOrWhiteSpace(left);
        var rightIsEmpty = string.IsNullOrWhiteSpace(right);

        if (leftIsEmpty && rightIsEmpty)
        {
            return 0;
        }

        if (leftIsEmpty)
        {
            return 1;
        }

        if (rightIsEmpty)
        {
            return -1;
        }

        return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
    }

    private static int CompareDecimal(decimal left, decimal right)
    {
        return left.CompareTo(right);
    }

    private static int CompareInt(int left, int right)
    {
        return left.CompareTo(right);
    }

    private static int CompareNullableDecimal(decimal? left, decimal? right)
    {
        if (!left.HasValue && !right.HasValue)
        {
            return 0;
        }

        if (!left.HasValue)
        {
            return 1;
        }

        if (!right.HasValue)
        {
            return -1;
        }

        return left.Value.CompareTo(right.Value);
    }

    private static int CompareNullableInt(int? left, int? right)
    {
        if (!left.HasValue && !right.HasValue)
        {
            return 0;
        }

        if (!left.HasValue)
        {
            return 1;
        }

        if (!right.HasValue)
        {
            return -1;
        }

        return left.Value.CompareTo(right.Value);
    }

    private static int CompareNullableDateTime(DateTime? left, DateTime? right)
    {
        if (!left.HasValue && !right.HasValue)
        {
            return 0;
        }

        if (!left.HasValue)
        {
            return 1;
        }

        if (!right.HasValue)
        {
            return -1;
        }

        return left.Value.CompareTo(right.Value);
    }

    private static decimal? GetMinimumDecimal(IEnumerable<decimal?> values)
    {
        decimal? minimum = null;
        foreach (var value in values)
        {
            if (!value.HasValue)
            {
                continue;
            }

            if (!minimum.HasValue || value.Value < minimum.Value)
            {
                minimum = value.Value;
            }
        }

        return minimum;
    }

    private static int? GetMinimumInt(IEnumerable<int?> values)
    {
        int? minimum = null;
        foreach (var value in values)
        {
            if (!value.HasValue)
            {
                continue;
            }

            if (!minimum.HasValue || value.Value < minimum.Value)
            {
                minimum = value.Value;
            }
        }

        return minimum;
    }

    private static DateTime? GetMinimumDateTime(IEnumerable<DateTime?> values)
    {
        DateTime? minimum = null;
        foreach (var value in values)
        {
            if (!value.HasValue)
            {
                continue;
            }

            if (!minimum.HasValue || value.Value < minimum.Value)
            {
                minimum = value.Value;
            }
        }

        return minimum;
    }

    private static string FormatDate(DateTime value)
    {
        return value.ToString("M/d/yyyy", CultureInfo.InvariantCulture);
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatInt(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private sealed record PreviewRowSource(
        IReadOnlyList<Model_ReportRow> Rows,
        string PartNumber,
        string LotNumber,
        DateTime? CreatedDate,
        int SourceIndex
    )
    {
        public static PreviewRowSource CreateRaw(Model_ReportRow row, int sourceIndex)
        {
            return new PreviewRowSource(
                [row],
                NormalizeGroupValue(row.PartNumber, NoPartNumberLabel),
                NormalizeGroupValue(row.HeatLotNumber, NoLotLabel),
                row.CreatedDate.Date,
                sourceIndex
            );
        }
    }

    private readonly record struct PreviewGroupingKey(
        DateTime? CreatedDate,
        string PartNumber,
        string LotNumber
    );
}
