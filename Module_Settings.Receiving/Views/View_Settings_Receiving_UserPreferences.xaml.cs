using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_UserPreferences : Page
{
    public ViewModel_Settings_Receiving_UserPreferences ViewModel { get; }

    public View_Settings_Receiving_UserPreferences(ViewModel_Settings_Receiving_UserPreferences viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;

        // Subscribe to SelectedRule changes
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
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
        }
        else
        {
            RuleEditorFields.Visibility = Visibility.Visible;
            NoSelectionText.Visibility = Visibility.Collapsed;

            // Bind values to controls
            PrefixTextBox.Text = ViewModel.SelectedRule.Prefix;
            MaxLengthNumberBox.Value = ViewModel.SelectedRule.MaxLength;
            IsEnabledToggle.IsOn = ViewModel.SelectedRule.IsEnabled;

            // Set PadChar combobox selection
            SetPadCharSelection(ViewModel.SelectedRule.PadChar);

            // Wire up two-way binding manually
            PrefixTextBox.TextChanged += (s, e) =>
            {
                if (ViewModel.SelectedRule != null)
                {
                    ViewModel.SelectedRule.Prefix = PrefixTextBox.Text;
                }
            };

            MaxLengthNumberBox.ValueChanged += (s, e) =>
            {
                if (ViewModel.SelectedRule != null)
                {
                    ViewModel.SelectedRule.MaxLength = (int)MaxLengthNumberBox.Value;
                }
            };

            IsEnabledToggle.Toggled += (s, e) =>
            {
                if (ViewModel.SelectedRule != null)
                {
                    ViewModel.SelectedRule.IsEnabled = IsEnabledToggle.IsOn;
                }
            };
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
            _ => 0
        };
    }

    private void PadCharComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox || ViewModel.SelectedRule is null)
        {
            return;
        }

        if (comboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string tag)
        {
            if (tag.Length > 0)
            {
                ViewModel.SelectedRule.PadChar = tag[0];
            }
        }
    }
}


