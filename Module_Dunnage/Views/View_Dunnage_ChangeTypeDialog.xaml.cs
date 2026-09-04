using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

/// <summary>
/// Modal used to move a dunnage part to a different dunnage type. Shows the spec
/// reconciliation (fields that will be carried over or added to the target type)
/// and collects values for required target fields the part does not already have.
/// </summary>
public sealed partial class View_Dunnage_ChangeTypeDialog : ContentDialog
{
    private readonly Model_DunnagePart _part;
    private readonly string _currentTypeName;
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly List<Model_DunnageType> _types;
    private readonly List<(string FieldName, Control Control)> _requiredInputs = new();
    private List<Model_CustomFieldDefinition> _sourceFields = new();
    private bool _isReconciling;

    public bool WasAccepted { get; private set; }

    public int? SelectedTargetTypeId => (TypesComboBox.SelectedItem as Model_DunnageType)?.Id;

    public IReadOnlyDictionary<string, string?> ProvidedValues { get; private set; } =
        new Dictionary<string, string?>();

    public View_Dunnage_ChangeTypeDialog(
        Model_DunnagePart part,
        string currentTypeName,
        List<Model_DunnageType> types,
        IService_MySQL_Dunnage dunnageService
    )
    {
        _part = part;
        _currentTypeName = currentTypeName;
        _types = types ?? new List<Model_DunnageType>();
        _dunnageService = dunnageService;

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);

        PartSummaryTextBlock.Text =
            $"Moving part '{part.PartId}' from type '{currentTypeName}'. "
            + "Saved label data and history entries for this part will be updated to the new type.";
        TypesComboBox.ItemsSource = _types;

        Loaded += async (_, _) => await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        _sourceFields = await LoadFieldsHydratedAsync(_part.TypeId);
        await ReconcileAsync();
    }

    private async Task<List<Model_CustomFieldDefinition>> LoadFieldsHydratedAsync(int typeId)
    {
        var fieldsResult = await _dunnageService.GetCustomFieldsByTypeAsync(typeId);
        var fields =
            fieldsResult.IsSuccess && fieldsResult.Data != null
                ? fieldsResult.Data
                : new List<Model_CustomFieldDefinition>();

        foreach (var field in fields)
        {
            if (
                string.Equals(field.FieldType, "Choices", StringComparison.OrdinalIgnoreCase)
                is false
                || field.Choices.Count > 0
            )
            {
                continue;
            }

            var choicesResult = await _dunnageService.GetCustomFieldChoicesAsync(field.Id);
            if (choicesResult.IsSuccess && choicesResult.Data != null)
            {
                field.Choices = choicesResult.Data
                    .OrderBy(choice => choice.SortOrder)
                    .Select(choice => choice.Choice)
                    .ToList();
            }
        }

        return fields;
    }

    private async void OnTypesSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await ReconcileAsync();
    }

    private async Task ReconcileAsync()
    {
        if (_isReconciling)
        {
            return;
        }

        _isReconciling = true;
        ClearError();
        RequiredFieldsPanel.Children.Clear();
        _requiredInputs.Clear();
        IsPrimaryButtonEnabled = false;

        try
        {
            var targetType = TypesComboBox.SelectedItem as Model_DunnageType;
            if (targetType is null)
            {
                ReconcileTextBlock.Text = "Select the dunnage type to move this part to.";
                return;
            }

            var targetFields = await LoadFieldsHydratedAsync(targetType.Id);
            var sourceValues = Helper_Dunnage_PartSpecs.ExtractUdc(_part);

            var targetNames = new HashSet<string>(
                targetFields.Select(field => field.FieldName),
                StringComparer.OrdinalIgnoreCase
            );

            var missingSourceFields = _sourceFields
                .Where(field => targetNames.Contains(field.FieldName) is false)
                .OrderBy(field => field.DisplayOrder)
                .ToList();

            var usedSlots = new HashSet<int>(
                targetFields
                    .Where(field => field.DisplayOrder is >= 1 and <= 10)
                    .Select(field => field.DisplayOrder)
            );
            var freeSlots = Enumerable
                .Range(1, Helper_Dunnage_PartSpecs.MaxUdcCount)
                .Where(slot => usedSlots.Contains(slot) is false)
                .ToList();

            var notes = new List<string>
            {
                $"'{targetType.TypeName}' has {targetFields.Count} spec field(s).",
            };

            if (missingSourceFields.Count == 0)
            {
                notes.Add("All of this part's specs exist on the target type and will be carried over.");
            }
            else if (missingSourceFields.Count <= freeSlots.Count)
            {
                notes.Add(
                    $"This part uses {missingSourceFields.Count} spec field(s) not defined on "
                    + $"'{targetType.TypeName}': {string.Join(", ", missingSourceFields.Select(f => f.FieldName))}. "
                    + "These will be added to the type (as optional) so the part keeps them. "
                    + "Adding them applies to every part of that type, so you may need to update those parts or labels afterward."
                );
            }
            else
            {
                ReconcileTextBlock.Text =
                    $"{targetType.TypeName} doesn't have enough open spec slots to hold this part's specs "
                    + $"(needs {missingSourceFields.Count}, has {freeSlots.Count} free). The transfer can't be completed.";
                return;
            }

            // Required target fields that still need a value (no source value, no default).
            var requiredToFill = targetFields
                .Where(field => field.IsRequired)
                .Where(field =>
                {
                    var hasSourceValue = _sourceFields.Any(sourceField =>
                        string.Equals(sourceField.FieldName, field.FieldName, StringComparison.OrdinalIgnoreCase)
                        && string.IsNullOrWhiteSpace(
                            Helper_Dunnage_PartSpecs.GetValueForSlot(
                                sourceValues,
                                sourceField.DisplayOrder
                            )
                        ) is false
                    );
                    return hasSourceValue is false
                        && string.IsNullOrWhiteSpace(field.DefaultValue);
                })
                .OrderBy(field => field.DisplayOrder)
                .ToList();

            if (requiredToFill.Count > 0)
            {
                notes.Add(
                    $"Required value(s) below must be filled before the part can be transferred."
                );

                foreach (var field in requiredToFill)
                {
                    var row = BuildRequiredRow(field);
                    if (row.Control is not null)
                    {
                        RequiredFieldsPanel.Children.Add(row.Label);
                        RequiredFieldsPanel.Children.Add(row.Control);
                        _requiredInputs.Add((field.FieldName, row.Control));
                    }
                }
            }
            else
            {
                notes.Add("No additional required values are needed for the target type.");
            }

            ReconcileTextBlock.Text = string.Join(" ", notes);
            IsPrimaryButtonEnabled = true;
        }
        finally
        {
            _isReconciling = false;
        }
    }

    private static (TextBlock Label, Control? Control) BuildRequiredRow(
        Model_CustomFieldDefinition field
    )
    {
        var requiredText = field.IsRequired ? " *" : string.Empty;
        var labelText = string.IsNullOrWhiteSpace(field.Unit)
            ? $"{field.FieldName}{requiredText}"
            : $"{field.FieldName} ({field.Unit}){requiredText}";

        var label = new TextBlock
        {
            Text = labelText,
            Margin = new Thickness(0, 6, 0, 0),
            Style = (Style?)Application.Current.Resources["CaptionTextBlockStyle"],
        };

        var control = CreateInputControl(field);
        return (label, control);
    }

    private static Control? CreateInputControl(Model_CustomFieldDefinition field)
    {
        if (string.Equals(field.FieldType, "Number", StringComparison.OrdinalIgnoreCase))
        {
            var numberBox = new NumberBox
            {
                PlaceholderText = "Required",
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
                HorizontalAlignment = HorizontalAlignment.Stretch,
                PlaceholderText = $"Select {field.FieldName.ToLowerInvariant()}",
            };

            foreach (var choice in field.Choices ?? new List<string>())
            {
                comboBox.Items.Add(choice);
            }

            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }

            return comboBox;
        }

        return new TextBox { PlaceholderText = "Required", MaxLength = 255 };
    }

    private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        ClearError();
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var (fieldName, control) in _requiredInputs)
        {
            var value = GetControlValue(control);
            if (string.IsNullOrWhiteSpace(value))
            {
                args.Cancel = true;
                ShowError($"'{fieldName}' is required before the part can be transferred.");
                return;
            }

            values[fieldName] = value;
        }

        var targetType = TypesComboBox.SelectedItem as Model_DunnageType;
        if (targetType is null || targetType.Id == _part.TypeId)
        {
            args.Cancel = true;
            ShowError("Choose a dunnage type different from the part's current type.");
            return;
        }

        // Give the UI a moment so validation does not feel instant.
        await Task.Yield();

        ProvidedValues = values;
        WasAccepted = true;
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

    private void ShowError(string message)
    {
        ErrorTextBlock.Text = message;
        ErrorTextBlock.Visibility = Visibility.Visible;
    }

    private void ClearError()
    {
        ErrorTextBlock.Visibility = Visibility.Collapsed;
        ErrorTextBlock.Text = string.Empty;
    }
}
