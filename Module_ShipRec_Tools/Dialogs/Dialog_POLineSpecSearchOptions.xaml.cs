using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;

/// <summary>
/// Options dialog for PO line spec search filters and visible columns.
/// </summary>
public sealed partial class Dialog_POLineSpecSearchOptions : ContentDialog
{
    private readonly Dictionary<string, CheckBox> _columnCheckboxes;

    public Model_Tool_POLineSpecSearchOptions SelectedOptions { get; private set; }

    public Dialog_POLineSpecSearchOptions(Model_Tool_POLineSpecSearchOptions currentOptions)
    {
        ArgumentNullException.ThrowIfNull(currentOptions);

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);

        Title = "PO Line Spec Search Options";
        SelectedOptions = currentOptions.Clone();

        _columnCheckboxes = new Dictionary<string, CheckBox>(StringComparer.OrdinalIgnoreCase)
        {
            ["PONumber"] = ColumnPONumber,
            ["POLineNumber"] = ColumnPOLineNumber,
            ["PartId"] = ColumnPartId,
            ["VendorName"] = ColumnVendorName,
            ["VendorId"] = ColumnVendorId,
            ["VendorPartId"] = ColumnVendorPartId,
            ["QtyOrdered"] = ColumnQtyOrdered,
            ["TotalQtyReceived"] = ColumnTotalQtyReceived,
            ["PoStatus"] = ColumnPoStatus,
            ["SpecExcerpt"] = ColumnSpecExcerpt,
            ["MatchScore"] = ColumnMatchScore,
        };

        VisibleLinesComboBox.ItemsSource = Model_Tool_POLineSpecSearchOptions.VisibleLinesOptions;
        VisibleLinesComboBox.SelectedItem = Model_Tool_POLineSpecSearchOptions.VisibleLinesOptions
            .Contains(SelectedOptions.VisibleLines)
            ? SelectedOptions.VisibleLines
            : Model_Tool_POLineSpecSearchOptions.DefaultVisibleLines;

        SearchModeComboBox.ItemsSource = Model_Tool_POLineSpecSearchOptions.SearchModeOptions;
        var normalizedSearchMode = string.IsNullOrWhiteSpace(SelectedOptions.SearchMode)
            ? Model_Tool_POLineSpecSearchOptions.DefaultSearchMode
            : SelectedOptions.SearchMode.Trim();
        SearchModeComboBox.SelectedItem =
            Model_Tool_POLineSpecSearchOptions.SearchModeOptions.Contains(
                normalizedSearchMode,
                StringComparer.OrdinalIgnoreCase
            )
                ? Model_Tool_POLineSpecSearchOptions.SearchModeOptions.First(option =>
                    string.Equals(option, normalizedSearchMode, StringComparison.OrdinalIgnoreCase)
                )
                : Model_Tool_POLineSpecSearchOptions.DefaultSearchMode;

        PoStatusFilterComboBox.ItemsSource =
            Model_Tool_POLineSpecSearchOptions.PoStatusFilterOptions;
        var normalizedPoStatus = string.IsNullOrWhiteSpace(SelectedOptions.PoStatusFilter)
            ? "All"
            : SelectedOptions.PoStatusFilter.Trim();
        PoStatusFilterComboBox.SelectedItem =
            Model_Tool_POLineSpecSearchOptions.PoStatusFilterOptions.Contains(
                normalizedPoStatus,
                StringComparer.OrdinalIgnoreCase
            )
                ? Model_Tool_POLineSpecSearchOptions.PoStatusFilterOptions.First(option =>
                    string.Equals(option, normalizedPoStatus, StringComparison.OrdinalIgnoreCase)
                )
                : "All";

        foreach (var entry in _columnCheckboxes)
        {
            entry.Value.IsChecked = SelectedOptions.VisibleColumnKeys.Contains(entry.Key);
        }

        ColumnSpecExcerpt.IsChecked = true;

        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var selectedKeys = _columnCheckboxes
            .Where(static pair => pair.Value.IsChecked == true)
            .Select(static pair => pair.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        selectedKeys.Add("SpecExcerpt");

        if (selectedKeys.Count == 0)
        {
            args.Cancel = true;
            return;
        }

        var visibleLines = VisibleLinesComboBox.SelectedItem as int?;
        var selectedPoStatus = PoStatusFilterComboBox.SelectedItem?.ToString()?.Trim() ?? "All";
        var selectedSearchMode =
            SearchModeComboBox.SelectedItem?.ToString()?.Trim()
            ?? Model_Tool_POLineSpecSearchOptions.DefaultSearchMode;
        SelectedOptions = new Model_Tool_POLineSpecSearchOptions
        {
            SearchMode = selectedSearchMode,
            PoStatusFilter = string.Equals(
                selectedPoStatus,
                "All",
                StringComparison.OrdinalIgnoreCase
            )
                ? string.Empty
                : selectedPoStatus,
            VisibleLines = visibleLines ?? Model_Tool_POLineSpecSearchOptions.DefaultVisibleLines,
            VisibleColumnKeys = selectedKeys,
        };
    }
}
