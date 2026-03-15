using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

/// <summary>
/// Dialog shown when the user tries to advance past Details Entry without a PO number.
/// Lets them enter a free-form reference reason and optionally save it for later reuse.
/// </summary>
public sealed partial class View_Dunnage_Dialog_NonPOEntry : ContentDialog
{
    /// <summary>
    /// The reference string chosen by the user.
    /// Null when the user cancelled without selecting anything.
    /// </summary>
    public string? Result { get; private set; }

    public ObservableCollection<Model_DunnageNonPOEntry> SavedEntries { get; } = new();

    private readonly IService_MySQL_Dunnage _dunnageService;

    public View_Dunnage_Dialog_NonPOEntry()
    {
        InitializeComponent();
        _dunnageService = App.GetService<IService_MySQL_Dunnage>();
        _ = LoadSavedEntriesAsync();
    }

    private async Task LoadSavedEntriesAsync()
    {
        var result = await _dunnageService.GetNonPOEntriesAsync();
        if (result.IsSuccess && result.Data != null)
        {
            foreach (var entry in result.Data)
            {
                SavedEntries.Add(entry);
            }

            SavedEntriesPanel.Visibility = SavedEntries.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    private void OnReferenceTextChanged(object sender, TextChangedEventArgs e)
    {
        IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(ReferenceTextBox.Text);
    }

    private void OnSavedEntrySelected(object sender, SelectionChangedEventArgs e)
    {
        if (SavedEntriesList.SelectedItem is Model_DunnageNonPOEntry entry)
        {
            ReferenceTextBox.Text = entry.Value;
        }
    }

    private async void OnDeleteEntryClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Model_DunnageNonPOEntry entry)
        {
            var deleteResult = await _dunnageService.DeleteNonPOEntryAsync(entry.Id);
            if (deleteResult.IsSuccess)
            {
                SavedEntries.Remove(entry);

                SavedEntriesPanel.Visibility = SavedEntries.Count > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }

    private async void OnConfirmClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var reference = ReferenceTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(reference))
        {
            args.Cancel = true;
            return;
        }

        Result = reference;

        if (SaveForNextTimeCheckBox.IsChecked == true)
        {
            var currentUser = App.GetService<MTM_Receiving_Application.Module_Core.Contracts.Services.IService_UserSessionManager>()
                .CurrentSession?.User?.WindowsUsername ?? "System";

            await _dunnageService.SaveNonPOEntryAsync(reference, currentUser);
        }
    }
}
