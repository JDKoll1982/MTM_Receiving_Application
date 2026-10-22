using System.Text.RegularExpressions;

namespace MtmTools.PackingListScanProbe;

internal enum ScanKind
{
    BillOfLading,
    EmailScreenshot,
    ErpLocationScreenshot,
    MaterialTransactionSheet,
    Unknown,
}

internal sealed class ScanFields
{
    public string? Source { get; set; }
    public string? Page { get; set; }
    public int WordCount { get; set; }
    public string Kind { get; set; } = nameof(ScanKind.Unknown);
    public List<string> Parts { get; set; } = new();
    public List<string> PurchaseOrders { get; set; } = new();
    public List<string> Quantities { get; set; } = new();
    public List<string> Heats { get; set; } = new();
    public List<string> Dates { get; set; } = new();
    public List<string> Warehouses { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Pulls the receiving-relevant signals out of OCR text. Layout-agnostic on purpose:
/// the same token classes appear on supplier BOLs, ERP screenshots and internal emails,
/// and the real service will reconcile them against Infor Visual before trusting them.
/// </summary>
internal static class ScanExtractor
{
    private static readonly Regex PartPattern = new(
        @"\b(?<part>(?:MMC|MMF|MMT|MME|T)\s?[0-9O]{5,8})\b",
        RegexOptions.Compiled);

    private static readonly Regex PoPattern = new(
        @"(?:PO\s*[-#:]?\s*|P\s*[/\\]\s*[O0]\s*#?\s*:?\s*)(?<po>\d{4,7})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex QuantityPattern = new(
        @"\b(?<qty>\d{1,3}(?:,\d{3})+(?:\.\d+)?|\d{4,6}(?:\.\d+)?)\b",
        RegexOptions.Compiled);

    private static readonly Regex HeatPattern = new(@"\b(?<heat>0\d{8})\b", RegexOptions.Compiled);
    private static readonly Regex DatePattern = new(@"\b(?<date>\d{1,2}/\d{1,2}/\d{2,4})\b", RegexOptions.Compiled);
    private static readonly Regex WarehousePattern = new(@"WAREHOUSE\s+ID\s+(?<warehouse>\d{3})", RegexOptions.Compiled);

    public static ScanFields Extract(IReadOnlyList<OcrWord> words)
    {
        var fields = new ScanFields();
        var text = string.Join(' ', words.Select(word => word.Text));
        var upper = text.ToUpperInvariant();

        fields.Kind = DetectKind(upper).ToString();

        fields.Parts = Distinct(PartPattern.Matches(upper).Select(m => NormalizeDigits(m.Groups["part"].Value)));
        fields.PurchaseOrders = Distinct(PoPattern.Matches(upper).Select(m => m.Groups["po"].Value));
        fields.Quantities = Distinct(QuantityPattern.Matches(upper).Select(m => m.Groups["qty"].Value)
            .Where(value => value.Replace(",", string.Empty).Length is >= 4 and <= 7));
        fields.Heats = Distinct(HeatPattern.Matches(upper).Select(m => m.Groups["heat"].Value));
        fields.Dates = Distinct(DatePattern.Matches(upper).Select(m => m.Groups["date"].Value));
        fields.Warehouses = Distinct(WarehousePattern.Matches(upper).Select(m => m.Groups["warehouse"].Value));

        if (words.Count < 20)
        {
            fields.Warnings.Add(
                $"only {words.Count} words recognized - the capture quality is too low to trust");
        }

        if (fields.Parts.Count == 0)
        {
            fields.Warnings.Add("no part number found");
        }

        if (fields.PurchaseOrders.Count == 0)
        {
            fields.Warnings.Add("no purchase order found");
        }

        return fields;
    }

    private static ScanKind DetectKind(string upper)
    {
        if (upper.Contains("BILL OF LADING") || upper.Contains("LADING")) return ScanKind.BillOfLading;
        if (upper.Contains("MATERIAL AVAILABILITY TRANSACTION")) return ScanKind.MaterialTransactionSheet;
        if (upper.Contains("WAREHOUSE LOCATIONS FOR PART")) return ScanKind.ErpLocationScreenshot;
        if (upper.Contains("SUBJECT:") && upper.Contains("SENT:")) return ScanKind.EmailScreenshot;
        return ScanKind.Unknown;
    }

    private static List<string> Distinct(IEnumerable<string> values) =>
        values.Distinct(StringComparer.Ordinal).ToList();

    /// <summary>
    /// OCR reads zero as O inside part numbers ("MMCOOO113O"). This is safe to fold because
    /// the MTM part prefixes are letters and the remainder is always numeric.
    /// </summary>
    private static string NormalizeDigits(string value) =>
        Regex.Replace(value.Replace(" ", string.Empty), "(?<=[A-Z])O(?=[0-9])|(?<=[0-9])O", "0");
}
