# Receiving - Add/Edit Package Type Modal

**SVG File**: `04-receiving-modal-add-type.svg`  
**Parent Page**: Receiving Settings  
**Type**: ContentDialog  
**Purpose**: Create or modify package type definitions

---

## WinUI 3 Implementation

### ContentDialog Structure

```xml
<ContentDialog
    x:Class="MTM_Receiving_Application.Dialogs.PackageTypeDialog"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="Add Package Type"
    PrimaryButtonText="Save"
    CloseButtonText="Cancel"
    DefaultButton="Primary"
    PrimaryButtonClick="SaveButton_Click"
    CloseButtonClick="CancelButton_Click">
    
    <StackPanel Spacing="16" MinWidth="400" Padding="0,8,0,0">
        <!-- Name -->
        <TextBox 
            x:Name="NameTextBox"
            Header="Name"
            Text="{x:Bind PackageType.Name, Mode=TwoWay}"
            PlaceholderText="e.g., Box, Pallet, Crate"
            MaxLength="50"/>
        
        <!-- Code -->
        <TextBox 
            x:Name="CodeTextBox"
            Header="Code"
            Text="{x:Bind PackageType.Code, Mode=TwoWay}"
            PlaceholderText="e.g., BOX, PLT, CRT"
            CharacterCasing="Upper"
            MaxLength="10"/>
        
        <!-- Active -->
        <ToggleSwitch 
            Header="Active"
            IsOn="{x:Bind PackageType.IsActive, Mode=TwoWay}"
            OnContent="Yes"
            OffContent="No"/>
    </StackPanel>
</ContentDialog>
```

---

## Code-Behind

```csharp
public sealed partial class PackageTypeDialog : ContentDialog
{
    public Model_PackageType PackageType { get; set; }
    
    public PackageTypeDialog()
    {
        InitializeComponent();
        PackageType = new Model_PackageType
        {
            IsActive = true
        };
    }
    
    public PackageTypeDialog(Model_PackageType existingType)
    {
        InitializeComponent();
        PackageType = existingType;
        Title = "Edit Package Type";
    }
    
    private void SaveButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(PackageType.Name))
        {
            args.Cancel = true;
            ShowValidationError("Name is required");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(PackageType.Code))
        {
            args.Cancel = true;
            ShowValidationError("Code is required");
            return;
        }
        
        if (!Regex.IsMatch(PackageType.Code, @"^[A-Z0-9]+$"))
        {
            args.Cancel = true;
            ShowValidationError("Code must be uppercase letters/numbers only");
            return;
        }
    }
    
    private void CancelButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // No action needed
    }
    
    private async void ShowValidationError(string message)
    {
        var errorDialog = new ContentDialog
        {
            Title = "Validation Error",
            Content = message,
            CloseButtonText = "OK"
        };
        await errorDialog.ShowAsync();
    }
}
```

---

## Usage in ViewModel

```csharp
[RelayCommand]
private async Task AddPackageTypeAsync()
{
    var dialog = new PackageTypeDialog
    {
        XamlRoot = _xamlRoot // Set from Page
    };
    
    var result = await dialog.ShowAsync();
    
    if (result == ContentDialogResult.Primary)
    {
        var saveResult = await _packageTypeService.SavePackageTypeAsync(dialog.PackageType);
        
        if (saveResult.IsSuccess)
        {
            await LoadPackageTypesAsync();
            StatusMessage = $"Package type '{dialog.PackageType.Name}' added successfully";
        }
        else
        {
            _errorHandler.ShowUserError(
                saveResult.ErrorMessage,
                "Save Failed",
                nameof(AddPackageTypeAsync)
            );
        }
    }
}

[RelayCommand]
private async Task EditPackageTypeAsync()
{
    if (SelectedPackageType == null) return;
    
    var dialog = new PackageTypeDialog(SelectedPackageType)
    {
        XamlRoot = _xamlRoot
    };
    
    var result = await dialog.ShowAsync();
    
    if (result == ContentDialogResult.Primary)
    {
        await _packageTypeService.SavePackageTypeAsync(dialog.PackageType);
        await LoadPackageTypesAsync();
    }
}
```

---

## Validation Rules

| Field | Required | Max Length | Pattern | Notes |
|-------|----------|------------|---------|-------|
| Name | ✅ Yes | 50 | Any | Unique across system |
| Code | ✅ Yes | 10 | `^[A-Z0-9]+$` | Uppercase only, unique |
| Active | N/A | N/A | Boolean | Default: true |

---

## Model

```csharp
public partial class Model_PackageType : ObservableObject
{
    [ObservableProperty]
    private int _id;
    
    [ObservableProperty]
    [property: Required]
    [property: MaxLength(50)]
    private string _name = string.Empty;
    
    [ObservableProperty]
    [property: Required]
    [property: MaxLength(10)]
    private string _code = string.Empty;
    
    [ObservableProperty]
    private bool _isActive = true;
    
    [ObservableProperty]
    private DateTime _createdDate = DateTime.Now;
}
```

---

## Database Operations

### Insert

```sql
INSERT INTO package_types (name, code, is_active, created_date)
VALUES (@Name, @Code, @IsActive, CURRENT_TIMESTAMP);
```

### Update

```sql
UPDATE package_types 
SET name = @Name,
    code = @Code,
    is_active = @IsActive,
    modified_date = CURRENT_TIMESTAMP
WHERE id = @Id;
```

### Validation (Check Unique)

```sql
SELECT COUNT(*) 
FROM package_types 
WHERE (name = @Name OR code = @Code) 
  AND id != @Id;
```

---

## Accessibility

```xml
<ContentDialog 
    AutomationProperties.Name="Package Type Editor"
    AutomationProperties.HelpText="Create or modify package type definitions">
    
    <TextBox 
        AutomationProperties.Name="Package Type Name"
        AutomationProperties.IsRequiredForForm="True"/>
    
    <TextBox 
        AutomationProperties.Name="Package Type Code"
        AutomationProperties.IsRequiredForForm="True"/>
</ContentDialog>
```

---

## Error Handling

### Duplicate Name/Code

```csharp
if (await _packageTypeService.ExistsAsync(PackageType.Name, PackageType.Code, PackageType.Id))
{
    args.Cancel = true;
    ShowValidationError("A package type with this name or code already exists");
    return;
}
```

### Database Errors

```csharp
var saveResult = await _packageTypeService.SavePackageTypeAsync(dialog.PackageType);

if (!saveResult.IsSuccess)
{
    _errorHandler.ShowUserError(
        "Failed to save package type. Please try again.",
        "Database Error",
        nameof(AddPackageTypeAsync)
    );
}
```

---

## Keyboard Shortcuts

- **Enter** - Triggers Primary (Save) button when focused on fields
- **Escape** - Closes dialog without saving
- **Tab** - Navigate between fields

---

## References

- [WinUI 3 Gallery - ContentDialog](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/ContentDialogPage.xaml)
- [ContentDialog Class](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.contentdialog)
