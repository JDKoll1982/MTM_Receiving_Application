using System.Collections.ObjectModel;

namespace MTM_Receiving_Application.Module_Reporting.Models;

public class Model_ReportingPreviewRow
{
    public ObservableCollection<Model_ReportingPreviewCell> Cells { get; set; } = [];
}