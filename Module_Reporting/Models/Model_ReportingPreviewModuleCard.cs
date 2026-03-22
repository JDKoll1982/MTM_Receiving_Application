using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Models.Reporting;

namespace MTM_Receiving_Application.Module_Reporting.Models;

public partial class Model_ReportingPreviewModuleCard : ObservableObject
{
    [ObservableProperty]
    private bool _isIncluded = true;

    public string ModuleName { get; set; } = string.Empty;

    public string CardTitle { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public SolidColorBrush AccentBackgroundBrush { get; set; } = new(Colors.DarkSlateBlue);

    public SolidColorBrush AccentForegroundBrush { get; set; } = new(Colors.White);

    public Model_ReportSummaryTable SummaryTable { get; set; } = new();

    public Model_ReportSection DetailSection { get; set; } = new();

    public double DetailTableWidth { get; set; } = 900d;

    public ObservableCollection<Model_ReportingPreviewColumnOption> AvailableColumns { get; } = [];

    public ObservableCollection<Model_ReportingPreviewColumnOption> IncludedColumns { get; } = [];

    public ObservableCollection<Model_ReportingPreviewRow> PreviewRows { get; } = [];

    public ObservableCollection<Model_ReportRow> Rows => DetailSection.Rows;

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    public bool HasIncludedColumns => IncludedColumns.Count > 0;

    public void InitializeColumns(IEnumerable<Model_ReportingPreviewColumnOption> availableColumns)
    {
        DetachColumnHandlers();

        AvailableColumns.Clear();
        foreach (var availableColumn in availableColumns)
        {
            availableColumn.PropertyChanged += OnColumnPropertyChanged;
            AvailableColumns.Add(availableColumn);
        }

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
        IncludedColumns.Clear();
        foreach (var availableColumn in AvailableColumns.Where(column => column.IsIncluded))
        {
            IncludedColumns.Add(availableColumn);
        }

        PreviewRows.Clear();
        foreach (var row in Rows)
        {
            var previewRow = new Model_ReportingPreviewRow();
            foreach (var includedColumn in IncludedColumns)
            {
                previewRow.Cells.Add(
                    new Model_ReportingPreviewCell
                    {
                        Value = row.GetColumnValue(includedColumn.Key),
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
    }

    private void OnColumnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Model_ReportingPreviewColumnOption.IsIncluded))
        {
            RefreshPreviewRows();
        }
    }
}
