using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Views;

/// <summary>
/// Dialog shown before entering Review when guided Receiving is in Non-PO mode and no reference has been chosen yet.
/// </summary>
public sealed partial class View_Receiving_Dialog_NonPOEntry : ContentDialog
{
    public string? Result { get; private set; }

    public ObservableCollection<Model_ReceivingNonPOEntry> SavedEntries { get; } = new();

    private readonly IService_MySQL_Receiving _receivingService;
    private readonly string? _partId;
    private string? _savedPartDefaultValue;

    public View_Receiving_Dialog_NonPOEntry(string? partId = null)
    {
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
        _receivingService = App.GetService<IService_MySQL_Receiving>();
        _partId = string.IsNullOrWhiteSpace(partId) ? null : partId.Trim();
        _ = LoadSavedEntriesAsync();
    }

    private async Task LoadSavedEntriesAsync()
    {
        var result = await _receivingService.GetNonPOEntriesAsync();
        if (result.IsSuccess && result.Data != null)
        {
            foreach (var entry in result.Data)
            {
                SavedEntries.Add(entry);
            }

            SavedEntriesPanel.Visibility =
                SavedEntries.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        if (string.IsNullOrWhiteSpace(_partId))
        {
            return;
        }

        var partDefaultResult = await _receivingService.GetNonPOPartDefaultAsync(_partId);
        if (
            partDefaultResult.IsSuccess
            && string.IsNullOrWhiteSpace(partDefaultResult.Data) is false
        )
        {
            _savedPartDefaultValue = partDefaultResult.Data.Trim();
            ReferenceTextBox.Text = _savedPartDefaultValue;

            var matchingEntry = SavedEntries.FirstOrDefault(entry =>
                string.Equals(
                    entry.Value,
                    _savedPartDefaultValue,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (matchingEntry != null)
            {
                SavedEntriesList.SelectedItem = matchingEntry;
            }
        }
    }

    private void OnReferenceTextChanged(object sender, TextChangedEventArgs e)
    {
        IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(ReferenceTextBox.Text);
    }

    private void OnSavedEntrySelected(object sender, SelectionChangedEventArgs e)
    {
        if (SavedEntriesList.SelectedItem is Model_ReceivingNonPOEntry entry)
        {
            ReferenceTextBox.Text = entry.Value;
        }
    }

    private async void OnDeleteEntryClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Model_ReceivingNonPOEntry entry)
        {
            var deleteResult = await _receivingService.DeleteNonPOEntryAsync(entry.Id);
            if (deleteResult.IsSuccess)
            {
                SavedEntries.Remove(entry);
                SavedEntriesPanel.Visibility =
                    SavedEntries.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
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

        var currentUser =
            App.GetService<MTM_Receiving_Application.Module_Core.Contracts.Services.IService_UserSessionManager>().CurrentSession?.User?.WindowsUsername
            ?? "System";

        if (SaveForNextTimeCheckBox.IsChecked == true)
        {
            await _receivingService.SaveNonPOEntryAsync(reference, currentUser);
        }

        if (!string.IsNullOrWhiteSpace(_partId))
        {
            var shouldPersistPartDefault = true;

            if (
                string.IsNullOrWhiteSpace(_savedPartDefaultValue) is false
                && !string.Equals(
                    _savedPartDefaultValue,
                    reference,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                shouldPersistPartDefault = await ConfirmOverwritePartDefaultAsync(reference);
            }

            if (shouldPersistPartDefault)
            {
                await _receivingService.SaveNonPOPartDefaultAsync(_partId, reference, currentUser);
            }
        }
    }

    private async Task<bool> ConfirmOverwritePartDefaultAsync(string reference)
    {
        var dialog = new ContentDialog
        {
            Title = "Overwrite saved part default?",
            Content =
                $"Part {_partId} already has '{_savedPartDefaultValue}' saved. Overwrite it with '{reference}' for future non-PO prompts?",
            PrimaryButtonText = "Overwrite",
            CloseButtonText = "Keep Existing",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot,
        };

        Helper_UI_ContentDialogTheme.ApplyTheme(dialog, XamlRoot);
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }
}
