# Reporting Settings - WinUI 3 Controls

**SVG File**: `08-reporting.svg`  
**Category**: Reporting  
**Purpose**: Export formats and scheduled reports configuration

---

## WinUI 3 Controls Used

### 1. **Export Options**

#### Default Export Format - `ComboBox`

```xml
<ComboBox 
    Header="Default Export Format"
    SelectedItem="{x:Bind ViewModel.ReportingDefaultExportFormat, Mode=TwoWay}"
    Width="250">
    <ComboBoxItem Content="Excel (.xlsx)" IsSelected="True"/>
    <ComboBoxItem Content="CSV (.csv)"/>
    <ComboBoxItem Content="PDF (.pdf)"/>
</ComboBox>
```

#### Export Save Location - `TextBox` with Browse

```xml
<StackPanel Orientation="Horizontal" Spacing="8">
    <TextBox 
        Header="Export Save Location"
        Text="{x:Bind ViewModel.ReportingExportLocation, Mode=TwoWay}"
        Width="450"
        IsReadOnly="True"/>
    <Button 
        Content="..." 
        Command="{x:Bind ViewModel.BrowseExportFolderCommand}"
        VerticalAlignment="Bottom"
        Width="32"/>
</StackPanel>
```

#### Auto-open After Export - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Auto-open After Export"
    IsOn="{x:Bind ViewModel.ReportingAutoOpenExports, Mode=TwoWay}"/>
```

---

### 2. **Scheduled Reports**

#### Enable Scheduled Reports - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Enable Scheduled Reports"
    IsOn="{x:Bind ViewModel.ReportingEnableScheduled, Mode=TwoWay}"/>
```

#### Default Email Recipients - `TextBox`

```xml
<TextBox 
    Header="Default Email Recipients"
    Text="{x:Bind ViewModel.ReportingDefaultEmailRecipients, Mode=TwoWay}"
    PlaceholderText="email1@example.com, email2@example.com"
    Width="450"/>
```

---

## ViewModel

```csharp
public partial class ReportingSettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _reportingDefaultExportFormat = "Excel (.xlsx)";
    
    [ObservableProperty]
    private string _reportingExportLocation = @"C:\Users\johnk\Documents\MTM Reports";
    
    [ObservableProperty]
    private bool _reportingAutoOpenExports = true;
    
    [ObservableProperty]
    private bool _reportingEnableScheduled = true;
    
    [ObservableProperty]
    private string _reportingDefaultEmailRecipients = "management@mtm.com, accounting@mtm.com";
}
```

---

## References

- [WinUI 3 Gallery - FolderPicker](https://learn.microsoft.com/en-us/windows/uwp/files/quickstart-using-file-and-folder-pickers)
