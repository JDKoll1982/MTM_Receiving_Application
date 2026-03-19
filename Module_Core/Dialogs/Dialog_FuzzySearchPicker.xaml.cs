using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Module_Core.Dialogs;

/// <summary>
/// Reusable fuzzy-search selection dialog.
/// Pre-load a set of <see cref="Model_FuzzySearchResult"/> candidates (returned from a
/// LIKE-based SQL query), then await <see cref="ShowAsync"/> to get the user's choice.
/// <para>
/// Usage:
/// <code>
/// var dialog = new Dialog_FuzzySearchPicker(matches, "Select Part", "Matching parts for '21-288':") { XamlRoot = this.XamlRoot };
/// var outcome = await dialog.ShowAsync();
/// if (outcome == ContentDialogResult.Primary)
///     DoSomethingWith(dialog.SelectedResult!);
/// </code>
/// </para>
/// </summary>
public sealed partial class Dialog_FuzzySearchPicker : ContentDialog
{
    private readonly IReadOnlyList<Model_FuzzySearchResult> _allItems;
    private readonly ObservableCollection<Model_FuzzySearchResult> _filtered = new();

    /// <summary>The item selected by the user. Non-null when result is <see cref="ContentDialogResult.Primary"/>.</summary>
    public Model_FuzzySearchResult? SelectedResult { get; private set; }

    /// <summary>
    /// Initializes the picker with a pre-loaded list of fuzzy matches.
    /// </summary>
    /// <param name="items">Candidates from the fuzzy SQL query.</param>
    /// <param name="pickerTitle">Dialog window title.</param>
    /// <param name="subtitle">Optional context sentence shown below the title.</param>
    public Dialog_FuzzySearchPicker(
        IReadOnlyList<Model_FuzzySearchResult> items,
        string pickerTitle,
        string? subtitle = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        _allItems = items;

        InitializeComponent();

        Title = pickerTitle;

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            SubtitleText.Text = subtitle;
            SubtitleText.Visibility = Visibility.Visible;
        }

        ApplyFilter(string.Empty);
    }

    // ─── Filter Logic ───────────────────────────────────────────────────────

    private void ApplyFilter(string term)
    {
        _filtered.Clear();

        var lower = term.Trim();
        IEnumerable<Model_FuzzySearchResult> matches = string.IsNullOrEmpty(lower)
            ? _allItems
            : _allItems.Where(i =>
                i.Label.Contains(lower, StringComparison.OrdinalIgnoreCase) ||
                (i.Detail?.Contains(lower, StringComparison.OrdinalIgnoreCase) ?? false));

        foreach (var item in matches)
        {
            _filtered.Add(item);
        }

        ResultsListView.ItemsSource = _filtered;
        UpdateResultCount();

        SelectedResult = null;
        IsPrimaryButtonEnabled = false;
    }

    private void UpdateResultCount()
    {
        ResultCountText.Text = _filtered.Count switch
        {
            0 => "No matches — try a shorter term",
            1 => "1 match",
            _ => $"{_filtered.Count} matches"
        };
    }

    // ─── Event Handlers ─────────────────────────────────────────────────────

    private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter(FilterBox.Text);
    }

    private void ResultsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedResult = ResultsListView.SelectedItem as Model_FuzzySearchResult;
        IsPrimaryButtonEnabled = SelectedResult != null;
    }

    /// <summary>Double-tapping a row is equivalent to selecting it and clicking "Select".</summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void ResultsListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (SelectedResult != null)
        {
            // Trigger the primary button click programmatically by hiding with accepted state.
            // ContentDialog doesn't expose "click primary button", so we accept manually.
            _acceptedViaDoubleTap = true;
            Hide();
        }
    }

    // ContentDialog does not have a built-in "accept" signal that returns Primary when you
    // call Hide(), so we track the double-tap state ourselves and swap the result after close.
    private bool _acceptedViaDoubleTap;

    /// <summary>
    /// Shows the dialog and returns the result. When the user double-taps an item the result
    /// is coerced to <see cref="ContentDialogResult.Primary"/> even though Hide() was called.
    /// </summary>
    public new async System.Threading.Tasks.Task<ContentDialogResult> ShowAsync()
    {
        _acceptedViaDoubleTap = false;
        var result = await base.ShowAsync();

        if (_acceptedViaDoubleTap && SelectedResult != null)
        {
            return ContentDialogResult.Primary;
        }

        return result;
    }
}
