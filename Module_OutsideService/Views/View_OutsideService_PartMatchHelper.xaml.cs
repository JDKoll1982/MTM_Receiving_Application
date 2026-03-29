using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_OutsideService.Models;

namespace MTM_Receiving_Application.Module_OutsideService.Views;

/// <summary>
/// Part Match Helper dialog for user-friendly part correction.
/// </summary>
public sealed partial class View_OutsideService_PartMatchHelper : ContentDialog
{
    public View_OutsideService_PartMatchHelper(
        string typedValue,
        IReadOnlyList<Model_OutsideServicePartMatchSuggestion> suggestions
    )
    {
        TypedValue = typedValue;
        Suggestions = suggestions ?? throw new ArgumentNullException(nameof(suggestions));
        InitializeComponent();

        TypedValueTextBlock.Text = TypedValue;
        SuggestionsList.ItemsSource = Suggestions;

        if (Suggestions.Count > 0)
        {
            SuggestionsList.SelectedIndex = 0;
        }
    }

    public string TypedValue { get; }

    public IReadOnlyList<Model_OutsideServicePartMatchSuggestion> Suggestions { get; }

    public Model_OutsideServicePartMatchSuggestion? SelectedSuggestion { get; private set; }

    private void SuggestionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedSuggestion =
            SuggestionsList.SelectedItem as Model_OutsideServicePartMatchSuggestion;
        IsPrimaryButtonEnabled = SelectedSuggestion is not null;
        SelectedSummaryText.Text = SelectedSuggestion is null
            ? "Select a suggested part to return it to the Add Line modal."
            : $"{SelectedSuggestion.PartId} — {SelectedSuggestion.Description}";
        SelectedReasonText.Text = SelectedSuggestion?.MatchReason ?? string.Empty;
    }
}
