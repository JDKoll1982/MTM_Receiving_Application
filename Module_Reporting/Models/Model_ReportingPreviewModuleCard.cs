using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
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

    public ObservableCollection<Model_ReportRow> Rows => DetailSection.Rows;

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
}