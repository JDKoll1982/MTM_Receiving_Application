using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
    private const int QuantityTypeWizardStep = 2;

    public bool WasAccepted { get; private set; }

    public string PartId { get; private set; } = string.Empty;
    public int TypeId { get; }
    public string TypeName { get; }
    public string HomeLocation { get; private set; } = string.Empty;
    public string ResolvedQuantityType { get; private set; } = "Quantity";
    public string SelectedInventoryMethod { get; private set; } = "Not Inventoried";
    public string SelectedImagePath { get; private set; } = string.Empty;
    public bool RequestChooseExistingSpecs { get; private set; }
    public bool RequestDelete { get; private set; }
    public bool RequestSaveQuantityTypeForFutureUse { get; private set; }

    private readonly List<Model_CustomFieldDefinition> _customFields;
    private readonly List<Model_DunnageQuantityType> _availableQuantityTypes;
    private readonly List<FrameworkElement> _specPanels = new();
    private readonly Dictionary<string, Control> _specInputs = new();
    private readonly Dictionary<int, Model_CustomFieldDefinition> _fieldsBySlot = new();
    private readonly string?[] _udcValues = new string?[10];
    private readonly ObservableCollection<Model_DunnageQuantityType> _quantityTypes = new();
    private readonly IService_DunnageImageStorage _imageStorage;
    private string _initialResolvedQuantityType = "Quantity";
    private int _currentWizardStep;
    private int _currentSpecPage;

    public ViewModel_Dunnage_PartDialogValidation ValidationViewModel { get; } = new();

    public Visibility ValidationMessageVisibility =>
        string.IsNullOrWhiteSpace(ValidationViewModel.ValidationMessage)
            ? Visibility.Collapsed
            : Visibility.Visible;

    public ObservableCollection<Model_DunnageQuantityType> QuantityTypes => _quantityTypes;

    public View_Dunnage_QuickAddPartDialog(
        int typeId,
        string typeName,
        List<Model_CustomFieldDefinition> customFields,
        List<Model_DunnageQuantityType> quantityTypes,
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
        _customFields =
            customFields ?? new List<Model_CustomFieldDefinition>();
        _availableQuantityTypes = quantityTypes ?? new List<Model_DunnageQuantityType>();

        GenerateSpecFields();
        InitializeQuantityTypes();
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

        CaptureInitialQuantityType();
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
        CopyInputValuesToUdc();
        var draft = new Model_DunnagePartDialogDraft
        {
            PartId = PartIdTextBox.Text.Trim(),
            ImagePath = SelectedImagePath,
            HomeLocation = HomeLocationTextBox.Text.Trim(),
            QuantityType = GetDraftQuantityType(),
            SelectedQuantityType = GetSelectedQuantityType(),
            Notes = NotesTextBox.Text.Trim(),
            SelectedInventoryMethod = GetSelectedInventoryMethod(),
        };

        for (var slot = 1; slot <= 10; slot++)
        {
            draft.SetUdcValue(slot, _udcValues[slot - 1]);
        }

        return draft;
    }

    private void CopyInputValuesToUdc()
    {
        Array.Clear(_udcValues);
        foreach (var pair in _fieldsBySlot)
        {
            if (_specInputs.TryGetValue(pair.Value.FieldName, out var control))
            {
                _udcValues[pair.Key - 1] = GetControlValue(control);
            }
        }
    }

    private void ApplyDraft(Model_DunnagePartDialogDraft draft)
    {
        PartIdTextBox.Text = draft.PartId;
        SelectedImagePath = draft.ImagePath;
        HomeLocationTextBox.Text = draft.HomeLocation;
        ApplyQuantityTypeDraft(draft.QuantityType, draft.SelectedQuantityType);
        NotesTextBox.Text = draft.Notes;
        SelectInventoryMethod(draft.SelectedInventoryMethod);
        UpdateImagePreview();

        foreach (var pair in _fieldsBySlot)
        {
            var value = draft.GetUdcValue(pair.Key);
            if (value is null || !_specInputs.TryGetValue(pair.Value.FieldName, out var control))
            {
                continue;
            }

            ApplyControlValue(control, value);
        }
    }

    private void GenerateSpecFields()
    {
        _specPanels.Clear();
        _specInputs.Clear();
        _fieldsBySlot.Clear();
        Array.Clear(_udcValues);

        foreach (var field in _customFields.OrderBy(field => field.DisplayOrder))
        {
            var panel = CreateSpecPanel(field, out var inputControl);
            _specPanels.Add(panel);
            _specInputs[field.FieldName] = inputControl;
            if (field.DisplayOrder is >= 1 and <= 10)
            {
                _fieldsBySlot[field.DisplayOrder] = field;
            }
        }

        _currentSpecPage = 0;
        RenderSpecPage();
    }

    private StackPanel CreateSpecPanel(
        Model_CustomFieldDefinition field,
        out Control inputControl
    )
    {
        var panel = new StackPanel { Spacing = 4 };
        var labelText = field.FieldName;
        if (field.IsRequired)
        {
            labelText += " *";
        }

        if (!string.IsNullOrWhiteSpace(field.Unit))
        {
            labelText += $" ({field.Unit})";
        }

        panel.Children.Add(
            new TextBlock
            {
                Text = labelText,
                Style = (Style?)Application.Current.Resources["CaptionTextBlockStyle"],
            }
        );

        inputControl = CreateInputControl(field);
        panel.Children.Add(inputControl);
        return panel;
    }

    private static Control CreateInputControl(Model_CustomFieldDefinition field)
    {
        if (string.Equals(field.FieldType, "Number", StringComparison.OrdinalIgnoreCase))
        {
            var numberBox = new NumberBox
            {
                PlaceholderText = "0",
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
            };
            if (field.MinValue.HasValue)
            {
                numberBox.Minimum = (double)field.MinValue.Value;
            }

            if (field.MaxValue.HasValue)
            {
                numberBox.Maximum = (double)field.MaxValue.Value;
            }

            return numberBox;
        }

        if (string.Equals(field.FieldType, "Boolean", StringComparison.OrdinalIgnoreCase))
        {
            return new CheckBox { Content = "Yes" };
        }

        if (string.Equals(field.FieldType, "Choices", StringComparison.OrdinalIgnoreCase))
        {
            var comboBox = new ComboBox
            {
                PlaceholderText = $"Select {field.FieldName.ToLowerInvariant()}",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            foreach (var choice in field.Choices ?? new List<string>())
            {
                comboBox.Items.Add(choice);
            }

            return comboBox;
        }

        return new TextBox
        {
            PlaceholderText = $"Enter {field.FieldName.ToLowerInvariant()}",
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

    private List<KeyValuePair<string, string?>> GetConfiguredSpecValues()
    {
        var values = new List<KeyValuePair<string, string?>>();
        foreach (var pair in _fieldsBySlot.OrderBy(item => item.Key))
        {
            if (!_specInputs.TryGetValue(pair.Value.FieldName, out var control))
            {
                continue;
            }

            var value = GetControlValue(control);
            if (string.IsNullOrWhiteSpace(value) is false)
            {
                values.Add(new KeyValuePair<string, string?>(pair.Value.FieldName, value));
            }
        }

        return values;
    }

    private void InitializeQuantityTypes()
    {
        _quantityTypes.Clear();

        foreach (
            var quantityType in _availableQuantityTypes
                .Where(item => string.IsNullOrWhiteSpace(item.QuantityType) is false)
                .GroupBy(item => item.QuantityType.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(item => item.QuantityType, StringComparer.OrdinalIgnoreCase)
        )
        {
            _quantityTypes.Add(quantityType);
        }
    }

    private void CaptureInitialQuantityType()
    {
        _initialResolvedQuantityType = GetDraftQuantityType();
        ResolvedQuantityType = _initialResolvedQuantityType;
    }

    private void ApplyQuantityTypeDraft(string quantityType, string selectedQuantityType)
    {
        QuantityTypeTextBox.Text = string.Empty;
        QuantityTypesListView.SelectedItem = null;

        if (string.IsNullOrWhiteSpace(selectedQuantityType) is false)
        {
            var explicitMatch = FindQuantityType(selectedQuantityType);
            if (explicitMatch is not null)
            {
                QuantityTypesListView.SelectedItem = explicitMatch;
            }
        }

        if (
            string.IsNullOrWhiteSpace(quantityType)
            || string.Equals(quantityType, "Quantity", StringComparison.OrdinalIgnoreCase)
        )
        {
            return;
        }

        var matchedQuantityType = FindQuantityType(quantityType);
        if (matchedQuantityType is not null && QuantityTypesListView.SelectedItem is null)
        {
            QuantityTypesListView.SelectedItem = matchedQuantityType;
            return;
        }

        QuantityTypeTextBox.Text = quantityType;
    }

    private Model_DunnageQuantityType? FindQuantityType(string quantityType)
    {
        return _quantityTypes.FirstOrDefault(item =>
            string.Equals(item.QuantityType, quantityType, StringComparison.OrdinalIgnoreCase)
        );
    }

    private string GetCustomQuantityType()
    {
        return QuantityTypeTextBox.Text.Trim();
    }

    private string GetSelectedQuantityType()
    {
        return (
                QuantityTypesListView.SelectedItem as Model_DunnageQuantityType
            )?.QuantityType?.Trim() ?? string.Empty;
    }

    private string GetDraftQuantityType()
    {
        var customQuantityType = GetCustomQuantityType();
        if (string.IsNullOrWhiteSpace(customQuantityType) is false)
        {
            return customQuantityType;
        }

        var selectedQuantityType = GetSelectedQuantityType();
        return string.IsNullOrWhiteSpace(selectedQuantityType) ? "Quantity" : selectedQuantityType;
    }

    private bool ShouldPromptToPersistQuantityType(
        string quantityType,
        bool resolvedFromCustomInput
    )
    {
        if (!resolvedFromCustomInput)
        {
            return false;
        }

        if (
            string.Equals(
                quantityType,
                _initialResolvedQuantityType,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return false;
        }

        return _quantityTypes.Any(item =>
            string.Equals(item.QuantityType, quantityType, StringComparison.OrdinalIgnoreCase)
        )
            is false;
    }

    private async Task<(
        bool IsResolved,
        string QuantityType,
        bool SaveForFutureUse
    )> ResolveQuantityTypeAsync()
    {
        var customQuantityType = GetCustomQuantityType();
        var selectedQuantityType = GetSelectedQuantityType();

        if (
            string.IsNullOrWhiteSpace(customQuantityType)
            && string.IsNullOrWhiteSpace(selectedQuantityType)
        )
        {
            return (true, "Quantity", false);
        }

        if (string.IsNullOrWhiteSpace(customQuantityType))
        {
            return (true, selectedQuantityType, false);
        }

        if (string.IsNullOrWhiteSpace(selectedQuantityType))
        {
            return (
                true,
                customQuantityType,
                ShouldPromptToPersistQuantityType(customQuantityType, true)
            );
        }

        if (
            string.Equals(
                customQuantityType,
                selectedQuantityType,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return (
                true,
                customQuantityType,
                ShouldPromptToPersistQuantityType(customQuantityType, true)
            );
        }

        var dialog = new ContentDialog
        {
            Title = "Choose Quantity Type",
            Content =
                "You entered a custom quantity type and also selected a saved quantity type. Choose which one to save.",
            PrimaryButtonText = customQuantityType,
            SecondaryButtonText = selectedQuantityType,
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
        };

        Helper_UI_ContentDialogTheme.ApplyTheme(dialog, XamlRoot);

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            return (
                true,
                customQuantityType,
                ShouldPromptToPersistQuantityType(customQuantityType, true)
            );
        }

        if (result == ContentDialogResult.Secondary)
        {
            return (true, selectedQuantityType, false);
        }

        if (_currentWizardStep != QuantityTypeWizardStep)
        {
            _currentWizardStep = QuantityTypeWizardStep;
            UpdateWizardStepVisibility();
            UpdateWizardNavigation();
        }

        QuantityTypeTextBox.Focus(FocusState.Programmatic);
        return (false, string.Empty, false);
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

    private void ApplyControlValue(Control control, object? rawValue)
    {
        var stringValue = rawValue?.ToString();
        if (control is TextBox textBox)
        {
            textBox.Text = stringValue ?? string.Empty;
            return;
        }

        if (control is NumberBox numberBox)
        {
            numberBox.Value = double.TryParse(
                stringValue,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var parsed
            )
                ? parsed
                : double.NaN;
            return;
        }

        if (control is CheckBox checkBox)
        {
            checkBox.IsChecked = bool.TryParse(stringValue, out var parsedBool) && parsedBool;
            return;
        }

        if (control is ComboBox comboBox)
        {
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

    private static string? GetControlValue(Control control)
    {
        if (control is TextBox textBox)
        {
            return string.IsNullOrWhiteSpace(textBox.Text) ? null : textBox.Text.Trim();
        }

        if (control is NumberBox numberBox)
        {
            return double.IsNaN(numberBox.Value)
                ? null
                : numberBox.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (control is CheckBox checkBox)
        {
            return checkBox.IsChecked == true ? "true" : "false";
        }

        if (control is ComboBox comboBox)
        {
            return comboBox.SelectedItem?.ToString();
        }

        return null;
    }

    private async Task<bool> TryCommitAsync()
    {
        RequestChooseExistingSpecs = false;
        RequestSaveQuantityTypeForFutureUse = false;
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

        var quantityTypeResolution = await ResolveQuantityTypeAsync();
        if (!quantityTypeResolution.IsResolved)
        {
            return false;
        }

        HomeLocation = HomeLocationTextBox.Text.Trim();
        ResolvedQuantityType = quantityTypeResolution.QuantityType;
        SelectedInventoryMethod = GetSelectedInventoryMethod();
        RequestSaveQuantityTypeForFutureUse = quantityTypeResolution.SaveForFutureUse;
        return true;
    }

    private async void OnFooterPrimaryButtonClick(object sender, RoutedEventArgs e)
    {
        if (await TryCommitAsync())
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
        StepQuantityTypePanel.Visibility =
            _currentWizardStep == QuantityTypeWizardStep
                ? Visibility.Visible
                : Visibility.Collapsed;
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
            2 => "Step 3 of 3 • Quantity Type",
            _ => string.Empty,
        };

        Bindings.Update();
    }

    private void QuantityTypeInputChanged(object sender, TextChangedEventArgs e)
    {
        RequestSaveQuantityTypeForFutureUse = false;
    }

    private void QuantityTypesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RequestSaveQuantityTypeForFutureUse = false;
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
