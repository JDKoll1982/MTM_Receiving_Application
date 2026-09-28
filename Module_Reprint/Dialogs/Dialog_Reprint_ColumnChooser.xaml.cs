using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Reprint.Models;
using Windows.Foundation;

namespace MTM_Receiving_Application.Module_Reprint.Dialogs;

/// <summary>
/// Column visibility chooser for the Reprint history grid, modeled on the Receiving Edit Mode
/// column chooser. Apply mutates the column options; Cancel leaves them unchanged.
/// </summary>
public sealed partial class Dialog_Reprint_ColumnChooser : ContentDialog
{
    private readonly IReadOnlyList<Model_ReprintColumnOption> _columns;
    private readonly Dictionary<string, bool> _initialSelection;
    private readonly Dictionary<string, CheckBox> _checkBoxesByKey =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>True when the user accepted with Apply.</summary>
    public bool WasAccepted { get; private set; }

    public Dialog_Reprint_ColumnChooser(IEnumerable<Model_ReprintColumnOption> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
        WasAccepted = false;

        _columns = columns.ToList();
        _initialSelection = _columns.ToDictionary(column => column.Key, column => column.IsVisible);
        BuildColumnOptions();
    }

    
    public void PrepareDialogSize()
    {
        if (XamlRoot is null)
        {
            return;
        }

        RootGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        var desiredWidth = Math.Ceiling(Math.Max(RootGrid.DesiredSize.Width, 760) + 32);
        var availableWidth = Math.Max(720, XamlRoot.Size.Width - 32);
        var availableHeight = Math.Max(560, XamlRoot.Size.Height - 48);

        Width = Math.Min(desiredWidth, availableWidth);
        MinWidth = Math.Min(720, availableWidth);
        MinHeight = Math.Min(560, availableHeight);
        MaxHeight = availableHeight;
    }

    private void BuildColumnOptions()
    {
        var selectable = _columns.Where(column => !column.IsAlwaysVisible).ToList();
        if (selectable.Count == 0)
        {
            return;
        }

        var grid = new Grid { ColumnSpacing = 16, RowSpacing = 12 };
        for (int columnIndex = 0; columnIndex < 3; columnIndex++)
        {
            grid.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            );
        }

        for (int itemIndex = 0; itemIndex < selectable.Count; itemIndex++)
        {
            int rowIndex = itemIndex / 3;
            int columnIndex = itemIndex % 3;

            while (grid.RowDefinitions.Count <= rowIndex)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            var cell = BuildOptionCell(selectable[itemIndex]);
            Grid.SetRow(cell, rowIndex);
            Grid.SetColumn(cell, columnIndex);
            grid.Children.Add(cell);
        }

        ColumnsPanel.Children.Add(
            new Border
            {
                Padding = new Thickness(18),
                Background = (Brush)
                    Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
                BorderBrush = (Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Child = grid,
            }
        );
    }

    private FrameworkElement BuildOptionCell(Model_ReprintColumnOption column)
    {
        var checkBox = new CheckBox
        {
            IsChecked = column.IsVisible,
            Content = column.Header,
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

    private void ApplySelection()
    {
        foreach (var column in _columns.Where(column => !column.IsAlwaysVisible))
        {
            if (_checkBoxesByKey.TryGetValue(column.Key, out var checkBox))
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
