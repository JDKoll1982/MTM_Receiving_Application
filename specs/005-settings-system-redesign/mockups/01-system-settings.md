# System Settings Page - WinUI 3 Controls

**SVG File**: `01-system-settings.svg`  
**Category**: System Settings  
**Purpose**: Configure environment, database, and logging settings

---

## WinUI 3 Controls Used

### Page Structure

```xml
<Page>
    <Grid RowDefinitions="Auto,*,Auto">
        <!-- Header -->
        <!-- ScrollViewer with content -->
        <!-- Footer buttons -->
    </Grid>
</Page>
```

---

## Control Breakdown

### 1. **Header Section**

**Controls**:

- `TextBlock` - Page title
  - Text: "System Settings"
  - Style: `TitleTextBlockStyle`
  - FontSize: 28
- `TextBlock` - Subtitle
  - Text: "Environment, Database, and Developer Configuration"
  - Style: `CaptionTextBlockStyle`

```xml
<StackPanel Background="{ThemeResource CardBackgroundFillColorDefaultBrush}" Padding="40,20">
    <TextBlock Text="System Settings" Style="{StaticResource TitleTextBlockStyle}"/>
    <TextBlock Text="Environment, Database, and Developer Configuration" 
               Style="{StaticResource CaptionTextBlockStyle}"
               Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
</StackPanel>
```

---

### 2. **Development & Testing Section**

#### Use Mock Data - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Use Infor Visual Mock Data"
    IsOn="{x:Bind ViewModel.UseInforVisualMockData, Mode=TwoWay}"
    OnContent="On"
    OffContent="Off">
    <ToggleSwitch.Description>
        <TextBlock Text="Enable mock data instead of querying Infor Visual database"/>
    </ToggleSwitch.Description>
</ToggleSwitch>
```

**Properties**:

- Data type: `boolean`
- Default: `false`
- Permission: Developer

---

#### Default Mock PO - `TextBox`

```xml
<TextBox 
    Header="Default Mock PO Number"
    PlaceholderText="Enter PO number"
    Text="{x:Bind ViewModel.DefaultMockPONumber, Mode=TwoWay}"
    Width="300"
    HorizontalAlignment="Left">
    <TextBox.Description>
        <TextBlock Text="PO number used when mock mode is enabled"/>
    </TextBox.Description>
</TextBox>
```

**Properties**:

- Data type: `string`
- Default: `"PO-066868"`
- Validation: Regex pattern `^PO-\d+$`
- Permission: Developer

---

#### Environment - `ComboBox`

```xml
<ComboBox 
    Header="Environment"
    SelectedItem="{x:Bind ViewModel.Environment, Mode=TwoWay}"
    Width="300"
    HorizontalAlignment="Left">
    <ComboBox.Description>
        <TextBlock Text="Current deployment environment"/>
    </ComboBox.Description>
    <ComboBoxItem Content="Development"/>
    <ComboBoxItem Content="Production" IsSelected="True"/>
</ComboBox>
```

**Properties**:

- Data type: `string`
- Allowed values: `["Development", "Production"]`
- Default: `"Production"`
- Permission: Super Admin

---

### 3. **Database Configuration Section**

#### Database Max Retries - `NumberBox`

```xml
<NumberBox 
    Header="Database Max Retries"
    Value="{x:Bind ViewModel.DatabaseMaxRetries, Mode=TwoWay}"
    Minimum="1"
    Maximum="10"
    SpinButtonPlacementMode="Inline"
    Width="150"
    HorizontalAlignment="Left">
    <NumberBox.Description>
        <TextBlock Text="Maximum retry attempts for transient failures (1-10)"/>
    </NumberBox.Description>
</NumberBox>
```

**Properties**:

- Data type: `int`
- Range: 1-10
- Default: `3`
- Permission: Developer

---

#### Retry Delays - `TextBox`

```xml
<TextBox 
    Header="Database Retry Delays (ms)"
    Text="{x:Bind ViewModel.DatabaseRetryDelaysMs, Mode=TwoWay}"
    PlaceholderText="e.g., 100,200,400"
    Width="300"
    HorizontalAlignment="Left">
    <TextBox.Description>
        <TextBlock Text="Comma-separated delay values in milliseconds"/>
    </TextBox.Description>
</TextBox>
```

**Properties**:

- Data type: `string`
- Default: `"100,200,400"`
- Validation: Comma-separated integers
- Permission: Developer

---

### 4. **Logging & Debugging Section**

#### Enable Console Logging - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Enable Console Logging"
    IsOn="{x:Bind ViewModel.EnableConsoleLogging, Mode=TwoWay}"
    OnContent="On"
    OffContent="Off">
    <ToggleSwitch.Description>
        <TextBlock Text="Write logs to console output"/>
    </ToggleSwitch.Description>
</ToggleSwitch>
```

**Properties**:

- Data type: `boolean`
- Default: `true`
- Permission: Developer

---

#### Enable File Logging - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Enable File Logging"
    IsOn="{x:Bind ViewModel.EnableFileLogging, Mode=TwoWay}"
    OnContent="On"
    OffContent="Off">
    <ToggleSwitch.Description>
        <TextBlock Text="Write logs to file system"/>
    </ToggleSwitch.Description>
</ToggleSwitch>
```

**Properties**:

- Data type: `boolean`
- Default: `true`
- Permission: Developer

---

#### Minimum Log Level - `ComboBox`

```xml
<ComboBox 
    Header="Minimum Log Level"
    SelectedItem="{x:Bind ViewModel.MinimumLogLevel, Mode=TwoWay}"
    Width="200"
    HorizontalAlignment="Left">
    <ComboBox.Description>
        <TextBlock Text="Filter logs by severity"/>
    </ComboBox.Description>
    <ComboBoxItem Content="Trace"/>
    <ComboBoxItem Content="Debug"/>
    <ComboBoxItem Content="Information" IsSelected="True"/>
    <ComboBoxItem Content="Warning"/>
    <ComboBoxItem Content="Error"/>
    <ComboBoxItem Content="Critical"/>
</ComboBox>
```

**Properties**:

- Data type: `string`
- Allowed values: `["Trace", "Debug", "Information", "Warning", "Error", "Critical"]`
- Default: `"Information"`
- Permission: Developer

---

#### Max Log Files - `NumberBox`

```xml
<NumberBox 
    Header="Max Log Files to Retain"
    Value="{x:Bind ViewModel.MaxLogFilesToRetain, Mode=TwoWay}"
    Minimum="1"
    Maximum="30"
    SpinButtonPlacementMode="Inline"
    Width="150"
    HorizontalAlignment="Left">
    <NumberBox.Description>
        <TextBlock Text="Number of archived log files (1-30)"/>
    </NumberBox.Description>
</NumberBox>
```

**Properties**:

- Data type: `int`
- Range: 1-30
- Default: `7`
- Permission: Developer

---

### 5. **Footer Actions**

#### Save Button - `Button`

```xml
<Button 
    Content="Save"
    Style="{StaticResource AccentButtonStyle}"
    Command="{x:Bind ViewModel.SaveCommand}"
    Width="100"/>
```

#### Cancel Button - `Button`

```xml
<Button 
    Content="Cancel"
    Command="{x:Bind ViewModel.CancelCommand}"
    Width="100"
    Margin="0,0,10,0"/>
```

#### Auto-save Indicator - `TextBlock`

```xml
<TextBlock 
    Text="Changes auto-save after 500ms"
    Style="{StaticResource CaptionTextBlockStyle}"
    Foreground="{ThemeResource TextFillColorSecondaryBrush}"
    VerticalAlignment="Center"
    Margin="40,0,0,0"/>
```

---

## Layout Pattern

### Two-Column Grid

```xml
<Grid ColumnDefinitions="*,*" ColumnSpacing="40" Padding="40,20">
    <!-- Left column: Dev/Test + Database -->
    <StackPanel Grid.Column="0" Spacing="24">
        <StackPanel Spacing="16">
            <TextBlock Text="Development &amp; Testing" 
                       Style="{StaticResource SubtitleTextBlockStyle}"/>
            <!-- Controls -->
        </StackPanel>
        
        <StackPanel Spacing="16">
            <TextBlock Text="Database Configuration" 
                       Style="{StaticResource SubtitleTextBlockStyle}"/>
            <!-- Controls -->
        </StackPanel>
    </StackPanel>
    
    <!-- Right column: Logging -->
    <StackPanel Grid.Column="1" Spacing="16">
        <TextBlock Text="Logging &amp; Debugging" 
                   Style="{StaticResource SubtitleTextBlockStyle}"/>
        <!-- Controls -->
    </StackPanel>
</Grid>
```

---

## Data Binding

### ViewModel Properties

```csharp
public partial class SystemSettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool _useInforVisualMockData = false;
    
    [ObservableProperty]
    private string _defaultMockPONumber = "PO-066868";
    
    [ObservableProperty]
    private string _environment = "Production";
    
    [ObservableProperty]
    private int _databaseMaxRetries = 3;
    
    [ObservableProperty]
    private string _databaseRetryDelaysMs = "100,200,400";
    
    [ObservableProperty]
    private bool _enableConsoleLogging = true;
    
    [ObservableProperty]
    private bool _enableFileLogging = true;
    
    [ObservableProperty]
    private string _minimumLogLevel = "Information";
    
    [ObservableProperty]
    private int _maxLogFilesToRetain = 7;
}
```

---

## Validation

### TextBox Validation (Default Mock PO)

```csharp
partial void OnDefaultMockPONumberChanged(string value)
{
    var regex = new Regex(@"^PO-\d+$");
    if (!regex.IsMatch(value))
    {
        // Show validation error
        ValidationMessage = "PO number must match pattern: PO-######";
    }
}
```

### NumberBox Validation (Auto-handled)

- Built-in min/max validation
- Spin buttons enforce range
- Invalid input reverts to last valid value

---

## Auto-Save Implementation

```csharp
private DispatcherTimer _autoSaveTimer;

public SystemSettingsViewModel()
{
    _autoSaveTimer = new DispatcherTimer
    {
        Interval = TimeSpan.FromMilliseconds(500)
    };
    _autoSaveTimer.Tick += AutoSave_Tick;
}

partial void OnUseInforVisualMockDataChanged(bool value)
{
    _autoSaveTimer.Stop();
    _autoSaveTimer.Start();
}

private async void AutoSave_Tick(object sender, object e)
{
    _autoSaveTimer.Stop();
    await SaveSettingsAsync();
}
```

---

## Permission Enforcement

All settings in this page require **Developer** or **Super Admin** role:

```csharp
public override async Task OnNavigatedToAsync(object parameter)
{
    if (!_authService.HasPermission(Enum_PermissionLevel.Developer))
    {
        _errorHandler.ShowUserError(
            "You do not have permission to access System Settings.",
            "Access Denied",
            nameof(OnNavigatedToAsync)
        );
        _navigationService.GoBack();
    }
}
```

---

## Accessibility

```xml
<!-- ToggleSwitch -->
<ToggleSwitch 
    AutomationProperties.Name="Use Infor Visual Mock Data"
    AutomationProperties.HelpText="Enable mock data instead of querying Infor Visual database"/>

<!-- NumberBox -->
<NumberBox 
    AutomationProperties.Name="Database Max Retries"
    AutomationProperties.HelpText="Maximum retry attempts, range 1 to 10"/>

<!-- ComboBox -->
<ComboBox 
    AutomationProperties.Name="Environment Selection"
    AutomationProperties.HelpText="Select deployment environment"/>
```

---

## Settings Persistence

Settings are saved to database `system_settings` table:

```csharp
public async Task<Model_Dao_Result> SaveSettingsAsync()
{
    var settings = new List<(string key, string value)>
    {
        ("UseInforVisualMockData", UseInforVisualMockData.ToString()),
        ("DefaultMockPONumber", DefaultMockPONumber),
        ("Environment", Environment),
        ("DatabaseMaxRetries", DatabaseMaxRetries.ToString()),
        ("DatabaseRetryDelaysMs", DatabaseRetryDelaysMs),
        ("EnableConsoleLogging", EnableConsoleLogging.ToString()),
        ("EnableFileLogging", EnableFileLogging.ToString()),
        ("MinimumLogLevel", MinimumLogLevel),
        ("MaxLogFilesToRetain", MaxLogFilesToRetain.ToString())
    };
    
    return await _settingsService.SaveSystemSettingsAsync(settings);
}
```

---

## References

- [WinUI 3 Gallery - ToggleSwitch](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/ToggleSwitchPage.xaml)
- [WinUI 3 Gallery - NumberBox](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/NumberBoxPage.xaml)
- [WinUI 3 Gallery - ComboBox](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/ComboBoxPage.xaml)
- [WinUI 3 Gallery - TextBox](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/TextBoxPage.xaml)
