using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Helpers;
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
    private const int DefaultDisplayLimit = 50;

    private readonly IReadOnlyList<Model_FuzzySearchResult> _allItems;
    private readonly Func<string, Task<IReadOnlyList<Model_FuzzySearchResult>>>? _fallbackSearchAsync;
    private readonly ObservableCollection<Model_FuzzySearchResult> _filtered = new();
    private int _filterOperationId;

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
        string? subtitle = null,
        Func<string, Task<IReadOnlyList<Model_FuzzySearchResult>>>? fallbackSearchAsync = null
    )
    {
        ArgumentNullException.ThrowIfNull(items);
        _allItems = items;
        _fallbackSearchAsync = fallbackSearchAsync;

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);

        Title = pickerTitle;

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            SubtitleText.Text = subtitle;
            SubtitleText.Visibility = Visibility.Visible;
        }

        _ = ApplyFilterAsync(string.Empty);
    }

    // ─── Filter Logic ───────────────────────────────────────────────────────

    private async Task ApplyFilterAsync(string term)
    {
        var operationId = ++_filterOperationId;
        _filtered.Clear();

        var trimmedTerm = term.Trim();
        var isFilterActive = string.IsNullOrEmpty(trimmedTerm) is false;

        var localMatches = isFilterActive
            ? _allItems.Where(i =>
                i.Label.Contains(trimmedTerm, StringComparison.OrdinalIgnoreCase)
                || (i.Detail?.Contains(trimmedTerm, StringComparison.OrdinalIgnoreCase) ?? false)
            )
            : _allItems.Take(DefaultDisplayLimit);

        foreach (var item in localMatches)
        {
            _filtered.Add(item);
        }

        ResultsListView.ItemsSource = _filtered;

        if (isFilterActive && _filtered.Count == 0 && _fallbackSearchAsync is not null)
        {
            ResultCountText.Text = "Searching…";

            IReadOnlyList<Model_FuzzySearchResult> fallbackMatches;
            try
            {
                fallbackMatches =
                    await _fallbackSearchAsync(trimmedTerm)
                    ?? Array.Empty<Model_FuzzySearchResult>();
            }
            catch
            {
                fallbackMatches = Array.Empty<Model_FuzzySearchResult>();
            }

            if (operationId != _filterOperationId)
            {
                return;
            }

            _filtered.Clear();
            foreach (var item in fallbackMatches)
            {
                _filtered.Add(item);
            }

            ResultsListView.ItemsSource = _filtered;
            UpdateResultCount(true);
        }
        else
        {
            UpdateResultCount(isFilterActive);
        }

        SelectedResult = null;
        IsPrimaryButtonEnabled = false;
    }

    private void UpdateResultCount(bool isFilterActive)
    {
        if (!isFilterActive && _allItems.Count > DefaultDisplayLimit)
        {
            ResultCountText.Text =
                $"Showing first {DefaultDisplayLimit} of {_allItems.Count} matches — type to narrow results";
            return;
        }

        ResultCountText.Text = _filtered.Count switch
        {
            0 => "No matches — try a shorter term",
            1 => "1 match",
            _ => $"{_filtered.Count} matches",
        };
    }

    // ─── Event Handlers ─────────────────────────────────────────────────────

    private async void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        await ApplyFilterAsync(FilterBox.Text);
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
