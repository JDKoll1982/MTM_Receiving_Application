using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_OutsideService.Models;
using MTM_Receiving_Application.Module_OutsideService.ViewModels;
using AppInfoBarSeverity = MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity;

namespace MTM_Receiving_Application.Module_OutsideService.Views;

/// <summary>
/// Request-entry page for the Outside Service module.
/// </summary>
public sealed partial class View_OutsideService_RequestEntry : Page
{
    public ViewModel_OutsideService_RequestEntry ViewModel { get; }

    public View_OutsideService_RequestEntry()
    {
        ViewModel = App.GetService<ViewModel_OutsideService_RequestEntry>();
        InitializeComponent();
        ViewModel.AddLineRequested += OnAddLineRequested;
    }

    private async void OnAddLineRequested()
    {
        var dialog = new View_OutsideService_AddLineModal
        {
            XamlRoot = XamlRoot,
            ValidatePartAsync = ViewModel.ValidatePartAsync,
            ResolvePartMatchAsync = ResolvePartMatchAsync,
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && dialog.CreatedLine is not null)
        {
            ViewModel.AddDraftLine(dialog.CreatedLine);
        }
    }

    private async Task<Model_OutsideServicePartMatchSuggestion?> ResolvePartMatchAsync(
        string typedValue
    )
    {
        var suggestions = await ViewModel.GetPartSuggestionsAsync(typedValue);
        if (suggestions.Length == 0)
        {
            ViewModel.ShowStatus(
                $"No similar parts found for '{typedValue}'.",
                AppInfoBarSeverity.Warning
            );
            return null;
        }

        var dialog = new View_OutsideService_PartMatchHelper(typedValue, suggestions)
        {
            XamlRoot = XamlRoot,
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? dialog.SelectedSuggestion : null;
    }
}
