using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.Views;

/// <summary>
/// Placeholder scanner workbench page.
/// </summary>
public sealed partial class View_Scanner_Workbench : Page
{
    private readonly IService_ReceivingSettings _receivingSettings;
    private List<Model_PartNumberPrefixRule> _paddingRules = [];
    private bool _isPaddingEnabled;
    private bool _paddingSettingsLoaded;

    public ViewModel_Scanner_Workbench ViewModel { get; }

    public View_Scanner_Workbench(
        ViewModel_Scanner_Workbench viewModel,
        IService_ReceivingSettings receivingSettings
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(receivingSettings);
        ViewModel = viewModel;
        _receivingSettings = receivingSettings;
        InitializeComponent();
        DataContext = ViewModel;
        _ = LoadPaddingSettingsAsync();
    }

    private async Task LoadPaddingSettingsAsync()
    {
        try
        {
            _isPaddingEnabled = await _receivingSettings.GetBoolAsync(
                ReceivingSettingsKeys.PartNumberPadding.Enabled
            );
            var rulesJson = await _receivingSettings.GetStringAsync(
                ReceivingSettingsKeys.PartNumberPadding.RulesJson
            );

            if (!string.IsNullOrWhiteSpace(rulesJson))
            {
                var rules = JsonSerializer.Deserialize<Model_PartNumberPrefixRule[]>(rulesJson);
                _paddingRules = rules?.Where(static rule => rule is not null).ToList() ?? [];
            }

            _paddingSettingsLoaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ScannerWorkbench] Failed to load part padding settings: {ex.Message}");
            _isPaddingEnabled = false;
            _paddingRules = [];
            _paddingSettingsLoaded = true;
        }
    }

    private async void PartTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        if (!_paddingSettingsLoaded)
        {
            await LoadPaddingSettingsAsync();
        }

        var value = textBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            ViewModel.NewPartId = string.Empty;
            return;
        }

        var formattedPartId = ApplyPartNumberPadding(value);
        if (string.Equals(formattedPartId, value, StringComparison.Ordinal))
        {
            return;
        }

        ViewModel.NewPartId = formattedPartId;
        textBox.Text = formattedPartId;
    }

    private string ApplyPartNumberPadding(string input)
    {
        if (!_paddingSettingsLoaded || !_isPaddingEnabled || _paddingRules.Count == 0)
        {
            return input;
        }

        return Model_PartNumberPrefixRule.ApplyBestMatchingRule(_paddingRules.ToArray(), input);
    }
}