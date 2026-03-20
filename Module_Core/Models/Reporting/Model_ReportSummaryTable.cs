using System.Collections.ObjectModel;

namespace MTM_Receiving_Application.Module_Core.Models.Reporting;

public class Model_ReportSummaryTable
{
    public string ModuleName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public ObservableCollection<Model_ReportSummaryColumn> Columns { get; set; } = [];

    public ObservableCollection<Model_ReportSummaryTableRow> Rows { get; set; } = [];

    public double TableWidth { get; set; }
}
