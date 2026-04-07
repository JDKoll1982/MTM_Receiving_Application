using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;
using Windows.Foundation;
using Windows.Storage.Pickers;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_QuickAddPartDialog : ContentDialog
{
    private const int WizardStepCount = 3;
    private const int ConfiguredSpecsPerPage = 6;
    private const int CustomSpecsPerPage = 4;

    public bool WasAccepted { get; private set; }

    public string PartId { get; private set; } = string.Empty;
    public int TypeId { get; }
    public string TypeName { get; }
    public string SpecValuesJson { get; private set; } = "{}";
    public string HomeLocation { get; private set; } = string.Empty;
    public string SelectedInventoryMethod { get; private set; } = "Not Inventoried";
    public string SelectedImagePath { get; private set; } = string.Empty;
    public bool RequestChooseExistingSpecs { get; private set; }

    private readonly List<Model_DunnageSpec> _specs;
    private readonly List<FrameworkElement> _specPanels = new();
    private readonly Dictionary<string, Control> _specInputs = new();
    private readonly ObservableCollection<Model_DunnagePartCustomSpecEntry> _customSpecs = new();
    private readonly ObservableCollection<Model_DunnagePartCustomSpecEntry> _visibleCustomSpecs =
        new();
    private int _currentWizardStep;
    private int _currentSpecPage;
    private int _currentCustomSpecsPage;

    public ObservableCollection<Model_DunnagePartCustomSpecEntry> VisibleCustomSpecs =>
        _visibleCustomSpecs;

    public View_Dunnage_QuickAddPartDialog(
        int typeId,
        string typeName,
        List<Model_DunnageSpec> specs,
        Model_DunnagePartDialogDraft? initialDraft = null
    )
    {
        InitializeComponent();
        WasAccepted = false;

        TypeId = typeId;
        TypeName = typeName;
        TypeNameTextBlock.Text = typeName;
        _specs = specs;

        GenerateSpecFields();
        RenderCustomSpecsPage();
        _currentWizardStep = 0;
        UpdateWizardStepVisibility();
        UpdateWizardNavigation();
        UpdateImagePreview();

        if (initialDraft is not null)
        {
            ApplyDraft(initialDraft);
        }
        else
        {
            SelectInventoryMethod("Not Inventoried");
            PartIdTextBox.Text = BuildSuggestedPartId();
        }
    }

    public void PrepareDialogSize()
    {
        if (XamlRoot is null)
        {
            return;
        }

        RootGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        var desiredWidth = Math.Ceiling(Math.Max(RootGrid.DesiredSize.Width, 900) + 32);
        var availableWidth = Math.Max(900, XamlRoot.Size.Width - 32);
        var availableHeight = Math.Max(760, XamlRoot.Size.Height - 48);

        Width = Math.Min(desiredWidth, availableWidth);
        MinWidth = Math.Min(900, availableWidth);
        MinHeight = Math.Min(760, availableHeight);
        MaxHeight = availableHeight;
    }

    public Model_DunnagePartDialogDraft GetDraft()
    {
        var specValues = BuildMergedSpecValues(requireValidCustomSpecs: false, out _);
        var draft = new Model_DunnagePartDialogDraft
        {
            PartId = PartIdTextBox.Text.Trim(),
            ImagePath = SelectedImagePath,
            HomeLocation = HomeLocationTextBox.Text.Trim(),
            Notes = NotesTextBox.Text.Trim(),
            SelectedInventoryMethod = GetSelectedInventoryMethod(),
            SpecValues = new Dictionary<string, object?>(specValues),
        };

        draft.SpecValues.Remove("Notes");
        return draft;
    }

    private void ApplyDraft(Model_DunnagePartDialogDraft draft)
    {
        PartIdTextBox.Text = draft.PartId;
        SelectedImagePath = draft.ImagePath;
        HomeLocationTextBox.Text = draft.HomeLocation;
        NotesTextBox.Text = draft.Notes;
        SelectInventoryMethod(draft.SelectedInventoryMethod);
        UpdateImagePreview();

        foreach (var pair in _specInputs)
        {
            if (!draft.SpecValues.TryGetValue(pair.Key, out var rawValue))
            {
                continue;
            }

            ApplyControlValue(pair.Value, rawValue);
        }

        LoadCustomSpecs(draft.SpecValues);
    }

    private void GenerateSpecFields()
    {
        _specPanels.Clear();
        _specInputs.Clear();

        for (var index = 0; index < _specs.Count; index++)
        {
            var spec = _specs[index];
            var definition = ParseDefinition(spec.SpecValue);
            var panel = CreateSpecPanel(spec.SpecKey, definition, out var inputControl);
            _specPanels.Add(panel);
            _specInputs[spec.SpecKey] = inputControl;
        }

        _currentSpecPage = 0;
        RenderSpecPage();
    }

    private static SpecDefinition ParseDefinition(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<SpecDefinition>(json)
                ?? new SpecDefinition { DataType = "Text" };
        }
        catch
        {
            return new SpecDefinition { DataType = "Text" };
        }
    }

    private StackPanel CreateSpecPanel(
        string specKey,
        SpecDefinition definition,
        out Control inputControl
    )
    {
        var panel = new StackPanel { Spacing = 4 };
        var labelText = specKey;
        if (definition.Required)
        {
            labelText += " *";
        }

        if (!string.IsNullOrWhiteSpace(definition.Unit))
        {
            labelText += $" ({definition.Unit})";
        }

        panel.Children.Add(
            new TextBlock
            {
                Text = labelText,
                Style = (Style?)Application.Current.Resources["CaptionTextBlockStyle"],
            }
        );

        inputControl = CreateInputControl(specKey, definition);
        panel.Children.Add(inputControl);
        return panel;
    }

    private static Control CreateInputControl(string specKey, SpecDefinition definition)
    {
        if (string.Equals(definition.DataType, "Number", StringComparison.OrdinalIgnoreCase))
        {
            var numberBox = new NumberBox
            {
                PlaceholderText = "0",
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
            };
            if (definition.MinValue.HasValue)
            {
                numberBox.Minimum = definition.MinValue.Value;
            }

            if (definition.MaxValue.HasValue)
            {
                numberBox.Maximum = definition.MaxValue.Value;
            }

            return numberBox;
        }

        if (string.Equals(definition.DataType, "Boolean", StringComparison.OrdinalIgnoreCase))
        {
            return new CheckBox { Content = "Yes" };
        }

        if (string.Equals(definition.DataType, "Choices", StringComparison.OrdinalIgnoreCase))
        {
            var comboBox = new ComboBox
            {
                PlaceholderText = $"Select {specKey.ToLowerInvariant()}",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            foreach (var choice in definition.Choices)
            {
                comboBox.Items.Add(choice);
            }

            return comboBox;
        }

        return new TextBox
        {
            PlaceholderText = $"Enter {specKey.ToLowerInvariant()}",
            MaxLength = 100,
        };
    }

    private void AddPanelToGrid(FrameworkElement panel, int index)
    {
        var row = index / 2;
        while (DynamicSpecsGrid.RowDefinitions.Count <= row)
        {
            DynamicSpecsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        Grid.SetRow(panel, row);
        Grid.SetColumn(panel, index % 2);
        DynamicSpecsGrid.Children.Add(panel);
    }

    private int GetSpecPageCount()
    {
        return Math.Max(
            1,
            (_specPanels.Count + ConfiguredSpecsPerPage - 1) / ConfiguredSpecsPerPage
        );
    }

    private void RenderSpecPage()
    {
        DynamicSpecsGrid.Children.Clear();
        DynamicSpecsGrid.RowDefinitions.Clear();

        if (_specPanels.Count == 0)
        {
            DynamicSpecsGrid.Visibility = Visibility.Collapsed;
            NoConfiguredSpecsTextBlock.Visibility = Visibility.Visible;
            SpecPageSummaryTextBlock.Text = "No configured specs";
            PreviousSpecPageButton.IsEnabled = false;
            NextSpecPageButton.IsEnabled = false;
            return;
        }

        NoConfiguredSpecsTextBlock.Visibility = Visibility.Collapsed;
        DynamicSpecsGrid.Visibility = Visibility.Visible;

        var pageCount = GetSpecPageCount();
        _currentSpecPage = Math.Clamp(_currentSpecPage, 0, pageCount - 1);

        foreach (
            var panel in _specPanels
                .Skip(_currentSpecPage * ConfiguredSpecsPerPage)
                .Take(ConfiguredSpecsPerPage)
                .Select((panel, index) => new { panel, index })
        )
        {
            AddPanelToGrid(panel.panel, panel.index);
        }

        SpecPageSummaryTextBlock.Text = $"Group {_currentSpecPage + 1} of {pageCount}";
        PreviousSpecPageButton.IsEnabled = _currentSpecPage > 0;
        NextSpecPageButton.IsEnabled = _currentSpecPage < pageCount - 1;
    }

    private Dictionary<string, object?> GetConfiguredSpecValues()
    {
        var specValues = new Dictionary<string, object?>();
        foreach (var pair in _specInputs)
        {
            if (pair.Value is TextBox textBox && !string.IsNullOrWhiteSpace(textBox.Text))
            {
                specValues[pair.Key] = textBox.Text.Trim();
            }
            else if (pair.Value is NumberBox numberBox && !double.IsNaN(numberBox.Value))
            {
                specValues[pair.Key] = numberBox.Value;
            }
            else if (pair.Value is CheckBox checkBox)
            {
                specValues[pair.Key] = checkBox.IsChecked ?? false;
            }
            else if (pair.Value is ComboBox comboBox && comboBox.SelectedItem is string choice)
            {
                specValues[pair.Key] = choice.Trim();
            }
        }

        return specValues;
    }

    private Dictionary<string, object?> BuildMergedSpecValues(
        bool requireValidCustomSpecs,
        out string validationMessage
    )
    {
        HideCustomSpecValidation();

        var specValues = GetConfiguredSpecValues();
        var customSpecValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var customSpec in _customSpecs)
        {
            var name = customSpec.Name.Trim();
            var value = customSpec.Value.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                if (requireValidCustomSpecs && !string.IsNullOrWhiteSpace(value))
                {
                    validationMessage = "Each part-specific spec needs a name.";
                    return specValues;
                }

                continue;
            }

            if (string.Equals(name, "Notes", StringComparison.OrdinalIgnoreCase))
            {
                if (requireValidCustomSpecs)
                {
                    validationMessage = "Part-specific specs cannot use the reserved name 'Notes'.";
                    return specValues;
                }

                continue;
            }

            if (
                _specInputs.Keys.Any(key =>
                    string.Equals(key, name, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                if (requireValidCustomSpecs)
                {
                    validationMessage = $"'{name}' is already defined as a type spec.";
                    return specValues;
                }

                continue;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                if (requireValidCustomSpecs)
                {
                    validationMessage = $"Enter a value for part-specific spec '{name}'.";
                    return specValues;
                }

                customSpecValues[name] = string.Empty;
                continue;
            }

            if (!customSpecValues.TryAdd(name, value))
            {
                if (requireValidCustomSpecs)
                {
                    validationMessage = $"Part-specific spec '{name}' is listed more than once.";
                    return specValues;
                }

                customSpecValues[name] = value;
            }
        }

        foreach (var pair in customSpecValues)
        {
            specValues[pair.Key] = pair.Value;
        }

        validationMessage = string.Empty;
        return specValues;
    }

    private void LoadCustomSpecs(IReadOnlyDictionary<string, object?> specValues)
    {
        _customSpecs.Clear();

        foreach (var pair in specValues.OrderBy(item => item.Key))
        {
            if (string.Equals(pair.Key, "Notes", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (
                _specInputs.Keys.Any(key =>
                    string.Equals(key, pair.Key, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                continue;
            }

            _customSpecs.Add(
                new Model_DunnagePartCustomSpecEntry
                {
                    Name = pair.Key,
                    Value = GetString(pair.Value),
                }
            );
        }

        _currentCustomSpecsPage = 0;
        RenderCustomSpecsPage();
    }

    private int GetCustomSpecsPageCount()
    {
        return Math.Max(1, (_customSpecs.Count + CustomSpecsPerPage - 1) / CustomSpecsPerPage);
    }

    private void RenderCustomSpecsPage()
    {
        _visibleCustomSpecs.Clear();

        if (_customSpecs.Count == 0)
        {
            NoCustomSpecsTextBlock.Visibility = Visibility.Visible;
            CustomSpecsItemsRepeater.Visibility = Visibility.Collapsed;
            CustomSpecsPageSummaryTextBlock.Text = "No part-specific specs";
            PreviousCustomSpecsPageButton.IsEnabled = false;
            NextCustomSpecsPageButton.IsEnabled = false;
            return;
        }

        NoCustomSpecsTextBlock.Visibility = Visibility.Collapsed;
        CustomSpecsItemsRepeater.Visibility = Visibility.Visible;

        var pageCount = GetCustomSpecsPageCount();
        _currentCustomSpecsPage = Math.Clamp(_currentCustomSpecsPage, 0, pageCount - 1);

        foreach (
            var spec in _customSpecs
                .Skip(_currentCustomSpecsPage * CustomSpecsPerPage)
                .Take(CustomSpecsPerPage)
        )
        {
            _visibleCustomSpecs.Add(spec);
        }

        var firstItemNumber = _currentCustomSpecsPage * CustomSpecsPerPage + 1;
        var lastItemNumber = firstItemNumber + _visibleCustomSpecs.Count - 1;
        CustomSpecsPageSummaryTextBlock.Text =
            $"Showing {firstItemNumber}-{lastItemNumber} of {_customSpecs.Count}";
        PreviousCustomSpecsPageButton.IsEnabled = _currentCustomSpecsPage > 0;
        NextCustomSpecsPageButton.IsEnabled = _currentCustomSpecsPage < pageCount - 1;
    }

    private string BuildSuggestedPartId()
    {
        return Helper_Dunnage_PartIdSuggestion.BuildSuggestedPartId(
            TypeName,
            GetConfiguredSpecValues()
        );
    }

    private void SuggestPartIdButton_Click(object sender, RoutedEventArgs e)
    {
        PartIdTextBox.Text = BuildSuggestedPartId();
    }

    private async void ChooseImageButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".png");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync().AsTask();
        if (file is null)
        {
            return;
        }

        if (
            string.Equals(
                Path.GetExtension(file.Path),
                ".png",
                System.StringComparison.OrdinalIgnoreCase
            )
            is false
        )
        {
            return;
        }

        SelectedImagePath = file.Path;
        UpdateImagePreview();
    }

    private void ClearImageButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedImagePath = string.Empty;
        UpdateImagePreview();
    }

    private void AddCustomSpecButton_Click(object sender, RoutedEventArgs e)
    {
        var name = NewCustomSpecNameTextBox.Text.Trim();
        var value = NewCustomSpecValueTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowCustomSpecValidation("Enter a name for the part-specific spec.");
            NewCustomSpecNameTextBox.Focus(FocusState.Programmatic);
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            ShowCustomSpecValidation("Enter a value for the part-specific spec.");
            NewCustomSpecValueTextBox.Focus(FocusState.Programmatic);
            return;
        }

        if (string.Equals(name, "Notes", StringComparison.OrdinalIgnoreCase))
        {
            ShowCustomSpecValidation("'Notes' is reserved for the dialog notes field.");
            NewCustomSpecNameTextBox.Focus(FocusState.Programmatic);
            return;
        }

        if (
            _specInputs.Keys.Any(key =>
                string.Equals(key, name, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            ShowCustomSpecValidation($"'{name}' is already defined as a type spec.");
            NewCustomSpecNameTextBox.Focus(FocusState.Programmatic);
            return;
        }

        if (
            _customSpecs.Any(spec =>
                string.Equals(spec.Name, name, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            ShowCustomSpecValidation($"'{name}' is already in the part-specific spec list.");
            NewCustomSpecNameTextBox.Focus(FocusState.Programmatic);
            return;
        }

        _customSpecs.Add(new Model_DunnagePartCustomSpecEntry { Name = name, Value = value });
        HideCustomSpecValidation();
        NewCustomSpecNameTextBox.Text = string.Empty;
        NewCustomSpecValueTextBox.Text = string.Empty;
        _currentCustomSpecsPage = GetCustomSpecsPageCount() - 1;
        RenderCustomSpecsPage();
        NewCustomSpecNameTextBox.Focus(FocusState.Programmatic);
    }

    private void RemoveCustomSpecButton_Click(object sender, RoutedEventArgs e)
    {
        if (
            sender is Button button
            && button.DataContext is Model_DunnagePartCustomSpecEntry customSpec
        )
        {
            _customSpecs.Remove(customSpec);
            HideCustomSpecValidation();
            RenderCustomSpecsPage();
        }
    }

    private void ChooseExistingSpecsButton_Click(object sender, RoutedEventArgs e)
    {
        WasAccepted = false;
        RequestChooseExistingSpecs = true;
        Hide();
    }

    private void SelectInventoryMethod(string inventoryMethod)
    {
        foreach (var item in InventoryTypeComboBox.Items.OfType<ComboBoxItem>())
        {
            var content = item.Content?.ToString() ?? string.Empty;
            if (string.Equals(content, inventoryMethod, StringComparison.OrdinalIgnoreCase))
            {
                InventoryTypeComboBox.SelectedItem = item;
                return;
            }
        }

        InventoryTypeComboBox.SelectedIndex = 0;
    }

    private string GetSelectedInventoryMethod()
    {
        return (InventoryTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()
            ?? "Not Inventoried";
    }

    private static void ApplyControlValue(Control control, object? rawValue)
    {
        if (control is TextBox textBox)
        {
            textBox.Text = GetString(rawValue);
            return;
        }

        if (control is NumberBox numberBox)
        {
            numberBox.Value = GetDouble(rawValue);
            return;
        }

        if (control is CheckBox checkBox)
        {
            checkBox.IsChecked = GetBool(rawValue);
            return;
        }

        if (control is ComboBox comboBox)
        {
            var stringValue = GetString(rawValue);
            foreach (var item in comboBox.Items)
            {
                if (
                    string.Equals(item?.ToString(), stringValue, StringComparison.OrdinalIgnoreCase)
                )
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
        }
    }

    private static string GetString(object? rawValue)
    {
        if (rawValue is JsonElement element)
        {
            return element.ValueKind == JsonValueKind.String
                ? element.GetString() ?? string.Empty
                : element.ToString();
        }

        return rawValue?.ToString() ?? string.Empty;
    }

    private static double GetDouble(object? rawValue)
    {
        if (rawValue is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.String when double.TryParse(element.GetString(), out var parsed) =>
                    parsed,
                _ => double.NaN,
            };
        }

        return rawValue is null ? double.NaN : Convert.ToDouble(rawValue);
    }

    private static bool GetBool(object? rawValue)
    {
        if (rawValue is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(element.GetString(), out var parsed)
                    && parsed,
                _ => false,
            };
        }

        return rawValue is not null && Convert.ToBoolean(rawValue);
    }

    private bool TryCommit()
    {
        RequestChooseExistingSpecs = false;
        PartId = PartIdTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(PartId))
        {
            PartIdTextBox.Focus(FocusState.Programmatic);
            return false;
        }

        var specValues = BuildMergedSpecValues(
            requireValidCustomSpecs: true,
            out var validationMessage
        );
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            ShowCustomSpecValidation(validationMessage);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(NotesTextBox.Text))
        {
            specValues["Notes"] = NotesTextBox.Text.Trim();
        }

        SpecValuesJson = specValues.Count > 0 ? JsonSerializer.Serialize(specValues) : "{}";
        HomeLocation = HomeLocationTextBox.Text.Trim();
        SelectedInventoryMethod = GetSelectedInventoryMethod();
        return true;
    }

    private void OnFooterPrimaryButtonClick(object sender, RoutedEventArgs e)
    {
        if (TryCommit())
        {
            WasAccepted = true;
            Hide();
        }
    }

    private void OnFooterCancelButtonClick(object sender, RoutedEventArgs e)
    {
        WasAccepted = false;
        Hide();
    }

    private void OnBackStepClick(object sender, RoutedEventArgs e)
    {
        if (_currentWizardStep > 0)
        {
            _currentWizardStep--;
            UpdateWizardStepVisibility();
            UpdateWizardNavigation();
        }
    }

    private void OnNextStepClick(object sender, RoutedEventArgs e)
    {
        if (_currentWizardStep < WizardStepCount - 1)
        {
            _currentWizardStep++;
            UpdateWizardStepVisibility();
            UpdateWizardNavigation();
        }
    }

    private void UpdateWizardStepVisibility()
    {
        StepPartSetupPanel.Visibility =
            _currentWizardStep == 0 ? Visibility.Visible : Visibility.Collapsed;
        StepTypeSpecsPanel.Visibility =
            _currentWizardStep == 1 ? Visibility.Visible : Visibility.Collapsed;
        StepPartSpecificSpecsPanel.Visibility =
            _currentWizardStep == 2 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateWizardNavigation()
    {
        var selectedIndex = Math.Max(_currentWizardStep, 0);

        BackStepButton.IsEnabled = selectedIndex > 0;
        NextStepButton.IsEnabled = selectedIndex < WizardStepCount - 1;
        StepSummaryTextBlock.Text = selectedIndex switch
        {
            0 => "Step 1 of 3 • Part Setup",
            1 => "Step 2 of 3 • Type Specs",
            2 => "Step 3 of 3 • Part-Specific Specs",
            _ => string.Empty,
        };
    }

    private void OnPreviousSpecPageClick(object sender, RoutedEventArgs e)
    {
        if (_currentSpecPage > 0)
        {
            _currentSpecPage--;
            RenderSpecPage();
        }
    }

    private void OnNextSpecPageClick(object sender, RoutedEventArgs e)
    {
        if (_currentSpecPage < GetSpecPageCount() - 1)
        {
            _currentSpecPage++;
            RenderSpecPage();
        }
    }

    private void OnPreviousCustomSpecsPageClick(object sender, RoutedEventArgs e)
    {
        if (_currentCustomSpecsPage > 0)
        {
            _currentCustomSpecsPage--;
            RenderCustomSpecsPage();
        }
    }

    private void OnNextCustomSpecsPageClick(object sender, RoutedEventArgs e)
    {
        if (_currentCustomSpecsPage < GetCustomSpecsPageCount() - 1)
        {
            _currentCustomSpecsPage++;
            RenderCustomSpecsPage();
        }
    }

    private void ShowCustomSpecValidation(string message)
    {
        CustomSpecsValidationTextBlock.Text = message;
        CustomSpecsValidationTextBlock.Visibility = Visibility.Visible;
    }

    private void HideCustomSpecValidation()
    {
        CustomSpecsValidationTextBlock.Text = string.Empty;
        CustomSpecsValidationTextBlock.Visibility = Visibility.Collapsed;
    }

    private void UpdateImagePreview()
    {
        PartImagePreview.Source = Helper_DunnageImagePaths.CreateImageSource(SelectedImagePath);
        PartImagePathTextBlock.Text = SelectedImagePath;
    }
}
