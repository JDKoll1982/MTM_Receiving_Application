# Receiving - Delete Package Type Confirmation

**SVG File**: `04-receiving-modal-delete-confirm.svg`  
**Parent Page**: Receiving Settings  
**Type**: ContentDialog (Confirmation)  
**Purpose**: Confirm deletion of package type

---

## WinUI 3 Implementation

```xml
<!-- Simple confirmation dialog -->
<ContentDialog
    Title="Delete Package Type"
    Content="{x:Bind DeleteConfirmationMessage}"
    PrimaryButtonText="Delete"
    CloseButtonText="Cancel"
    DefaultButton="Close"
    PrimaryButtonStyle="{StaticResource DangerButtonStyle}"/>
```

---

## Usage in ViewModel

```csharp
[RelayCommand]
private async Task DeletePackageTypeAsync()
{
    if (SelectedPackageType == null) return;
    
    // Check if in use
    var usageCheck = await _packageTypeService.IsPackageTypeInUseAsync(SelectedPackageType.Id);
    
    if (usageCheck.IsSuccess && usageCheck.Data > 0)
    {
        _errorHandler.ShowUserError(
            $"Cannot delete '{SelectedPackageType.Name}' because it is being used by {usageCheck.Data} package(s).",
            "Package Type In Use",
            nameof(DeletePackageTypeAsync)
        );
        return;
    }
    
    var confirm = new ContentDialog
    {
        Title = "Delete Package Type",
        Content = $"Delete package type \"{SelectedPackageType.Name}\"?\n\nThis action cannot be undone.",
        PrimaryButtonText = "Delete",
        CloseButtonText = "Cancel",
        DefaultButton = ContentDialogButton.Close,
        XamlRoot = _xamlRoot
    };
    
    // Style primary button as danger
    confirm.PrimaryButtonStyle = Application.Current.Resources["DangerButtonStyle"] as Style;
    
    var result = await confirm.ShowAsync();
    
    if (result == ContentDialogResult.Primary)
    {
        var deleteResult = await _packageTypeService.DeletePackageTypeAsync(SelectedPackageType.Id);
        
        if (deleteResult.IsSuccess)
        {
            StatusMessage = $"Package type '{SelectedPackageType.Name}' deleted";
            await LoadPackageTypesAsync();
        }
        else
        {
            _errorHandler.ShowUserError(
                deleteResult.ErrorMessage,
                "Delete Failed",
                nameof(DeletePackageTypeAsync)
            );
        }
    }
}
```

---

## Danger Button Style

```xml
<Style x:Key="DangerButtonStyle" TargetType="Button" BasedOn="{StaticResource AccentButtonStyle}">
    <Setter Property="Background" Value="{ThemeResource SystemFillColorCriticalBrush}"/>
    <Setter Property="Foreground" Value="White"/>
</Style>
```

---

## Service Validation

```csharp
public async Task<Model_Dao_Result<int>> IsPackageTypeInUseAsync(int packageTypeId)
{
    try
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_package_type_id", packageTypeId }
        };
        
        var result = await Helper_Database_StoredProcedure.ExecuteScalarAsync<int>(
            "sp_package_type_usage_count",
            parameters
        );
        
        return Model_Dao_Result<int>.Success(result);
    }
    catch (Exception ex)
    {
        return Model_Dao_Result<int>.Failure($"Error checking usage: {ex.Message}");
    }
}
```

---

## Database Query

```sql
DELIMITER //
CREATE PROCEDURE sp_package_type_usage_count(
    IN p_package_type_id INT
)
BEGIN
    SELECT COUNT(*) 
    FROM receiving_packages 
    WHERE package_type_id = p_package_type_id;
END//
DELIMITER ;
```

---

## References

- [ContentDialog Confirmation Pattern](https://learn.microsoft.com/en-us/windows/apps/design/controls/dialogs-and-flyouts/dialogs)
