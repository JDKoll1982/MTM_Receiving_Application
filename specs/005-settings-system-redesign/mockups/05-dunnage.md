# Dunnage Settings - WinUI 3 Controls

**SVG File**: `05-dunnage.svg`  
**Category**: Dunnage  
**Purpose**: Packing material tracking and inventory management configuration

---

## WinUI 3 Controls Used

### 1. **Tracking Options**

#### Enable Dunnage Tracking - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Enable Dunnage Tracking"
    IsOn="{x:Bind ViewModel.DunnageEnableTracking, Mode=TwoWay}"/>
```

#### Require Barcode Scan - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Require Barcode Scan"
    IsOn="{x:Bind ViewModel.DunnageRequireScan, Mode=TwoWay}"
    IsEnabled="{x:Bind ViewModel.DunnageEnableTracking, Mode=OneWay}"/>
```

#### Auto-calculate Costs - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Auto-calculate Costs"
    IsOn="{x:Bind ViewModel.DunnageAutoCalculateCosts, Mode=TwoWay}"/>
```

---

### 2. **Inventory Management**

#### Low Stock Threshold - `NumberBox`

```xml
<NumberBox 
    Header="Low Stock Threshold"
    Value="{x:Bind ViewModel.DunnageLowStockThreshold, Mode=TwoWay}"
    Minimum="1"
    Maximum="1000"
    SpinButtonPlacementMode="Inline"
    Width="150"/>
```

#### Email Alerts - `ToggleSwitch`

```xml
<ToggleSwitch 
    Header="Email Low Stock Alerts"
    IsOn="{x:Bind ViewModel.DunnageEmailAlerts, Mode=TwoWay}"/>
```

#### Alert Emails - `TextBox`

```xml
<TextBox 
    Header="Alert Email Addresses"
    Text="{x:Bind ViewModel.DunnageAlertEmails, Mode=TwoWay}"
    PlaceholderText="email1@example.com, email2@example.com"
    Width="350"
    IsEnabled="{x:Bind ViewModel.DunnageEmailAlerts, Mode=OneWay}"/>
```

---

### 3. **Info Card - InfoBar**

```xml
<InfoBar 
    Severity="Informational"
    IsOpen="True"
    IsClosable="False"
    Title="ℹ Material Types"
    Width="820">
    <InfoBar.Message>
        <StackPanel Spacing="4">
            <TextBlock TextWrapping="Wrap">
                Dunnage material types (cardboard, foam, bubble wrap, etc.) are managed in the main Dunnage module.
            </TextBlock>
            <TextBlock TextWrapping="Wrap">
                Navigate to Dunnage → Material Types to add, edit, or deactivate material types.
            </TextBlock>
        </StackPanel>
    </InfoBar.Message>
</InfoBar>
```

---

## ViewModel

```csharp
public partial class DunnageSettingsViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool _dunnageEnableTracking = true;
    
    [ObservableProperty]
    private bool _dunnageRequireScan = false;
    
    [ObservableProperty]
    private bool _dunnageAutoCalculateCosts = true;
    
    [ObservableProperty]
    private int _dunnageLowStockThreshold = 50;
    
    [ObservableProperty]
    private bool _dunnageEmailAlerts = true;
    
    [ObservableProperty]
    private string _dunnageAlertEmails = "purchasing@mtm.com, warehouse@mtm.com";
}
```

---

## Validation

```csharp
partial void OnDunnageAlertEmailsChanged(string value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        ValidationErrors["DunnageAlertEmails"] = "At least one email required";
        return;
    }
    
    var emails = value.Split(',').Select(e => e.Trim());
    var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    
    foreach (var email in emails)
    {
        if (!emailRegex.IsMatch(email))
        {
            ValidationErrors["DunnageAlertEmails"] = $"Invalid email: {email}";
            return;
        }
    }
    
    ValidationErrors.Remove("DunnageAlertEmails");
}
```

---

## Permissions

| Setting | User | Operator | Admin | Developer | Super Admin |
|---------|------|----------|-------|-----------|-------------|
| Enable Tracking | ❌ | ❌ | ✅ | ✅ | ✅ |
| Require Scan | ❌ | ✅ | ✅ | ✅ | ✅ |
| Auto-calculate | ❌ | ✅ | ✅ | ✅ | ✅ |
| Low Stock Threshold | ❌ | ✅ | ✅ | ✅ | ✅ |
| Email Alerts | ❌ | ❌ | ✅ | ✅ | ✅ |
| Alert Emails | ❌ | ❌ | ✅ | ✅ | ✅ |

---

## References

- [WinUI 3 Gallery - InfoBar](https://github.com/microsoft/WinUI-Gallery/blob/main/WinUIGallery/Samples/ControlPages/InfoBarPage.xaml)
