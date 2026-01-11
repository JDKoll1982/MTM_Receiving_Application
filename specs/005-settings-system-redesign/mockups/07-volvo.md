# Volvo Integration Settings - WinUI 3 Controls

**SVG File**: `07-volvo.svg`  
**Category**: Volvo Integration  
**Purpose**: EDI and master data synchronization configuration

---

## WinUI 3 Controls Used

### 1. **EDI Configuration**

#### Enable Volvo EDI - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Enable Volvo EDI"
    IsOn="{x:Bind ViewModel.VolvoEnableEDI, Mode=TwoWay}"/>
```

#### EDI File Drop Location - `TextBox` with Browse Button

```xml
<StackPanel Orientation="Horizontal" Spacing="8">
    <TextBox 
        Header="EDI File Drop Location"
        Text="{x:Bind ViewModel.VolvoEDIDropLocation, Mode=TwoWay}"
        Width="450"
        IsReadOnly="True"/>
    <Button 
        Content="..." 
        Command="{x:Bind ViewModel.BrowseFolderCommand}"
        VerticalAlignment="Bottom"
        Width="32"
        Height="32"/>
</StackPanel>
```

**Folder Picker Implementation**:

```csharp
[RelayCommand]
private async Task BrowseFolderAsync()
{
    var picker = new FolderPicker();
    picker.FileTypeFilter.Add("*");
    
    var folder = await picker.PickSingleFolderAsync();
    if (folder != null)
    {
        VolvoEDIDropLocation = folder.Path;
    }
}
```

#### Poll Interval - `NumberBox`

```xml
<NumberBox 
    Header="EDI Poll Interval (minutes)"
    Value="{x:Bind ViewModel.VolvoEDIPollIntervalMinutes, Mode=TwoWay}"
    Minimum="1"
    Maximum="60"
    SpinButtonPlacementMode="Inline"
    Width="150"/>
```

---

### 2. **Master Data Sync**

#### Auto-sync - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Auto-sync Master Data"
    IsOn="{x:Bind ViewModel.VolvoAutoSyncMasterData, Mode=TwoWay}"/>
```

#### Sync Schedule - `ComboBox`

```xml
<ComboBox 
    Header="Sync Schedule"
    SelectedItem="{x:Bind ViewModel.VolvoMasterDataSyncSchedule, Mode=TwoWay}"
    Width="250">
    <ComboBoxItem Content="Hourly"/>
    <ComboBoxItem Content="Daily at 02:00 AM" IsSelected="True"/>
    <ComboBoxItem Content="Weekly on Sunday"/>
    <ComboBoxItem Content="Manual Only"/>
</ComboBox>
```

---

### 3. **Last Sync Status - InfoBar**

```xml
<InfoBar 
    Severity="Success"
    IsOpen="True"
    IsClosable="False"
    Title="✓ Last Sync Successful"
    Message="{x:Bind ViewModel.LastSyncStatusMessage, Mode=OneWay}"
    Width="500"/>
```

**Alternative with Border**:

```xml
<Border 
    Background="#e8f5e9"
    BorderBrush="#4caf50"
    BorderThickness="1"
    CornerRadius="4"
    Padding="16"
    Width="500">
    <StackPanel Spacing="4">
        <TextBlock 
            Text="✓ Last Sync Successful"
            FontWeight="SemiBold"
            Foreground="#2e7d32"/>
        <TextBlock 
            Text="{x:Bind ViewModel.LastSyncStatusMessage, Mode=OneWay}"
            Foreground="#2e7d32"
            FontSize="11"/>
    </StackPanel>
</Border>
```

---

## ViewModel

```csharp
public partial class VolvoIntegrationViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool _volvoEnableEDI = true;
    
    [ObservableProperty]
    private string _volvoEDIDropLocation = @"\\FILESERVER\EDI\Volvo\Incoming";
    
    [ObservableProperty]
    private int _volvoEDIPollIntervalMinutes = 15;
    
    [ObservableProperty]
    private bool _volvoAutoSyncMasterData = true;
    
    [ObservableProperty]
    private string _volvoMasterDataSyncSchedule = "Daily at 02:00 AM";
    
    [ObservableProperty]
    private string _lastSyncStatusMessage = "January 10, 2026 02:00 AM - 1,247 parts updated";
}
```

---

## References

- [WinUI 3 Gallery - FolderPicker](https://learn.microsoft.com/en-us/windows/uwp/files/quickstart-using-file-and-folder-pickers)
- [WinUI 3 Gallery - InfoBar](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/InfoBarPage.xaml)
