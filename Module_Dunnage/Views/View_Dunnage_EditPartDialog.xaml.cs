using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using Windows.Foundation;
using Windows.Storage.Pickers;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_EditPartDialog : ContentDialog
{
    private const int WizardStepCount = 3;
    private const int ConfiguredSpecsPerPage = 6;
    private const int CustomSpecsPerPage = 4;

    public bool WasAccepted { get; private set; }

    public string UpdatedPartId { get; private set; } = string.Empty;
    public string UpdatedSpecValuesJson { get; private set; } = "{}";
    public string UpdatedHomeLocation { get; private set; } = string.Empty;
    public string SelectedInventoryMethod { get; private set; } = "Not Inventoried";
    public string SelectedImagePath { get; private set; } = string.Empty;
    public bool RequestChooseExistingSpecs { get; private set; }
    public bool RequestDelete { get; private set; }

    private readonly Model_DunnagePart _existingPart;
    private readonly List<Model_DunnageSpec> _specs;
    private readonly List<FrameworkElement> _specPanels = new();
    private readonly Dictionary<string, Control> _specInputs = new();
    private readonly ObservableCollection<Model_DunnagePartCustomSpecEntry> _customSpecs = new();
    private readonly ObservableCollection<Model_DunnagePartCustomSpecEntry> _visibleCustomSpecs =
        new();
    private readonly ObservableCollection<Model_SpecItem> _partSpecificSpecs = new();
    private readonly ObservableCollection<string> _partSpecificChoices = new();
    private readonly IService_DunnageImageStorage _imageStorage;
    private readonly string _typeName;
    private readonly bool _usesDefinitionBasedPartSpecificSpecs;
    private int _currentWizardStep;
    private int _currentSpecPage;
    private int _currentCustomSpecsPage;

    public ViewModel_Dunnage_PartDialogValidation ValidationViewModel { get; } = new();

    public Visibility ValidationMessageVisibility =>
        string.IsNullOrWhiteSpace(ValidationViewModel.ValidationMessage)
            ? Visibility.Collapsed
            : Visibility.Visible;

    public ObservableCollection<Model_DunnagePartCustomSpecEntry> VisibleCustomSpecs =>
        _visibleCustomSpecs;

    public ObservableCollection<Model_SpecItem> PartSpecificSpecs => _partSpecificSpecs;

    public ObservableCollection<string> PartSpecificChoices => _partSpecificChoices;

    public View_Dunnage_EditPartDialog(
        Model_DunnagePart existingPart,
        List<Model_DunnageSpec> specs,
        string typeName,
        string inventoryMethod,
        Model_DunnagePartDialogDraft? initialDraft = null,
        bool canDelete = false
    )
    {
        _imageStorage = App.GetService<IService_DunnageImageStorage>();
        InitializeComponent();
        WasAccepted = false;
        RequestDelete = false;
        FooterDeleteButton.Visibility = canDelete ? Visibility.Visible : Visibility.Collapsed;

        _existingPart = existingPart;
        _specs = specs;
        _typeName = typeName;
        _usesDefinitionBasedPartSpecificSpecs = existingPart.UsesDefinitionBasedPartSpecificSpecs;

        CurrentPartIdTextBlock.Text = existingPart.PartId;
        TypeNameTextBlock.Text = typeName;

        GenerateSpecFields();
        InitializePartSpecificSpecEditors();
        _currentWizardStep = 0;
        UpdateWizardStepVisibility();
        UpdateWizardNavigation();
        PrePopulateFromExistingPart(inventoryMethod);
        UpdateValidationState();

        if (initialDraft is not null)
        {
            ApplyDraft(initialDraft);
        }

        UpdateValidationState();
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
        var specValues = _usesDefinitionBasedPartSpecificSpecs
            ? BuildDefinitionBasedDraftSpecValues()
            : BuildLegacyMergedSpecValues(requireValidCustomSpecs: false, out _);
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

            if (Helper_Dunnage_PartSpecs.TryGetSpecDefinition(rawValue, out _))
            {
                continue;
            }

            ApplyControlValue(pair.Value, rawValue);
        }

        if (_usesDefinitionBasedPartSpecificSpecs)
        {
            LoadPartSpecificSpecDefinitions(draft.SpecValues);
        }
        else
        {
            LoadCustomSpecs(draft.SpecValues);
        }
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
            var definition =
                JsonSerializer.Deserialize<SpecDefinition>(json)
                ?? new SpecDefinition { DataType = "Text" };

            definition.DataType = string.IsNullOrWhiteSpace(definition.DataType)
                ? "Text"
                : definition.DataType;
            definition.Unit ??= string.Empty;
            definition.Choices ??= new List<string>();

            return definition;
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

            foreach (var choice in definition.Choices ?? new List<string>())
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

    private void PrePopulateFromExistingPart(string inventoryMethod)
    {
        PartIdTextBox.Text = _existingPart.PartId;
        SelectedImagePath = _existingPart.ImagePath ?? string.Empty;
        HomeLocationTextBox.Text = _existingPart.HomeLocation ?? string.Empty;
        SelectInventoryMethod(inventoryMethod);
        UpdateImagePreview();

        foreach (var pair in _specInputs)
        {
            if (!_existingPart.SpecValuesDict.TryGetValue(pair.Key, out var rawValue))
            {
                continue;
            }

            ApplyControlValue(pair.Value, rawValue);
        }

        if (_existingPart.SpecValuesDict.TryGetValue("Notes", out var notesValue))
        {
            NotesTextBox.Text = GetString(notesValue);
        }

        if (_usesDefinitionBasedPartSpecificSpecs)
        {
            LoadExistingPartSpecificSpecDefinitions();
        }
        else
        {
            var existingSpecValues = _existingPart.SpecValuesDict.ToDictionary(
                pair => pair.Key,
                pair => (object?)pair.Value
            );
            LoadCustomSpecs(existingSpecValues);
        }
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

    private Dictionary<string, object?> BuildLegacyMergedSpecValues(
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
                    validationMessage = "Each extra detail needs a name.";
                    return specValues;
                }

                continue;
            }

            if (string.Equals(name, "Notes", StringComparison.OrdinalIgnoreCase))
            {
                if (requireValidCustomSpecs)
                {
                    validationMessage = "'Notes' is already used by the notes box.";
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
                    validationMessage = $"'{name}' is already listed in the saved part details.";
                    return specValues;
                }

                continue;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                if (requireValidCustomSpecs)
                {
                    validationMessage = $"Enter a value for '{name}'.";
                    return specValues;
                }

                customSpecValues[name] = string.Empty;
                continue;
            }

            if (!customSpecValues.TryAdd(name, value))
            {
                if (requireValidCustomSpecs)
                {
                    validationMessage = $"'{name}' is listed more than once.";
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

    private Dictionary<string, object?> BuildDefinitionBasedDraftSpecValues()
    {
        return Helper_Dunnage_PartSpecs.BuildCombinedSpecPayload(
            GetConfiguredSpecValues(),
            _partSpecificSpecs,
            NotesTextBox.Text
        );
    }

    private void InitializePartSpecificSpecEditors()
    {
        if (_usesDefinitionBasedPartSpecificSpecs)
        {
            DefinitionPartSpecificSpecsPanel.Visibility = Visibility.Visible;
            LegacyPartSpecificSpecsPanel.Visibility = Visibility.Collapsed;
            LegacyPartSpecificSpecsListPanel.Visibility = Visibility.Collapsed;
            PartSpecificChoicesListView.ItemsSource = _partSpecificChoices;
            PartSpecificSpecsListView.ItemsSource = _partSpecificSpecs;
            PartSpecificSpecTypeComboBox.SelectedIndex = 0;
            UpdatePartSpecificSpecOptionVisibility();
            ResetPartSpecificSpecEditor();
            return;
        }

        DefinitionPartSpecificSpecsPanel.Visibility = Visibility.Collapsed;
        LegacyPartSpecificSpecsPanel.Visibility = Visibility.Visible;
        LegacyPartSpecificSpecsListPanel.Visibility = Visibility.Visible;
        RenderCustomSpecsPage();
    }

    private void LoadExistingPartSpecificSpecDefinitions()
    {
        _partSpecificSpecs.Clear();

        foreach (var pair in _existingPart.PartSpecificSpecDefinitions.OrderBy(item => item.Key))
        {
            _partSpecificSpecs.Add(Helper_Dunnage_PartSpecs.CreateSpecItem(pair.Key, pair.Value));
        }
    }

    private void LoadPartSpecificSpecDefinitions(IReadOnlyDictionary<string, object?> specValues)
    {
        _partSpecificSpecs.Clear();

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

            if (!Helper_Dunnage_PartSpecs.TryGetSpecDefinition(pair.Value, out var definition))
            {
                continue;
            }

            _partSpecificSpecs.Add(Helper_Dunnage_PartSpecs.CreateSpecItem(pair.Key, definition));
        }
    }

    private void UpdatePartSpecificSpecOptionVisibility()
    {
        var selectedType = PartSpecificSpecTypeComboBox.SelectedItem?.ToString() ?? "Text";
        PartSpecificNumberOptionsBorder.Visibility = string.Equals(
            selectedType,
            "Number",
            StringComparison.OrdinalIgnoreCase
        )
            ? Visibility.Visible
            : Visibility.Collapsed;
        PartSpecificChoicesBorder.Visibility = string.Equals(
            selectedType,
            "Choices",
            StringComparison.OrdinalIgnoreCase
        )
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void ResetPartSpecificSpecEditor()
    {
        PartSpecificSpecNameTextBox.Text = string.Empty;
        PartSpecificSpecTypeComboBox.SelectedItem = "Text";
        PartSpecificRequiredCheckBox.IsChecked = false;
        PartSpecificUnitTextBox.Text = string.Empty;
        PartSpecificMinValueNumberBox.Value = double.NaN;
        PartSpecificMaxValueNumberBox.Value = double.NaN;
        PartSpecificChoiceTextBox.Text = string.Empty;
        _partSpecificChoices.Clear();
        UpdatePartSpecificSpecOptionVisibility();
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
            CustomSpecsPageSummaryTextBlock.Text = "No extra details";
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
            _typeName,
            GetConfiguredSpecValues()
        );
    }

    private void SuggestPartIdButton_Click(object sender, RoutedEventArgs e)
    {
        PartIdTextBox.Text = BuildSuggestedPartId();
    }

    private async void ChooseImageButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.MainWindow is null)
        {
            return;
        }

        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync().AsTask();
        if (file is null)
        {
            return;
        }

        var extension = Path.GetExtension(file.Path);
        if (
            string.Equals(extension, ".png", System.StringComparison.OrdinalIgnoreCase) is false
            && string.Equals(extension, ".jpg", System.StringComparison.OrdinalIgnoreCase) is false
            && string.Equals(extension, ".jpeg", System.StringComparison.OrdinalIgnoreCase) is false
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

    private async void RotateImageButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SelectedImagePath))
        {
            return;
        }

        var result = await _imageStorage.CreateRotatedWorkingCopyAsync(SelectedImagePath);
        if (!result.IsSuccess)
        {
            return;
        }

        SelectedImagePath = result.Data ?? SelectedImagePath;
        UpdateImagePreview();
    }

    private void AddCustomSpecButton_Click(object sender, RoutedEventArgs e)
    {
        var name = NewCustomSpecNameTextBox.Text.Trim();
        var value = NewCustomSpecValueTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowCustomSpecValidation("Enter a name for the extra detail.");
            NewCustomSpecNameTextBox.Focus(FocusState.Programmatic);
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            ShowCustomSpecValidation("Enter a value for the extra detail.");
            NewCustomSpecValueTextBox.Focus(FocusState.Programmatic);
            return;
        }

        if (string.Equals(name, "Notes", StringComparison.OrdinalIgnoreCase))
        {
            ShowCustomSpecValidation("'Notes' is already used by the notes box.");
            NewCustomSpecNameTextBox.Focus(FocusState.Programmatic);
            return;
        }

        if (
            _specInputs.Keys.Any(key =>
                string.Equals(key, name, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            ShowCustomSpecValidation($"'{name}' is already listed in the saved part details.");
            NewCustomSpecNameTextBox.Focus(FocusState.Programmatic);
            return;
        }

        if (
            _customSpecs.Any(spec =>
                string.Equals(spec.Name, name, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            ShowCustomSpecValidation($"'{name}' is already in the extra details list.");
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

    private void PartSpecificSpecTypeComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        UpdatePartSpecificSpecOptionVisibility();
    }

    private void AddPartSpecificChoiceButton_Click(object sender, RoutedEventArgs e)
    {
        var choice = PartSpecificChoiceTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(choice))
        {
            return;
        }

        if (
            _partSpecificChoices.Any(existing =>
                existing.Equals(choice, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return;
        }

        _partSpecificChoices.Add(choice);
        PartSpecificChoiceTextBox.Text = string.Empty;
        HideCustomSpecValidation();
    }

    private void RemovePartSpecificChoiceButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: string choice })
        {
            _partSpecificChoices.Remove(choice);
        }
    }

    private void AddPartSpecificSpecButton_Click(object sender, RoutedEventArgs e)
    {
        var name = PartSpecificSpecNameTextBox.Text.Trim();
        var selectedType = PartSpecificSpecTypeComboBox.SelectedItem?.ToString() ?? "Text";

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowCustomSpecValidation("Enter a label before adding this prompt.");
            PartSpecificSpecNameTextBox.Focus(FocusState.Programmatic);
            return;
        }

        if (string.Equals(name, "Notes", StringComparison.OrdinalIgnoreCase))
        {
            ShowCustomSpecValidation("'Notes' is already used by the notes box.");
            PartSpecificSpecNameTextBox.Focus(FocusState.Programmatic);
            return;
        }

        if (
            _specInputs.Keys.Any(key =>
                string.Equals(key, name, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            ShowCustomSpecValidation($"'{name}' is already listed in the saved part details.");
            PartSpecificSpecNameTextBox.Focus(FocusState.Programmatic);
            return;
        }

        if (
            _partSpecificSpecs.Any(spec =>
                string.Equals(spec.Name, name, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            ShowCustomSpecValidation($"'{name}' is already in the extra details list.");
            PartSpecificSpecNameTextBox.Focus(FocusState.Programmatic);
            return;
        }

        if (
            string.Equals(selectedType, "Choices", StringComparison.OrdinalIgnoreCase)
            && _partSpecificChoices.Count == 0
        )
        {
            ShowCustomSpecValidation("Add at least one option before saving this prompt.");
            PartSpecificChoiceTextBox.Focus(FocusState.Programmatic);
            return;
        }

        _partSpecificSpecs.Add(
            new Model_SpecItem
            {
                Name = name,
                DataType = selectedType,
                IsRequired = PartSpecificRequiredCheckBox.IsChecked == true,
                Unit = string.Equals(selectedType, "Number", StringComparison.OrdinalIgnoreCase)
                    ? PartSpecificUnitTextBox.Text.Trim()
                    : string.Empty,
                MinValue =
                    string.Equals(selectedType, "Number", StringComparison.OrdinalIgnoreCase)
                    && !double.IsNaN(PartSpecificMinValueNumberBox.Value)
                        ? PartSpecificMinValueNumberBox.Value
                        : null,
                MaxValue =
                    string.Equals(selectedType, "Number", StringComparison.OrdinalIgnoreCase)
                    && !double.IsNaN(PartSpecificMaxValueNumberBox.Value)
                        ? PartSpecificMaxValueNumberBox.Value
                        : null,
                Choices = string.Equals(selectedType, "Choices", StringComparison.OrdinalIgnoreCase)
                    ? _partSpecificChoices.ToList()
                    : new List<string>(),
            }
        );

        HideCustomSpecValidation();
        ResetPartSpecificSpecEditor();
        PartSpecificSpecNameTextBox.Focus(FocusState.Programmatic);
    }

    private void RemovePartSpecificSpecButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: Model_SpecItem spec })
        {
            _partSpecificSpecs.Remove(spec);
            HideCustomSpecValidation();
        }
    }

    private void ChooseExistingSpecsButton_Click(object sender, RoutedEventArgs e)
    {
        WasAccepted = false;
        RequestDelete = false;
        RequestChooseExistingSpecs = true;
        Hide();
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

    private bool TryCommit()
    {
        RequestChooseExistingSpecs = false;
        if (!ValidationViewModel.ValidateForSubmit())
        {
            PartIdTextBox.Focus(FocusState.Programmatic);
            UpdateWizardNavigation();
            return false;
        }

        UpdatedPartId = PartIdTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(UpdatedPartId))
        {
            PartIdTextBox.Focus(FocusState.Programmatic);
            return false;
        }

        Dictionary<string, object?> specValues;
        if (_usesDefinitionBasedPartSpecificSpecs)
        {
            specValues = BuildDefinitionBasedDraftSpecValues();
        }
        else
        {
            specValues = BuildLegacyMergedSpecValues(
                requireValidCustomSpecs: true,
                out var validationMessage
            );
            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                ShowCustomSpecValidation(validationMessage);
                return false;
            }
        }

        UpdatedSpecValuesJson = specValues.Count > 0 ? JsonSerializer.Serialize(specValues) : "{}";
        UpdatedHomeLocation = HomeLocationTextBox.Text.Trim();
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

    private void OnFooterDeleteButtonClick(object sender, RoutedEventArgs e)
    {
        WasAccepted = false;
        RequestDelete = true;
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
        if (_currentWizardStep == 0 && !ValidationViewModel.ValidateForSubmit())
        {
            PartIdTextBox.Focus(FocusState.Programmatic);
            UpdateWizardNavigation();
            return;
        }

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
        var isRequiredFieldsValid = ValidationViewModel.IsValid;

        BackStepButton.IsEnabled = selectedIndex > 0;
        NextStepButton.IsEnabled = selectedIndex < WizardStepCount - 1 && isRequiredFieldsValid;
        FooterPrimaryButton.IsEnabled = isRequiredFieldsValid;
        StepSummaryTextBlock.Text = selectedIndex switch
        {
            0 => "Step 1 of 3 • Part Setup",
            1 => "Step 2 of 3 • Type Specs",
            2 => _usesDefinitionBasedPartSpecificSpecs
                ? "Step 3 of 3 • Part-Specific Spec Definitions"
                : "Step 3 of 3 • Part-Specific Specs",
            _ => string.Empty,
        };

        Bindings.Update();
    }

    private void RequiredPartFieldChanged(object sender, RoutedEventArgs e)
    {
        UpdateValidationState();
    }

    private void UpdateValidationState()
    {
        ValidationViewModel.Update(
            PartIdTextBox.Text,
            _existingPart.TypeId,
            GetSelectedInventoryMethod()
        );
        UpdateWizardNavigation();
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
        RotateImageButton.IsEnabled = string.IsNullOrWhiteSpace(SelectedImagePath) is false;
    }
}
