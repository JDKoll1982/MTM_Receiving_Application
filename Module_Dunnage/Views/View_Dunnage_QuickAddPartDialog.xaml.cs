using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using Windows.Foundation;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_QuickAddPartDialog : ContentDialog
{
    private const int WizardStepCount = 3;
    private const int ConfiguredSpecsPerPage = 6;

    public bool WasAccepted { get; private set; }

    public string PartId { get; private set; } = string.Empty;
    public int TypeId { get; }
    public string TypeName { get; }
    public string SpecValuesJson { get; private set; } = "{}";
    public string HomeLocation { get; private set; } = string.Empty;
    public string SelectedInventoryMethod { get; private set; } = "Not Inventoried";
    public string SelectedImagePath { get; private set; } = string.Empty;
    public bool RequestChooseExistingSpecs { get; private set; }
    public bool RequestDelete { get; private set; }

    private readonly List<Model_DunnageSpec> _specs;
    private readonly List<FrameworkElement> _specPanels = new();
    private readonly Dictionary<string, Control> _specInputs = new();
    private readonly ObservableCollection<Model_SpecItem> _partSpecificSpecs = new();
    private readonly ObservableCollection<string> _partSpecificChoices = new();
    private readonly IService_DunnageImageStorage _imageStorage;
    private int _currentWizardStep;
    private int _currentSpecPage;

    public ViewModel_Dunnage_PartDialogValidation ValidationViewModel { get; } = new();

    public Visibility ValidationMessageVisibility =>
        string.IsNullOrWhiteSpace(ValidationViewModel.ValidationMessage)
            ? Visibility.Collapsed
            : Visibility.Visible;

    public ObservableCollection<Model_SpecItem> PartSpecificSpecs => _partSpecificSpecs;

    public ObservableCollection<string> PartSpecificChoices => _partSpecificChoices;

    public View_Dunnage_QuickAddPartDialog(
        int typeId,
        string typeName,
        List<Model_DunnageSpec> specs,
        Model_DunnagePartDialogDraft? initialDraft = null,
        bool canDelete = false
    )
    {
        _imageStorage = App.GetService<IService_DunnageImageStorage>();
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
        WasAccepted = false;
        RequestDelete = false;
        FooterDeleteButton.Visibility = canDelete ? Visibility.Visible : Visibility.Collapsed;

        TypeId = typeId;
        TypeName = typeName;
        TypeNameTextBlock.Text = typeName;
        _specs = specs;

        GenerateSpecFields();
        InitializePartSpecificSpecEditor();
        _currentWizardStep = 0;
        UpdateWizardStepVisibility();
        UpdateWizardNavigation();
        UpdateImagePreview();
        UpdateValidationState();

        if (initialDraft is not null)
        {
            ApplyDraft(initialDraft);
        }
        else
        {
            SelectInventoryMethod("Not Inventoried");
            PartIdTextBox.Text = BuildSuggestedPartId();
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
        var specValues = BuildDraftSpecValues();
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

        LoadPartSpecificSpecDefinitions(draft.SpecValues);
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

        SpecPageSummaryTextBlock.Text = $"Page {_currentSpecPage + 1} of {pageCount}";
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

    private Dictionary<string, object?> BuildDraftSpecValues()
    {
        return Helper_Dunnage_PartSpecs.BuildCombinedSpecPayload(
            GetConfiguredSpecValues(),
            _partSpecificSpecs,
            NotesTextBox.Text
        );
    }

    private void InitializePartSpecificSpecEditor()
    {
        PartSpecificChoicesListView.ItemsSource = _partSpecificChoices;
        PartSpecificSpecsListView.ItemsSource = _partSpecificSpecs;
        PartSpecificSpecTypeComboBox.SelectedIndex = 0;
        UpdatePartSpecificSpecOptionVisibility();
        ResetPartSpecificSpecEditor();
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
        if (App.MainWindow is null)
        {
            return;
        }

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        var initialDirectory = await ResolvePreferredImageFolderAsync();
        var selectedFilePath = Helper_DunnageImageFileDialog.ChooseImageFile(
            hwnd,
            initialDirectory
        );
        if (string.IsNullOrWhiteSpace(selectedFilePath))
        {
            return;
        }

        var extension = Path.GetExtension(selectedFilePath);
        if (
            string.Equals(extension, ".png", System.StringComparison.OrdinalIgnoreCase) is false
            && string.Equals(extension, ".jpg", System.StringComparison.OrdinalIgnoreCase) is false
            && string.Equals(extension, ".jpeg", System.StringComparison.OrdinalIgnoreCase) is false
        )
        {
            return;
        }

        SelectedImagePath = selectedFilePath;
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

    private void OnOpenImageFolderTapped(object sender, TappedRoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SelectedImagePath))
        {
            return;
        }

        var absoluteImagePath = Path.IsPathRooted(SelectedImagePath)
            ? SelectedImagePath
            : _imageStorage.GetAbsolutePath(SelectedImagePath);

        if (string.IsNullOrWhiteSpace(absoluteImagePath))
        {
            return;
        }

        var directoryPath =
            File.Exists(absoluteImagePath) ? Path.GetDirectoryName(absoluteImagePath)
            : Directory.Exists(absoluteImagePath) ? absoluteImagePath
            : Path.GetDirectoryName(absoluteImagePath);

        if (string.IsNullOrWhiteSpace(directoryPath) || Directory.Exists(directoryPath) is false)
        {
            return;
        }

        var explorerArguments = File.Exists(absoluteImagePath)
            ? $"/select,\"{absoluteImagePath}\""
            : $"\"{directoryPath}\"";

        Process.Start(
            new ProcessStartInfo("explorer.exe", explorerArguments) { UseShellExecute = true }
        );
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
        if (!ValidationViewModel.ValidateForSubmit())
        {
            PartIdTextBox.Focus(FocusState.Programmatic);
            UpdateWizardNavigation();
            return false;
        }

        PartId = PartIdTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(PartId))
        {
            PartIdTextBox.Focus(FocusState.Programmatic);
            return false;
        }

        var specValues = BuildDraftSpecValues();
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
        if (
            BackStepButton is null
            || NextStepButton is null
            || FooterPrimaryButton is null
            || StepSummaryTextBlock is null
        )
        {
            return;
        }

        var selectedIndex = Math.Max(_currentWizardStep, 0);
        var isRequiredFieldsValid = ValidationViewModel.IsValid;

        BackStepButton.IsEnabled = selectedIndex > 0;
        NextStepButton.IsEnabled = selectedIndex < WizardStepCount - 1 && isRequiredFieldsValid;
        FooterPrimaryButton.IsEnabled = isRequiredFieldsValid;
        StepSummaryTextBlock.Text = selectedIndex switch
        {
            0 => "Step 1 of 3 • Part Setup",
            1 => "Step 2 of 3 • Type Specs",
            2 => "Step 3 of 3 • Part-Specific Spec Definitions",
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
        if (PartIdTextBox is null || InventoryTypeComboBox is null)
        {
            return;
        }

        ValidationViewModel.Update(PartIdTextBox.Text, TypeId, GetSelectedInventoryMethod());
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
        PartImagePathTextBlock.Text = string.IsNullOrWhiteSpace(SelectedImagePath)
            ? string.Empty
            : Path.GetFileName(SelectedImagePath);
        RotateImageButton.IsEnabled = string.IsNullOrWhiteSpace(SelectedImagePath) is false;
    }

    private async Task<string?> ResolvePreferredImageFolderAsync()
    {
        var absoluteImagePath = Path.IsPathRooted(SelectedImagePath)
            ? SelectedImagePath
            : _imageStorage.GetAbsolutePath(SelectedImagePath);

        var currentImageDirectory = File.Exists(absoluteImagePath)
            ? Path.GetDirectoryName(absoluteImagePath)
            : null;
        if (string.IsNullOrWhiteSpace(currentImageDirectory) is false)
        {
            return currentImageDirectory;
        }

        var configuredRootFolder = await _imageStorage.GetConfiguredRootFolderAsync();
        if (string.IsNullOrWhiteSpace(configuredRootFolder))
        {
            return null;
        }

        var partsFolder = Path.Combine(configuredRootFolder, "Parts");
        return Directory.Exists(partsFolder) ? partsFolder : configuredRootFolder;
    }
}
