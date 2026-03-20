using System.Collections.ObjectModel;

namespace MTM_Receiving_Application.Module_Core.Models.Reporting;

public class Model_ReportSummaryTableRow
{
    public ObservableCollection<Model_ReportSummaryTableCell> Cells { get; set; } = [];

    public bool IsGrandTotal { get; set; }
}