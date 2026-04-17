using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_PartFormatting : Page
{
    public ViewModel_Settings_Receiving_PartFormatting ViewModel { get; }

    public View_Settings_Receiving_PartFormatting(
        ViewModel_Settings_Receiving_PartFormatting viewModel
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e
    )
    {
        if (e.PropertyName == nameof(ViewModel.SelectedRule))
        {
            UpdateRuleEditor();
        }
    }

    private void UpdateRuleEditor()
    {
        if (ViewModel.SelectedRule is null)
        {
            RuleEditorFields.Visibility = Visibility.Collapsed;
            NoSelectionText.Visibility = Visibility.Visible;
            NameTextBox.Text = string.Empty;
            PrefixTextBox.Text = string.Empty;
            MaxLengthNumberBox.Value = 1;
            IsEnabledToggle.IsOn = false;
            SetPadCharSelection('0');
            return;
        }

        RuleEditorFields.Visibility = Visibility.Visible;
        NoSelectionText.Visibility = Visibility.Collapsed;
        NameTextBox.Text = ViewModel.SelectedRule.Name;
        PrefixTextBox.Text = ViewModel.SelectedRule.Prefix;
        MaxLengthNumberBox.Value = ViewModel.SelectedRule.MaxLength;
        IsEnabledToggle.IsOn = ViewModel.SelectedRule.IsEnabled;
        SetPadCharSelection(ViewModel.SelectedRule.PadChar);
    }

    private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ViewModel.SelectedRule != null)
        {
            ViewModel.SelectedRule.Name = NameTextBox.Text;
            ViewModel.RefreshTestOutput();
        }
    }

    private void PrefixTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ViewModel.SelectedRule != null)
        {
            ViewModel.SelectedRule.Prefix = PrefixTextBox.Text;
            ViewModel.RefreshTestOutput();
        }
    }

    private void MaxLengthNumberBox_ValueChanged(
        NumberBox sender,
        NumberBoxValueChangedEventArgs args
    )
    {
        if (ViewModel.SelectedRule != null)
        {
            ViewModel.SelectedRule.MaxLength = (int)sender.Value;
            ViewModel.RefreshTestOutput();
        }
    }

    private void IsEnabledToggle_Toggled(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedRule != null)
        {
            ViewModel.SelectedRule.IsEnabled = IsEnabledToggle.IsOn;
            ViewModel.RefreshTestOutput();
        }
    }

    private void SetPadCharSelection(char padChar)
    {
        PadCharComboBox.SelectedIndex = padChar switch
        {
            '0' => 0,
            ' ' => 1,
            '_' => 2,
            '-' => 3,
            _ => 0,
        };
    }

    private void PadCharComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox || ViewModel.SelectedRule is null)
        {
            return;
        }

        if (
            comboBox.SelectedItem is ComboBoxItem selectedItem
            && selectedItem.Tag is string tag
            && tag.Length > 0
        )
        {
            ViewModel.SelectedRule.PadChar = tag[0];
            ViewModel.RefreshTestOutput();
        }
    }

    private async void IgnoredLocationTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        await ValidateAndAddIgnoredLocationAsync(sender as TextBox);
    }

    private async void AddIgnoredLocationButton_Click(object sender, RoutedEventArgs e)
    {
        await ValidateAndAddIgnoredLocationAsync(IgnoredLocationTextBox);
    }

    private async Task ValidateAndAddIgnoredLocationAsync(TextBox? textBox)
    {
        if (textBox == null || string.IsNullOrWhiteSpace(ViewModel.PendingIgnoredLocation))
        {
            return;
        }

        var validation = await ViewModel.ValidateIgnoredLocationAsync();
        if (validation.IsValid)
        {
            if (
                ViewModel.TryAddIgnoredLocation(
                    ViewModel.PendingIgnoredLocation,
                    out var successMessage
                )
            )
            {
                ViewModel.ShowStatus(
                    successMessage,
                    Module_Core.Models.Enums.InfoBarSeverity.Success
                );
            }
            else if (!string.IsNullOrWhiteSpace(successMessage))
            {
                ViewModel.ShowStatus(
                    successMessage,
                    Module_Core.Models.Enums.InfoBarSeverity.Warning
                );
            }

            return;
        }

        var suggestionsResult = await ViewModel.GetIgnoredLocationSuggestionsAsync();
        if (suggestionsResult.IsSuccess && suggestionsResult.Data?.Count > 0)
        {
            var dialog = new Dialog_FuzzySearchPicker(
                suggestionsResult.Data,
                "Select Ignored Reconciliation Location",
                $"No exact match was found for '{ViewModel.PendingIgnoredLocation?.Trim()}'. Select a matching location."
            )
            {
                XamlRoot = textBox.XamlRoot,
            };

            var dialogResult = await dialog.ShowAsync();
            if (
                dialogResult == ContentDialogResult.Primary
                && dialog.SelectedResult is not null
                && string.IsNullOrWhiteSpace(dialog.SelectedResult.Label) is false
            )
            {
                if (
                    ViewModel.TryAddIgnoredLocation(
                        dialog.SelectedResult.Label.Trim(),
                        out var successMessage
                    )
                )
                {
                    ViewModel.ShowStatus(
                        successMessage,
                        Module_Core.Models.Enums.InfoBarSeverity.Success
                    );
                }
                else if (!string.IsNullOrWhiteSpace(successMessage))
                {
                    ViewModel.ShowStatus(
                        successMessage,
                        Module_Core.Models.Enums.InfoBarSeverity.Warning
                    );
                }

                return;
            }
        }

        var statusMessage = validation.Message;
        if (
            !suggestionsResult.IsSuccess
            && string.IsNullOrWhiteSpace(suggestionsResult.ErrorMessage) is false
        )
        {
            statusMessage = $"{validation.Message} {suggestionsResult.ErrorMessage}";
        }

        ViewModel.ShowStatus(statusMessage, Module_Core.Models.Enums.InfoBarSeverity.Warning);
    }
}
