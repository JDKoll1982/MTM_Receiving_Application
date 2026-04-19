using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Receiving.Models;
using Windows.Foundation;

namespace MTM_Receiving_Application.Module_Receiving.Dialogs;

public sealed partial class Dialog_Receiving_EditModeColumnChooser : ContentDialog
{
    private static readonly (string Title, string[] Keys)[] GroupDefinitions =
    [
        (
            "Part and receiving details",
            ["ReceivedDate", "PartType", "PartDescription", "UnitOfMeasure", "InitialLocation"]
        ),
        (
            "Purchase order details",
            [
                "PONumber",
                "POLineNumber",
                "QtyOrdered",
                "RemainingQuantity",
                "POVendor",
                "POStatus",
                "PODueDate",
                "IsNonPOItem",
            ]
        ),
        (
            "Packaging and traceability",
            [
                "WeightQuantity",
                "HeatLotNumber",
                "PackagesPerLoad",
                "PackageType",
                "WeightPerPackage",
            ]
        ),
        (
            "Quality hold",
            ["IsQualityHoldRequired", "IsQualityHoldAcknowledged", "QualityHoldRestrictionType"]
        ),
        ("Audit and ownership", ["UserId", "EmployeeNumber"]),
    ];

    private readonly IReadOnlyList<Model_EditModeColumn> _columns;
    private readonly Dictionary<string, bool> _initialSelection;
    private readonly Dictionary<string, CheckBox> _checkBoxesByKey = new(
        StringComparer.OrdinalIgnoreCase
    );

    public bool WasAccepted { get; private set; }

    public Dialog_Receiving_EditModeColumnChooser(IEnumerable<Model_EditModeColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
        WasAccepted = false;

        _columns = columns.ToList();
        _initialSelection = _columns.ToDictionary(column => column.Key, column => column.IsVisible);

        AlwaysVisibleTextBlock.Text = BuildAlwaysVisibleText();
        BuildGroupedColumnOptions();
    }

    public void PrepareDialogSize()
    {
        if (XamlRoot is null)
        {
            return;
        }

        RootGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        var desiredWidth = Math.Ceiling(Math.Max(RootGrid.DesiredSize.Width, 960) + 32);
        var availableWidth = Math.Max(900, XamlRoot.Size.Width - 32);
        var availableHeight = Math.Max(720, XamlRoot.Size.Height - 48);

        Width = Math.Min(desiredWidth, availableWidth);
        MinWidth = Math.Min(900, availableWidth);
        MinHeight = Math.Min(720, availableHeight);
        MaxHeight = availableHeight;
    }

    private string BuildAlwaysVisibleText()
    {
        var alwaysVisibleLabels = _columns
            .Where(column => column.IsAlwaysVisible)
            .Select(column => GetColumnLabel(column.Key))
            .ToList();

        return alwaysVisibleLabels.Count == 0
            ? string.Empty
            : $"Always shown: {string.Join(", ", alwaysVisibleLabels)}.";
    }

    private void BuildGroupedColumnOptions()
    {
        var selectableColumns = _columns.Where(column => !column.IsAlwaysVisible).ToList();
        var columnsByKey = selectableColumns.ToDictionary(column => column.Key);
        var groupedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (title, keys) in GroupDefinitions)
        {
            var groupColumns = keys.Where(columnsByKey.ContainsKey)
                .Select(key => columnsByKey[key])
                .ToList();

            if (groupColumns.Count == 0)
            {
                continue;
            }

            foreach (var key in keys)
            {
                groupedKeys.Add(key);
            }

            GroupsPanel.Children.Add(BuildGroupSection(title, groupColumns));
        }

        var uncategorizedColumns = selectableColumns
            .Where(column => !groupedKeys.Contains(column.Key))
            .ToList();

        if (uncategorizedColumns.Count > 0)
        {
            GroupsPanel.Children.Add(BuildGroupSection("Additional fields", uncategorizedColumns));
        }
    }

    private FrameworkElement BuildGroupSection(
        string title,
        IReadOnlyList<Model_EditModeColumn> columns
    )
    {
        var sectionPanel = new StackPanel { Spacing = 12 };

        var headerPanel = new StackPanel { Spacing = 4 };
        headerPanel.Children.Add(
            new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.SemiBold,
                Style = (Style)Application.Current.Resources["SubtitleTextBlockStyle"],
            }
        );
        headerPanel.Children.Add(
            new TextBlock
            {
                Text = $"{columns.Count} selectable columns",
                Foreground = (Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"],
            }
        );
        sectionPanel.Children.Add(headerPanel);

        var grid = new Grid { ColumnSpacing = 16, RowSpacing = 12 };

        for (int columnIndex = 0; columnIndex < 3; columnIndex++)
        {
            grid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            );
        }

        for (int itemIndex = 0; itemIndex < columns.Count; itemIndex++)
        {
            int rowIndex = itemIndex / 3;
            int columnIndex = itemIndex % 3;

            while (grid.RowDefinitions.Count <= rowIndex)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            var option = BuildOptionCell(columns[itemIndex]);
            Grid.SetRow(option, rowIndex);
            Grid.SetColumn(option, columnIndex);
            grid.Children.Add(option);
        }

        sectionPanel.Children.Add(grid);

        return new Border
        {
            Padding = new Thickness(18),
            Background = (Brush)
                Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
            BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = sectionPanel,
        };
    }

    private FrameworkElement BuildOptionCell(Model_EditModeColumn column)
    {
        var label = GetColumnLabel(column.Key);

        var labelTextBlock = new TextBlock
        {
            Text = label,
            TextWrapping = TextWrapping.WrapWholeWords,
            FontWeight = FontWeights.Medium,
        };

        var checkBox = new CheckBox
        {
            IsChecked = column.IsVisible,
            Content = labelTextBlock,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            MinWidth = 0,
            Tag = column.Key,
        };

        _checkBoxesByKey[column.Key] = checkBox;

        return new Border
        {
            Padding = new Thickness(12),
            MinHeight = 56,
            Background = (Brush)Application.Current.Resources["LayerFillColorDefaultBrush"],
            BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = checkBox,
        };
    }

    private static string GetColumnLabel(string key) =>
        key switch
        {
            "LoadNumber" => "Load Number",
            "ReceivedDate" => "Received Date",
            "PartID" => "Part ID",
            "PartType" => "Part Type",
            "PONumber" => "Purchase Order Number",
            "POLineNumber" => "Purchase Order Line",
            "WeightQuantity" => "Received Weight / Quantity",
            "HeatLotNumber" => "Heat / Lot Number",
            "InitialLocation" => "Initial Location",
            "RemainingQuantity" => "Remaining Quantity",
            "PackagesPerLoad" => "Packages per Load",
            "PackageType" => "Package Type",
            "WeightPerPackage" => "Weight per Package",
            "IsNonPOItem" => "Non-PO Item",
            "UserId" => "Created By User",
            "EmployeeNumber" => "Created By Employee Number",
            "IsQualityHoldRequired" => "Quality Hold Required",
            "IsQualityHoldAcknowledged" => "Quality Hold Acknowledged",
            "QualityHoldRestrictionType" => "Quality Hold Restriction",
            "PartDescription" => "Part Description",
            "UnitOfMeasure" => "Unit of Measure",
            "QtyOrdered" => "Quantity Ordered",
            "POVendor" => "Vendor",
            "POStatus" => "Purchase Order Status",
            "PODueDate" => "Purchase Order Due Date",
            _ => key,
        };

    private void ApplySelection()
    {
        foreach (Model_EditModeColumn column in _columns.Where(column => !column.IsAlwaysVisible))
        {
            if (_checkBoxesByKey.TryGetValue(column.Key, out CheckBox? checkBox))
            {
                column.IsVisible = checkBox.IsChecked == true;
            }
        }
    }

    private void SetAllCheckBoxes(bool isChecked)
    {
        foreach (var checkBox in _checkBoxesByKey.Values)
        {
            checkBox.IsChecked = isChecked;
        }
    }

    private void Dialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        PrepareDialogSize();
    }

    private void OnFooterApplyButtonClick(object sender, RoutedEventArgs e)
    {
        ApplySelection();
        WasAccepted = true;
        Hide();
    }

    private void OnFooterCancelButtonClick(object sender, RoutedEventArgs e)
    {
        WasAccepted = false;
        Hide();
    }

    private void SelectAllButton_Click(object sender, RoutedEventArgs e)
    {
        SetAllCheckBoxes(true);
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
        SetAllCheckBoxes(false);
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var (key, initialValue) in _initialSelection)
        {
            if (_checkBoxesByKey.TryGetValue(key, out var checkBox))
            {
                checkBox.IsChecked = initialValue;
            }
        }
    }
}
