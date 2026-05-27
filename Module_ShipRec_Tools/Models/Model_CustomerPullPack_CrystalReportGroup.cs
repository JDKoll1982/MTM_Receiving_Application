using System.Collections.Generic;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Media;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// UI projection for one crystal-style report group in the Customer Pull n' Pack preview surface.
/// </summary>
public sealed class Model_CustomerPullPack_CrystalReportGroup
{
    public string ParentPartId { get; set; } = string.Empty;

    public string ParentPartDisplay => $"P/N:{ParentPartId}";

    public decimal QuantityToPack { get; set; }

    public decimal QuantitySelected { get; set; }

    public string FgLocationId { get; set; } = string.Empty;

    public decimal FgOnHandQuantity { get; set; }

    public bool ShortageFlag { get; set; }

    public bool LateOrderFlag { get; set; }

    public string ServiceNote { get; set; } = string.Empty;

    public IReadOnlyList<Model_CustomerPullPack_CrystalSubPartLocation> SubPartLocations { get; set; } =
    [];

    public IReadOnlyList<Model_CustomerPullPack_CrystalRequestLine> RequestLines { get; set; } = [];

    public bool HasServiceNote => string.IsNullOrWhiteSpace(ServiceNote) is false;

    public bool HasSelectedRequestLine => RequestLines.Any(static line => line.IsSelected);

    public decimal TotalSubPartsOnHand => SubPartLocations.Sum(static item => item.OnHandQuantity);

    public Brush QuantityToPackBackgroundBrush =>
        ShortageFlag ? new SolidColorBrush(Colors.IndianRed)
        : LateOrderFlag ? new SolidColorBrush(ColorHelper.FromArgb(255, 244, 223, 125))
        : new SolidColorBrush(ColorHelper.FromArgb(255, 240, 240, 240));

    public Brush QuantityToPackForegroundBrush =>
        ShortageFlag ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);

    public Brush QuantitySelectedBackgroundBrush =>
        new SolidColorBrush(ColorHelper.FromArgb(255, 238, 228, 200));

    public Brush QuantitySelectedForegroundBrush =>
        new SolidColorBrush(ColorHelper.FromArgb(255, 136, 96, 22));
}

/// <summary>
/// UI projection for one request line inside the crystal-style report group.
/// </summary>
public sealed class Model_CustomerPullPack_CrystalRequestLine
{
    public string SourceLineKey { get; set; } = string.Empty;

    public string CustomerOrderId { get; set; } = string.Empty;

    public string ParentPartId { get; set; } = string.Empty;

    public string LocationId { get; set; } = string.Empty;

    public string CustomerLabel { get; set; } = string.Empty;

    public decimal ShipQuantity { get; set; }

    public string PullDateDisplay { get; set; } = string.Empty;

    public bool IsSelected { get; set; }

    public string StatusNoteText { get; set; } = string.Empty;

    public Brush RowBackgroundBrush =>
        IsSelected
            ? new SolidColorBrush(ColorHelper.FromArgb(32, 47, 88, 128))
            : new SolidColorBrush(Colors.Transparent);

    public bool HasStatusNote => string.IsNullOrWhiteSpace(StatusNoteText) is false;

    public Brush StatusNoteBackgroundBrush =>
        StatusNoteText switch
        {
            "Shortage" => new SolidColorBrush(Colors.IndianRed),
            "Late Order" => new SolidColorBrush(ColorHelper.FromArgb(255, 244, 223, 125)),
            "Waitlist" => new SolidColorBrush(ColorHelper.FromArgb(255, 223, 232, 242)),
            _ => new SolidColorBrush(ColorHelper.FromArgb(255, 240, 240, 240)),
        };

    public Brush StatusNoteForegroundBrush =>
        StatusNoteText switch
        {
            "Shortage" => new SolidColorBrush(Colors.White),
            _ => new SolidColorBrush(ColorHelper.FromArgb(255, 47, 88, 128)),
        };
}

/// <summary>
/// UI projection for one selectable SUB PARTS ON HAND location row.
/// </summary>
public sealed class Model_CustomerPullPack_CrystalSubPartLocation
{
    public string PartId { get; set; } = string.Empty;

    public string LocationId { get; set; } = string.Empty;

    public string PartLocationId { get; set; } = string.Empty;

    public string SeparatorText => "•";

    public decimal OnHandQuantity { get; set; }

    public bool IsSelected { get; set; }

    public Brush RowBackgroundBrush =>
        IsSelected
            ? new SolidColorBrush(ColorHelper.FromArgb(200, 47, 88, 128))
            : new SolidColorBrush(Colors.Transparent);

    public Windows.UI.Text.FontWeight RowFontWeight =>
        IsSelected ? FontWeights.SemiBold : FontWeights.Normal;
}
