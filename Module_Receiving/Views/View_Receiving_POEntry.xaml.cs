using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views
{
    public sealed partial class View_Receiving_POEntry : UserControl, IReceivingWorkflowFocusable
    {
        private static readonly JsonSerializerOptions PartPaddingJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public ViewModel_Receiving_POEntry ViewModel { get; }
        private readonly IService_Focus _focusService;
        private readonly IService_AdaptiveLayout _adaptiveLayout;
        private readonly IService_ReceivingSettings? _receivingSettings;
        private bool _partPaddingRulesLoaded;

        public View_Receiving_POEntry(
            ViewModel_Receiving_POEntry viewModel,
            IService_Focus focusService,
            IService_AdaptiveLayout adaptiveLayout
        )
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            ArgumentNullException.ThrowIfNull(focusService);
            ArgumentNullException.ThrowIfNull(adaptiveLayout);

            ViewModel = viewModel;
            _focusService = focusService;
            _adaptiveLayout = adaptiveLayout;
            _receivingSettings = ResolveReceivingSettings();
            DataContext = ViewModel;
            this.InitializeComponent();

            _focusService.AttachFocusOnVisibility(this, PoNumberTextBox);
            Loaded += View_Receiving_POEntry_Loaded;
            SizeChanged += View_Receiving_POEntry_SizeChanged;
            _ = LoadPartPaddingRulesAsync();
        }

        private void View_Receiving_POEntry_Loaded(object sender, RoutedEventArgs e)
        {
            _ = sender;
            _ = e;
            ApplyAdaptiveLayout();
        }

        private void View_Receiving_POEntry_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _ = sender;
            _ = e;
            ApplyAdaptiveLayout();
        }

        private void ApplyAdaptiveLayout()
        {
            var state = _adaptiveLayout.ResolveReceivingLayoutState(ActualWidth);
            _ = VisualStateManager.GoToState(this, state, false);
        }

        /// <summary>
        /// Moves focus to the PO entry field whenever guided mode re-enters this step.
        /// </summary>
        public void FocusForAccess()
        {
            _focusService.SetFocus(PoNumberTextBox);
        }

        private void POTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Trigger auto-correction command
            ViewModel.PoTextBoxLostFocusCommand.Execute(null);
        }

        private async void PartIDLookupControl_ValidationCompleted(
            object sender,
            Model_SharedLookupValidationCompletedEventArgs e
        )
        {
            if (!_partPaddingRulesLoaded)
            {
                await LoadPartPaddingRulesAsync();
                await PartIDLookupControl.ValidateAsync();
                return;
            }

            if (!e.Result.IsValid)
            {
                ViewModel.ShowStatus(
                    e.Result.Message,
                    MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity.Warning
                );
                return;
            }

            if (e.Result.UsedFuzzyFallback && e.Result.HasExactMatch is false)
            {
                var selectedResult = await ShowFuzzyPickerAsync(
                    e.Result.FormattedValue,
                    e.Result.FuzzyCandidates
                );

                if (selectedResult is null)
                {
                    PartIDLookupControl.InputValue = string.Empty;
                    ViewModel.PartID = string.Empty;
                    return;
                }

                var selectedValue = (selectedResult.Key ?? selectedResult.Label ?? string.Empty)
                    .Trim();

                if (string.IsNullOrWhiteSpace(selectedValue))
                {
                    PartIDLookupControl.InputValue = string.Empty;
                    ViewModel.PartID = string.Empty;
                    return;
                }

                PartIDLookupControl.InputValue = selectedValue;
                ViewModel.PartID = selectedValue;
            }

            ViewModel.PartTextBoxLostFocusCommand.Execute(null);
        }

        private async Task LoadPartPaddingRulesAsync()
        {
            _partPaddingRulesLoaded = false;

            if (_receivingSettings is null)
            {
                ApplyPartPaddingRulesFromJson(
                    ReceivingSettingsDefaults.StringDefaults[ReceivingSettingsKeys.PartNumberPadding.RulesJson]
                );
                _partPaddingRulesLoaded = true;
                return;
            }

            try
            {
                var isEnabled = await _receivingSettings.GetBoolAsync(
                    ReceivingSettingsKeys.PartNumberPadding.Enabled
                );
                if (!isEnabled)
                {
                    PartIDLookupControl.PrefixPaddingRules =
                        Array.Empty<Model_SharedLookupPrefixPaddingRule>();
                    _partPaddingRulesLoaded = true;
                    return;
                }

                var rulesJson = await _receivingSettings.GetStringAsync(
                    ReceivingSettingsKeys.PartNumberPadding.RulesJson
                );

                if (string.IsNullOrWhiteSpace(rulesJson))
                {
                    ApplyPartPaddingRulesFromJson(
                        ReceivingSettingsDefaults.StringDefaults[ReceivingSettingsKeys.PartNumberPadding.RulesJson]
                    );
                    _partPaddingRulesLoaded = true;
                    return;
                }

                ApplyPartPaddingRulesFromJson(rulesJson);
                _partPaddingRulesLoaded = true;
            }
            catch
            {
                ApplyPartPaddingRulesFromJson(
                    ReceivingSettingsDefaults.StringDefaults[ReceivingSettingsKeys.PartNumberPadding.RulesJson]
                );
                _partPaddingRulesLoaded = true;
            }
        }

        private void ApplyPartPaddingRulesFromJson(string rulesJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(rulesJson);
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                {
                    PartIDLookupControl.PrefixPaddingRules =
                        Array.Empty<Model_SharedLookupPrefixPaddingRule>();
                    return;
                }

                var rules = new List<Model_SharedLookupPrefixPaddingRule>();
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    var prefix = GetStringProperty(item, "Prefix");
                    if (string.IsNullOrWhiteSpace(prefix))
                    {
                        continue;
                    }

                    var isEnabled = GetBoolProperty(item, "IsEnabled", defaultValue: true);
                    var maxLength = GetIntProperty(item, "MaxLength", defaultValue: 10);
                    var padCharacter = GetCharProperty(item, "PadChar", defaultValue: '0');

                    if (!isEnabled || maxLength <= 0)
                    {
                        continue;
                    }

                    rules.Add(
                        new Model_SharedLookupPrefixPaddingRule
                        {
                            Prefix = prefix.Trim().ToUpperInvariant(),
                            MaxLength = maxLength,
                            PadCharacter = padCharacter,
                        }
                    );
                }

                PartIDLookupControl.PrefixPaddingRules = rules.ToArray();
            }
            catch
            {
                PartIDLookupControl.PrefixPaddingRules =
                    Array.Empty<Model_SharedLookupPrefixPaddingRule>();
            }
        }

        private static string? GetStringProperty(JsonElement element, string name)
        {
            if (!TryGetPropertyIgnoreCase(element, name, out var prop))
            {
                return null;
            }

            return prop.ValueKind == JsonValueKind.String ? prop.GetString() : prop.ToString();
        }

        private static bool GetBoolProperty(JsonElement element, string name, bool defaultValue)
        {
            if (!TryGetPropertyIgnoreCase(element, name, out var prop))
            {
                return defaultValue;
            }

            return prop.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String
                    => bool.TryParse(prop.GetString(), out var parsed) ? parsed : defaultValue,
                _ => defaultValue,
            };
        }

        private static int GetIntProperty(JsonElement element, string name, int defaultValue)
        {
            if (!TryGetPropertyIgnoreCase(element, name, out var prop))
            {
                return defaultValue;
            }

            if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var numericValue))
            {
                return numericValue;
            }

            if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var parsed))
            {
                return parsed;
            }

            return defaultValue;
        }

        private static char GetCharProperty(JsonElement element, string name, char defaultValue)
        {
            var value = GetStringProperty(element, name);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value[0];
        }

        private static bool TryGetPropertyIgnoreCase(
            JsonElement element,
            string propertyName,
            out JsonElement value
        )
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private async Task<Model_FuzzySearchResult?> ShowFuzzyPickerAsync(
            string searchTerm,
            IReadOnlyList<Model_FuzzySearchResult> items
        )
        {
            if (items.Count == 0)
            {
                return null;
            }

            var xamlRoot = XamlRoot;
            if (xamlRoot is null)
            {
                return null;
            }

            var dialog = new Dialog_FuzzySearchPicker(
                items,
                "Select Part",
                $"No exact part match was found for '{searchTerm}'. Select a similar part to continue."
            )
            {
                XamlRoot = xamlRoot,
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return null;
            }

            return dialog.SelectedResult;
        }

        private static IService_ReceivingSettings? ResolveReceivingSettings()
        {
#pragma warning disable CS0618
            try
            {
                return App.GetService<IService_ReceivingSettings>();
            }
            catch
            {
                return null;
            }
#pragma warning restore CS0618
        }
    }
}
