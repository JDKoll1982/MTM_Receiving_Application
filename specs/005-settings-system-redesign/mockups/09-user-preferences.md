# User Preferences - WinUI 3 Controls

**SVG File**: `09-user-preferences.svg`
**Category**: User Preferences
**Purpose**: Personalization and UI settings

---

## WinUI 3 Controls Used

### 1. **Appearance Section**

#### Theme - `ComboBox`
```xml
<ComboBox
    Header="Theme"
    SelectedItem="{x:Bind ViewModel.UserTheme, Mode=TwoWay}"
    Width="200">
    <ComboBoxItem Content="System Default" IsSelected="True"/>
    <ComboBoxItem Content="Light"/>
    <ComboBoxItem Content="Dark"/>
</ComboBox>
```

#### Font Size - `NumberBox`
```xml
<NumberBox
    Header="Font Size"
    Value="{x:Bind ViewModel.UserFontSize, Mode=TwoWay}"
    Minimum="10"
    Maximum="18"
    SpinButtonPlacementMode="Inline"
    Width="150"/>
```

#### Use Compact Mode - `ToggleSwitch`
```xml
<ToggleSwitch
    Header="Use Compact Mode"
    IsOn="{x:Bind ViewModel.UserCompactMode, Mode=TwoWay}"/>
```

---

### 2. **Behavior Section**

#### Enable Sound Effects - `ToggleSwitch`
```xml
<ToggleSwitch
    Header="Enable Sound Effects"
    IsOn="{x:Bind ViewModel.UserEnableSounds, Mode=TwoWay}"/>
```

#### Confirm Destructive Actions - `ToggleSwitch`
```xml
<ToggleSwitch
    Header="Confirm Destructive Actions"
    IsOn="{x:Bind ViewModel.UserConfirmDestructiveActions, Mode=TwoWay}"/>
```

---

### 3. **Data Display Section**

#### Date Format - `ComboBox`
```xml
<ComboBox
    Header="Date Format"
    SelectedItem="{x:Bind ViewModel.UserDateFormat, Mode=TwoWay}"
    Width="250">
    <ComboBoxItem Content="MM/DD/YYYY" IsSelected="True"/>
    <ComboBoxItem Content="DD/MM/YYYY"/>
    <ComboBoxItem Content="YYYY-MM-DD"/>
</ComboBox>
```

#### Time Format - `ComboBox`
```xml
<ComboBox
    Header="Time Format"
    SelectedItem="{x:Bind ViewModel.UserTimeFormat, Mode=TwoWay}"
    Width="200">
    <ComboBoxItem Content="12-hour (AM/PM)" IsSelected="True"/>
    <ComboBoxItem Content="24-hour"/>
</ComboBox>
```

#### Rows Per Page - `NumberBox`
```xml
<NumberBox
    Header="Rows Per Page"
    Value="{x:Bind ViewModel.UserRowsPerPage, Mode=TwoWay}"
    Minimum="10"
    Maximum="100"
    SpinButtonPlacementMode="Inline"
    Width="150"/>
```

#### Show Tooltips - `ToggleSwitch`
```xml
<ToggleSwitch
    Header="Show Tooltips"
    IsOn="{x:Bind ViewModel.UserShowTooltips, Mode=TwoWay}"/>
```

---

### 4. **Reset to Defaults Button**

```xml
<Button
    Content="Reset to Defaults"
    Command="{x:Bind ViewModel.ResetPreferencesCommand}"
    Foreground="{ThemeResource SystemFillColorCriticalBrush}"
    BorderBrush="{ThemeResource SystemFillColorCriticalBrush}"
    Width="180"
    Height="36"/>
```

---

## ViewModel

```csharp
public partial class UserPreferencesViewModel : BaseViewModel
{
    // Appearance
    [ObservableProperty]
    private string _userTheme = "System Default";

    [ObservableProperty]
    private int _userFontSize = 14;

    [ObservableProperty]
    private bool _userCompactMode = false;

    // Behavior
    [ObservableProperty]
    private bool _userEnableSounds = true;

    [ObservableProperty]
    private bool _userConfirmDestructiveActions = true;

    // Data Display
    [ObservableProperty]
    private string _userDateFormat = "MM/DD/YYYY";

    [ObservableProperty]
    private string _userTimeFormat = "12-hour (AM/PM)";

    [ObservableProperty]
    private int _userRowsPerPage = 25;

    [ObservableProperty]
    private bool _userShowTooltips = true;

    [RelayCommand]
    private async Task ResetPreferencesAsync()
    {
        var confirm = new ContentDialog
        {
            Title = "Reset Preferences",
            Content = "Reset all preferences to default values?",
            PrimaryButtonText = "Reset",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close
        };

        if (await confirm.ShowAsync() == ContentDialogResult.Primary)
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
        }
    }
}
```

---

## Theme Application

```csharp
partial void OnUserThemeChanged(string value)
{
    var requestedTheme = value switch
    {
        "Light" => ElementTheme.Light,
        "Dark" => ElementTheme.Dark,
        _ => ElementTheme.Default
    };

    if (App.Current.Window.Content is FrameworkElement rootElement)
    {
        rootElement.RequestedTheme = requestedTheme;
    }
}
```

---

## Permissions

All user preferences are **per-user** and available to all roles.

---

## Persistence

User preferences are stored per-user in the `user_preferences` table:

```sql
CREATE TABLE settings_dunnage_personal (
    user_id INT PRIMARY KEY,
    theme VARCHAR(20),
    font_size INT,
    compact_mode BOOLEAN,
    enable_sounds BOOLEAN,
    confirm_destructive BOOLEAN,
    date_format VARCHAR(20),
    time_format VARCHAR(20),
    rows_per_page INT,
    show_tooltips BOOLEAN,
    modified_date DATETIME
);
```

---

## References
- [WinUI 3 Gallery - Theme Resources](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/ThemeResourcesPage.xaml)
