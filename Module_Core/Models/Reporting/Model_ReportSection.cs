using System.Collections.ObjectModel;

namespace MTM_Receiving_Application.Module_Core.Models.Reporting;

public class Model_ReportSection
{
    public string ModuleName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ObservableCollection<Model_ReportRow> Rows { get; set; } = [];
}