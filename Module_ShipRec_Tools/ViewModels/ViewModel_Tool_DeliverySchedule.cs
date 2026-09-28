using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the Delivery Schedule tool: the receiving-schedule grid with
/// date-range, search, scope, delivery-state, and PO-state filters. Data is read
/// from Infor Visual (read-only).
/// </summary>
public partial class ViewModel_Tool_DeliverySchedule : ViewModel_Tool_Base
{
    private readonly IService_Tool_DeliverySchedule _service;

    /// <summary>
    /// Set by the view code-behind to open a generated HTML report in the browser.
    /// </summary>
    public Func<Model_FormattedReportDocument, Task<Model_Dao_Result<bool>>>? RequestExportAsync
    {
        get;
        set;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDateRange))]
    private DateTimeOffset? _fromDate = DateTimeOffset.Now.Date.AddDays(-7);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDateRange))]
    private DateTimeOffset? _toDate = DateTimeOffset.Now.Date;

    [ObservableProperty]
    private string _partSearch = string.Empty;

    [ObservableProperty]
    private string _poSearch = string.Empty;

    [ObservableProperty]
    private string _supplierSearch = string.Empty;

    [ObservableProperty]
    private string _carrierSearch = string.Empty;

    /// <summary>
    /// Single quick-search term that searches PO, part, supplier, and carrier at
    /// once (mapped to the four per-field search terms + SearchAll in the filter).
    /// </summary>
    [ObservableProperty]
    private string _quickSearch = string.Empty;

    [ObservableProperty]
    private bool _searchAll;

    [ObservableProperty]
    private bool _scopeParts = true;

    [ObservableProperty]
    private bool _scopeCoils = true;

    [ObservableProperty]
    private bool _scopeFlat = true;

    [ObservableProperty]
    private bool _scopeOutside = true;

    [ObservableProperty]
    private bool _scopeUninventoried = true;

    /// <summary>True = only show PARTIAL lines whose received/order percentage is BELOW the threshold.</summary>
    [ObservableProperty]
    private bool _showNearFilled;

    /// <summary>Percentage threshold text (1-99) for the near-filled filter. Default "90".</summary>
    [ObservableProperty]
    private string _nearFillPctText = "90";

    /// <summary>Parsed near-filled threshold, clamped to 1-99 (falls back to 90 on invalid input).</summary>
    public int NearFillPctValue =>
        int.TryParse(NearFillPctText, out var pct) ? Math.Clamp(pct, 1, 99) : 90;

    [ObservableProperty]
    private bool _showOpen = true;

    [ObservableProperty]
    private bool _showClosed = true;

    [ObservableProperty]
    private bool _showOnTime = true;

    [ObservableProperty]
    private bool _showLate = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    [NotifyPropertyChangedFor(nameof(EmptyStateText))]
    [NotifyPropertyChangedFor(nameof(ResultCountText))]
    private ObservableCollection<Model_Tool_DeliveryScheduleLine> _rows = [];

    /// <summary>True when a date range is present (used by the View to enable the date controls).</summary>
    public bool HasDateRange => FromDate is not null && ToDate is not null;

    /// <summary>True when no rows are displayed.</summary>
    public bool IsEmpty => Rows.Count == 0;

    public string EmptyStateText => "No receiving lines match your filters.";

    /// <summary>Result-count line shown above the grid (e.g., "Found 416 receiving line(s).").</summary>
    public string ResultCountText => $"Found {Rows.Count} receiving line{(Rows.Count == 1 ? string.Empty : "s")}.";

    public ViewModel_Tool_DeliverySchedule(
        IService_Tool_DeliverySchedule service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        ToolTitle = "Delivery Schedule";
        ToolDescription = "Review the receiving schedule by PO line with date, scope, and state filters.";
    }

    public void ActivateView()
    {
        _ = SearchAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            IsBusy = true;
            ShowStatus("Loading delivery schedule...", InfoBarSeverity.Informational);

            var filter = BuildFilter();
            var result = await _service.SearchAsync(filter);
            if (!result.IsSuccess || result.Data is null)
            {
                ReplaceRows(Array.Empty<Model_Tool_DeliveryScheduleLine>());
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            ReplaceRows(result.Data);
            ShowStatus(
                result.Data.Count > 0
                    ? $"Found {result.Data.Count} receiving line(s)."
                    : "No receiving lines match your filters.",
                result.Data.Count > 0 ? InfoBarSeverity.Success : InfoBarSeverity.Warning
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SearchAsync),
                nameof(ViewModel_Tool_DeliverySchedule)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        PartSearch = string.Empty;
        PoSearch = string.Empty;
        SupplierSearch = string.Empty;
        CarrierSearch = string.Empty;
        QuickSearch = string.Empty;
        SearchAll = false;
        ScopeParts = true;
        ScopeCoils = true;
        ScopeFlat = true;
        ScopeOutside = true;
        ScopeUninventoried = true;
        ShowNearFilled = false;
        NearFillPctText = "90";
        ShowOpen = true;
        ShowClosed = true;
        ShowOnTime = true;
        ShowLate = true;
        ReplaceRows(Array.Empty<Model_Tool_DeliveryScheduleLine>());
        ShowStatus("Filters cleared.", InfoBarSeverity.Informational);
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            if (Rows.Count == 0)
            {
                ShowStatus("Nothing to export yet. Run a search first.", InfoBarSeverity.Warning);
                return;
            }

            if (RequestExportAsync is null)
            {
                ShowStatus("Export is unavailable in this context.", InfoBarSeverity.Warning);
                return;
            }

            var document = BuildExportDocument();
            var result = await RequestExportAsync(document);
            if (!result.IsSuccess || !result.Data)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            ShowStatus("Opened the Delivery Schedule HTML export in your browser.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ExportAsync),
                nameof(ViewModel_Tool_DeliverySchedule)
            );
        }
    }

    private Model_FormattedReportDocument BuildExportDocument()
    {
        var rangeLabel =
            FromDate is not null && ToDate is not null
                ? $"{FromDate.Value.Date:MM/dd/yyyy} - {ToDate.Value.Date:MM/dd/yyyy}"
                : "All dates";

        var html = new StringBuilder();
        html.AppendLine("<h1>Delivery Schedule</h1>");
        html.AppendLine($"<p><strong>Date Range:</strong> {WebUtility.HtmlEncode(rangeLabel)}</p>");
        html.AppendLine($"<p><strong>Rows:</strong> {Rows.Count}</p>");
        html.AppendLine(
            """
            <style>
                table { border-collapse: collapse; width: 100%; font-family: Segoe UI, sans-serif; font-size: 12px; }
                th, td { border: 1px solid #ccc; padding: 4px 8px; text-align: left; }
                th { background: #e0e0e0; }
                .num { text-align: right; }
            </style>
            """
        );
        html.AppendLine("<table>");
        html.AppendLine("<thead><tr>");
        html.AppendLine(
            "<th>PO Number</th><th>Vendor</th><th>PO Desired</th><th>PO Promise</th>"
                + "<th>Part Number</th><th class='num'>Order Qty</th><th class='num'>Received Qty</th>"
                + "<th class='num'>Remaining Qty</th><th>Line Desired</th><th>Line Promise</th>"
                + "<th>PO Status</th><th>Line Status</th><th>Received By</th>"
        );
        html.AppendLine("</tr></thead>");
        html.AppendLine("<tbody>");

        foreach (var row in Rows)
        {
            html.AppendLine("<tr>");
            html.AppendLine($"<td>{WebUtility.HtmlEncode(row.PoNumber)}</td>");
            html.AppendLine($"<td>{WebUtility.HtmlEncode(row.VendorName)}</td>");
            html.AppendLine($"<td>{row.PoDesiredDateText}</td>");
            html.AppendLine($"<td>{row.PoPromiseDateText}</td>");
            html.AppendLine($"<td>{WebUtility.HtmlEncode(row.PartNumber)}</td>");
            html.AppendLine($"<td class='num'>{row.OrderQtyText}</td>");
            html.AppendLine($"<td class='num'>{row.ReceivedQtyText}</td>");
            html.AppendLine($"<td class='num'>{row.RemainingQtyText}</td>");
            html.AppendLine($"<td>{row.LineDesiredDateText}</td>");
            html.AppendLine($"<td>{row.LinePromiseDateText}</td>");
            html.AppendLine($"<td>{WebUtility.HtmlEncode(row.PoStatus)}</td>");
            html.AppendLine($"<td>{WebUtility.HtmlEncode(row.LineStatus)}</td>");
            html.AppendLine($"<td>{WebUtility.HtmlEncode(row.ReceivedBy)}</td>");
            html.AppendLine("</tr>");
        }

        html.AppendLine("</tbody></table>");

        return new Model_FormattedReportDocument
        {
            DocumentTitle = "Delivery Schedule",
            HtmlFragment = html.ToString(),
            PageCss =
                "@page { margin: 0.4in; } body { font-family: Segoe UI, sans-serif; }",
        };
    }

    private Model_InforVisualDeliveryScheduleFilter BuildFilter()
    {
        // The quick-search box maps to all four per-field search terms OR'd together.
        var quick = QuickSearch?.Trim() ?? string.Empty;
        var part = quick.Length > 0 ? quick : (PartSearch ?? string.Empty).Trim();
        var po = quick.Length > 0 ? quick : (PoSearch ?? string.Empty).Trim();
        var supplier = quick.Length > 0 ? quick : (SupplierSearch ?? string.Empty).Trim();
        var carrier = quick.Length > 0 ? quick : (CarrierSearch ?? string.Empty).Trim();
        var searchAll = quick.Length > 0 ? true : SearchAll;

        return new Model_InforVisualDeliveryScheduleFilter
        {
            FromDate = FromDate?.Date,
            ToDate = ToDate?.Date,
            PartSearch = part,
            PoSearch = po,
            SupplierSearch = supplier,
            CarrierSearch = carrier,
            SearchAll = searchAll,
            ScopeParts = ScopeParts,
            ScopeCoils = ScopeCoils,
            ScopeFlat = ScopeFlat,
            ScopeOutside = ScopeOutside,
            ScopeUninventoried = ScopeUninventoried,
            ShowNearFilled = ShowNearFilled,
            NearFillPct = NearFillPctValue,
            ShowOpen = ShowOpen,
            ShowClosed = ShowClosed,
            ShowOnTime = ShowOnTime,
            ShowLate = ShowLate,
            Today = DateTime.Today,
            MaxResults = 5000,
        };
    }

    private void ReplaceRows(
        System.Collections.Generic.IEnumerable<Model_Tool_DeliveryScheduleLine> rows
    )
    {
        Rows = new ObservableCollection<Model_Tool_DeliveryScheduleLine>(rows);
    }
}
