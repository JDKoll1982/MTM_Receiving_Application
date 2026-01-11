# User Preferences - Reset Confirmation

**SVG File**: `09-preferences-modal-reset.svg`  
**Parent Page**: User Preferences  
**Type**: ContentDialog (Confirmation)  
**Purpose**: Confirm reset of all user preferences to defaults

---

## WinUI 3 Implementation

```xml
<ContentDialog
    Title="Reset Preferences"
    Content="Reset all preferences to default values?&#x0a;&#x0a;Your current theme, font size, and display settings will be restored to system defaults."
    PrimaryButtonText="Reset"
    CloseButtonText="Cancel"
    DefaultButton="Close"/>
```

---

## Usage in ViewModel

```csharp
[RelayCommand]
private async Task ResetPreferencesAsync()
{
    var confirm = new ContentDialog
    {
        Title = "Reset Preferences",
        Content = "Reset all preferences to default values?\n\nYour current theme, font size, and display settings will be restored to system defaults.",
        PrimaryButtonText = "Reset",
        CloseButtonText = "Cancel",
        DefaultButton = ContentDialogButton.Close,
        XamlRoot = _xamlRoot
    };
    
    var result = await confirm.ShowAsync();
    
    if (result == ContentDialogResult.Primary)
    {
        // Reset to defaults
        UserTheme = "System Default";
        UserFontSize = 14;
        UserCompactMode = false;
        UserEnableSounds = true;
        UserConfirmDestructiveActions = true;
        UserDateFormat = "MM/DD/YYYY";
        UserTimeFormat = "12-hour (AM/PM)";
        UserRowsPerPage = 25;
        UserShowTooltips = true;
        
        await SavePreferencesAsync();
        
        StatusMessage = "Preferences reset to defaults";
        
        // Show success notification
        var success = new ContentDialog
        {
            Title = "Reset Complete",
            Content = "Your preferences have been reset to default values.",
            CloseButtonText = "OK",
            XamlRoot = _xamlRoot
        };
        await success.ShowAsync();
    }
}
```

---

## Default Values

```csharp
public static class UserPreferenceDefaults
{
    public const string Theme = "System Default";
    public const int FontSize = 14;
    public const bool CompactMode = false;
    public const bool EnableSounds = true;
    public const bool ConfirmDestructiveActions = true;
    public const string DateFormat = "MM/DD/YYYY";
    public const string TimeFormat = "12-hour (AM/PM)";
    public const int RowsPerPage = 25;
    public const bool ShowTooltips = true;
}
```

---

## References

- [ContentDialog Simple Confirmation](https://learn.microsoft.com/en-us/windows/apps/design/controls/dialogs-and-flyouts/dialogs)
