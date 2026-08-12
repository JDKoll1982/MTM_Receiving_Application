using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Reporting.Models;

public sealed class Model_ReportingPreviewSettings
{
    public Enum_ReportingPreviewRowDisplayMode RowDisplayMode { get; set; } =
        Enum_ReportingPreviewRowDisplayMode.RawRows;

    public Dictionary<string, Model_ReportingPreviewModuleSettings> ModuleSettings { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}

public sealed class Model_ReportingPreviewModuleSettings
{
    public bool IsIncluded { get; set; } = true;

    public string SelectedSortOptionKey { get; set; } = "__source_order";

    public Enum_ReportingPreviewSortDirection SelectedSortDirection { get; set; } =
        Enum_ReportingPreviewSortDirection.Ascending;

    public List<string> IncludedColumnKeys { get; set; } = new();
}
