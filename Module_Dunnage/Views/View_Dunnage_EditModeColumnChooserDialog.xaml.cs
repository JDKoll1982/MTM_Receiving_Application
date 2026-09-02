using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using Windows.Foundation;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_EditModeColumnChooserDialog : ContentDialog
{
    private static readonly (string Title, string[] Headers)[] GroupDefinitions =
    [
        (
            "Editable load fields",
            [
                "Load #",
                "Type",
                "Part ID",
                "Quantity",
                "PO Number",
                "Location",
                "Home Location",
                "Inventory Method",
                "Part Skid",
            ]
        ),
        (
            "Identifiers and tracking",
            ["Queue ID", "Load UUID", "Type ID", "Type Icon", "Label Number", "Specs JSON"]
        ),
        (
            "Dates and audit",
            [
                "Received Date",
                "Created At",
                "Created Date",
                "Created Time",
                "User ID",
                "Modified By",
                "Modified Date",
            ]
        ),
    ];

    private sealed class ColumnOption
    {
        public required string Header { get; init; }

        public required int Index { get; init; }

        public required bool IsVisible { get; init; }

        public bool IsAlwaysVisible { get; init; }
    }

    private readonly IReadOnlyList<ColumnOption> _columnOptions;
    private readonly Dictionary<int, bool> _initialSelection;
    private readonly Dictionary<int, CheckBox> _checkBoxesByIndex = new();

    public bool WasAccepted { get; private set; }

    public View_Dunnage_EditModeColumnChooserDialog(IReadOnlyList<DataGridColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);

        _columnOptions = columns
            .Select(
                (column, index) =>
                    new ColumnOption
                    {
                        Header = column.Header?.ToString() ?? $"Column {index}",
                        Index = index,
                        IsVisible = column.Visibility == Visibility.Visible,
                        IsAlwaysVisible = index == 0,
                    }
            )
            .Where(option => !string.IsNullOrWhiteSpace(option.Header))
            .ToList();

        _initialSelection = _columnOptions.ToDictionary(
            option => option.Index,
            option => option.IsVisible
        );
        WasAccepted = false;

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
        var alwaysVisible = _columnOptions
            .Where(option => option.IsAlwaysVisible)
            .Select(option => option.Header)
            .ToList();
        return alwaysVisible.Count == 0 ? "None" : string.Join(", ", alwaysVisible);
    }

    private void BuildGroupedColumnOptions()
    {
        var selectableColumns = _columnOptions.Where(option => !option.IsAlwaysVisible).ToList();
        var columnsByHeader = selectableColumns.ToDictionary(
            option => option.Header,
            StringComparer.OrdinalIgnoreCase
        );
        var groupedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (title, headers) in GroupDefinitions)
        {
            var groupColumns = headers
                .Where(columnsByHeader.ContainsKey)
                .Select(header => columnsByHeader[header])
                .ToList();
            if (groupColumns.Count == 0)
            {
                continue;
            }

            foreach (var header in headers)
            {
                groupedHeaders.Add(header);
            }

            GroupsPanel.Children.Add(BuildGroupSection(title, groupColumns));
        }

        var additionalColumns = selectableColumns
            .Where(option => !groupedHeaders.Contains(option.Header))
            .ToList();
        if (additionalColumns.Count > 0)
        {
            GroupsPanel.Children.Add(BuildGroupSection("Additional fields", additionalColumns));
        }
    }

    private FrameworkElement BuildGroupSection(string title, IReadOnlyList<ColumnOption> columns)
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

    private FrameworkElement BuildOptionCell(ColumnOption column)
    {
        var checkBox = new CheckBox
        {
            IsChecked = column.IsVisible,
            Content = new TextBlock
            {
                Text = column.Header,
                TextWrapping = TextWrapping.WrapWholeWords,
                FontWeight = FontWeights.Medium,
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            MinWidth = 0,
            Tag = column.Index,
        };

        _checkBoxesByIndex[column.Index] = checkBox;

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

    public void ApplySelection(IReadOnlyList<DataGridColumn> columns)
    {
        foreach (ColumnOption option in _columnOptions.Where(option => !option.IsAlwaysVisible))
        {
            if (_checkBoxesByIndex.TryGetValue(option.Index, out CheckBox? checkBox))
            {
                columns[option.Index].Visibility =
                    checkBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    private void SetAllCheckBoxes(bool isChecked)
    {
        foreach (CheckBox checkBox in _checkBoxesByIndex.Values)
        {
            checkBox.IsChecked = isChecked;
        }
    }

    private void Dialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        PrepareDialogSize();
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
        foreach (var (index, initialValue) in _initialSelection)
        {
            if (_checkBoxesByIndex.TryGetValue(index, out CheckBox? checkBox))
            {
                checkBox.IsChecked = initialValue;
            }
        }
    }

    private void OnFooterApplyButtonClick(object sender, RoutedEventArgs e)
    {
        WasAccepted = true;
        Hide();
    }

    private void OnFooterCancelButtonClick(object sender, RoutedEventArgs e)
    {
        WasAccepted = false;
        Hide();
    }
}
