using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

public partial class ViewModel_Dunnage_QuickAddTypeDialog : ViewModel_Shared_Base
{
    [ObservableProperty]
    private string _dialogTitle = "Add New Dunnage Type";

    [ObservableProperty]
    private string _dialogDescription = "Enter details for the new dunnage type.";

    [ObservableProperty]
    private string _primaryButtonText = "Add Type";

    [ObservableProperty]
    private string _typeName = string.Empty;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private MaterialIconKind _selectedIconKind = MaterialIconKind.PackageVariantClosed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedImage))]
    [NotifyPropertyChangedFor(nameof(SelectedImageSource))]
    [NotifyPropertyChangedFor(nameof(SelectedImagePathForSave))]
    private string? _selectedImagePath;

    private bool _isImageMode;

    public bool IsImageMode
    {
        get => _isImageMode;
        set
        {
            if (SetProperty(ref _isImageMode, value))
            {
                OnPropertyChanged(nameof(UseIconVisual));
                OnPropertyChanged(nameof(UseImageVisual));
                OnPropertyChanged(nameof(SelectedImagePathForSave));
            }
        }
    }

    [ObservableProperty]
    private ObservableCollection<Model_SpecItem> _specs = new();

    [ObservableProperty]
    private ObservableCollection<string> _currentChoices = new();

    [ObservableProperty]
    private string _newSpecName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNumberOptions))]
    [NotifyPropertyChangedFor(nameof(ShowChoicesOptions))]
    private string _newSpecType = "Text";

    [ObservableProperty]
    private bool _newSpecRequired;

    [ObservableProperty]
    private string _newSpecUnit = string.Empty;

    [ObservableProperty]
    private double _newSpecMinValue = double.NaN;

    [ObservableProperty]
    private double _newSpecMaxValue = double.NaN;

    [ObservableProperty]
    private string _newSpecChoice = string.Empty;

    public ViewModel_Dunnage_QuickAddTypeDialog(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService) { }

    public bool UseIconVisual => !IsImageMode;

    public bool UseImageVisual => IsImageMode;

    public bool HasSelectedImage => string.IsNullOrWhiteSpace(SelectedImagePath) is false;

    public bool HasValidationMessage => string.IsNullOrWhiteSpace(ValidationMessage) is false;

    public bool ShowNumberOptions =>
        string.Equals(NewSpecType, "Number", StringComparison.OrdinalIgnoreCase);

    public bool ShowChoicesOptions =>
        string.Equals(NewSpecType, "Choices", StringComparison.OrdinalIgnoreCase);

    public ImageSource? SelectedImageSource =>
        Helpers.Helper_DunnageImagePaths.CreateImageSource(SelectedImagePath);

    public string SelectedImageFileName =>
        string.IsNullOrWhiteSpace(SelectedImagePath)
            ? string.Empty
            : Path.GetFileName(SelectedImagePath);

    public string? SelectedImagePathForSave => UseImageVisual ? SelectedImagePath : null;

    partial void OnSelectedImagePathChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) is false && !IsImageMode)
        {
            IsImageMode = true;
        }

        OnPropertyChanged(nameof(SelectedImageFileName));
    }

    public void InitializeForCreate()
    {
        DialogTitle = "Add New Dunnage Type";
        DialogDescription = "Enter details for the new dunnage type.";
        PrimaryButtonText = "Add Type";
        TypeName = string.Empty;
        ValidationMessage = string.Empty;
        SelectedIconKind = MaterialIconKind.PackageVariantClosed;
        SelectedImagePath = null;
        IsImageMode = false;
        Specs.Clear();
        ResetSpecEditor();
    }

    public void InitializeForEdit(
        string typeName,
        string iconName,
        string? imagePath,
        List<Model_CustomFieldDefinition> customFields
    )
    {
        DialogTitle = "Edit Dunnage Type";
        DialogDescription = "Update the type name, visual mode, and custom fields.";
        PrimaryButtonText = "Save Changes";
        TypeName = typeName;
        ValidationMessage = string.Empty;
        SelectedImagePath = imagePath;
        IsImageMode = !string.IsNullOrWhiteSpace(imagePath);

        if (!Enum.TryParse(iconName, true, out MaterialIconKind parsedIcon))
        {
            parsedIcon = MaterialIconKind.PackageVariantClosed;
        }

        SelectedIconKind = parsedIcon;

        Specs.Clear();
        foreach (var field in customFields.OrderBy(field => field.DisplayOrder))
        {
            Specs.Add(Helper_Dunnage_PartSpecs.CreateSpecItem(field));
        }

        ResetSpecEditor();
    }

    public bool TryCommit()
    {
        ValidationMessage = string.Empty;
        string trimmedName = TypeName.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            ValidationMessage = "Type name is required.";
            return false;
        }

        if (!char.IsUpper(trimmedName[0]))
        {
            ValidationMessage = "Name must start with a capital letter.";
            return false;
        }

        if (UseImageVisual && string.IsNullOrWhiteSpace(SelectedImagePath))
        {
            ValidationMessage = "Choose a PNG or JPG image or switch back to icon mode.";
            return false;
        }

        TypeName = trimmedName;
        return true;
    }

    [RelayCommand]
    private void AddChoice()
    {
        string trimmedChoice = NewSpecChoice.Trim();
        if (string.IsNullOrWhiteSpace(trimmedChoice))
        {
            return;
        }

        if (
            CurrentChoices.Any(choice =>
                choice.Equals(trimmedChoice, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return;
        }

        CurrentChoices.Add(trimmedChoice);
        NewSpecChoice = string.Empty;
    }

    [RelayCommand]
    private void RemoveChoice(string? choice)
    {
        if (string.IsNullOrWhiteSpace(choice))
        {
            return;
        }

        CurrentChoices.Remove(choice);
    }

    [RelayCommand]
    private void AddSpec()
    {
        string trimmedName = NewSpecName.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            ValidationMessage = "Enter a specification field name before adding it.";
            return;
        }

        if (Specs.Any(spec => spec.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase)))
        {
            ValidationMessage = $"'{trimmedName}' is already defined for this type.";
            return;
        }

        if (ShowChoicesOptions && CurrentChoices.Count == 0)
        {
            ValidationMessage = "Add at least one choice value for a Choices field.";
            return;
        }

        Specs.Add(
            new Model_SpecItem
            {
                Name = trimmedName,
                DataType = NewSpecType,
                IsRequired = NewSpecRequired,
                Unit = ShowNumberOptions ? NewSpecUnit.Trim() : string.Empty,
                MinValue =
                    ShowNumberOptions && !double.IsNaN(NewSpecMinValue) ? NewSpecMinValue : null,
                MaxValue =
                    ShowNumberOptions && !double.IsNaN(NewSpecMaxValue) ? NewSpecMaxValue : null,
                Choices = ShowChoicesOptions ? CurrentChoices.ToList() : new List<string>(),
            }
        );

        ValidationMessage = string.Empty;
        ResetSpecEditor();
    }

    [RelayCommand]
    private void RemoveSpec(Model_SpecItem? spec)
    {
        if (spec is null)
        {
            return;
        }

        Specs.Remove(spec);
    }

    private void ResetSpecEditor()
    {
        NewSpecName = string.Empty;
        NewSpecType = "Text";
        NewSpecRequired = false;
        NewSpecUnit = string.Empty;
        NewSpecMinValue = double.NaN;
        NewSpecMaxValue = double.NaN;
        NewSpecChoice = string.Empty;
        CurrentChoices.Clear();
    }
}
